//! Copias de archivos con progreso: importar a la biblioteca, "Copiar a…" y "Mover a…".
//! Se hacen de una en una (varias copias a la vez al mismo disco van más lentas, no más rápido).

use crate::{AppState, db, library};
use anyhow::{Result, bail};
use serde::Serialize;
use std::io::{Read, Write};
use std::path::{Path, PathBuf};
use std::sync::atomic::{AtomicBool, AtomicU64, Ordering};
use std::sync::{Arc, Mutex};
use std::time::Duration;
use tauri::{AppHandle, Emitter, Manager};

/// Archivos que acompañan a un vídeo y viajan con él.
const COMPANION_EXT: &[&str] = &["srt", "ass", "ssa", "sub", "idx", "vtt", "nfo"];
const BUFFER: usize = 8 * 1024 * 1024;

#[derive(Serialize, Clone, Copy, PartialEq)]
#[serde(rename_all = "camelCase")]
pub enum Kind {
    Import,
    Copy,
    Move,
}

/// Qué hacer si ya hay un archivo con ese nombre en el destino.
#[derive(serde::Deserialize, Clone, Copy, PartialEq, Default)]
#[serde(rename_all = "camelCase")]
pub enum Conflict {
    #[default]
    Ask,
    /// Guardar como "Nombre (2).ext".
    Rename,
    /// Mandar el existente a la papelera y poner el nuevo en su lugar.
    Replace,
}

#[derive(Serialize, Clone, Copy, PartialEq)]
#[serde(rename_all = "camelCase")]
pub enum State {
    Queued,
    Running,
    Done,
    Error,
    Cancelled,
}

#[derive(Serialize, Clone)]
#[serde(rename_all = "camelCase")]
pub struct TransferInfo {
    pub id: u64,
    pub kind: Kind,
    pub name: String,
    pub source: String,
    pub dest: String,
    pub total: u64,
    pub done: u64,
    pub state: State,
    pub error: Option<String>,
}

struct Job {
    info: TransferInfo,
    /// Archivo de la biblioteca que se copia o mueve (para actualizar la base de datos).
    file_id: Option<i64>,
    delete_source: bool,
    cancel: Arc<AtomicBool>,
    progress: Arc<AtomicU64>,
}

impl Job {
    fn is_active(&self) -> bool {
        matches!(self.info.state, State::Queued | State::Running)
    }
}

#[derive(Default)]
pub struct Transfers {
    jobs: Mutex<Vec<Job>>,
    next_id: AtomicU64,
    running: AtomicBool,
}

impl Transfers {
    fn snapshot(&self) -> Vec<TransferInfo> {
        self.jobs
            .lock()
            .unwrap()
            .iter()
            .map(|j| TransferInfo { done: j.progress.load(Ordering::Relaxed), ..j.info.clone() })
            .collect()
    }
}

fn emit(app: &AppHandle) {
    let _ = app.emit("transfers", app.state::<AppState>().transfers.snapshot());
}

pub fn list(app: &AppHandle) -> Vec<TransferInfo> {
    app.state::<AppState>().transfers.snapshot()
}

/// Encola una transferencia. `dest_dir` es la carpeta de destino; el nombre se conserva.
pub fn enqueue(
    app: &AppHandle,
    kind: Kind,
    source: PathBuf,
    dest_dir: &Path,
    file_id: Option<i64>,
    delete_source: bool,
    conflict: Conflict,
) -> Result<()> {
    if !source.is_file() {
        bail!("file_missing");
    }
    let mut dest = dest_dir.join(source.file_name().unwrap_or_default());
    if same_path(&source, &dest) {
        bail!("same_folder");
    }
    let transfers = &app.state::<AppState>().transfers;
    let taken = |p: &Path| p.exists() || transfers.jobs.lock().unwrap().iter().any(|j| j.is_active() && same_path(Path::new(&j.info.dest), p));
    if taken(&dest) {
        match conflict {
            // La interfaz pregunta qué hacer y vuelve a llamar con la respuesta.
            Conflict::Ask => bail!("exists"),
            Conflict::Rename => dest = free_name(&dest, &taken),
            Conflict::Replace if dest.exists() => replace_existing(app, &dest)?,
            Conflict::Replace => bail!("exists"),
        }
    }
    let name = dest.file_name().map(|n| n.to_string_lossy().into_owned()).unwrap_or_default();
    let total = std::fs::metadata(&source)?.len();
    let id = transfers.next_id.fetch_add(1, Ordering::Relaxed) + 1;
    transfers.jobs.lock().unwrap().push(Job {
        info: TransferInfo {
            id,
            kind,
            name,
            source: source.to_string_lossy().into_owned(),
            dest: dest.to_string_lossy().into_owned(),
            total,
            done: 0,
            state: State::Queued,
            error: None,
        },
        file_id,
        delete_source,
        cancel: Arc::new(AtomicBool::new(false)),
        progress: Arc::new(AtomicU64::new(0)),
    });
    emit(app);
    if !transfers.running.swap(true, Ordering::SeqCst) {
        let app = app.clone();
        tauri::async_runtime::spawn(async move { worker(app).await });
    }
    Ok(())
}

pub fn cancel(app: &AppHandle, id: u64) {
    let transfers = &app.state::<AppState>().transfers;
    let mut jobs = transfers.jobs.lock().unwrap();
    if let Some(job) = jobs.iter_mut().find(|j| j.info.id == id) {
        job.cancel.store(true, Ordering::Relaxed);
        if job.info.state == State::Queued {
            job.info.state = State::Cancelled;
        }
    }
    drop(jobs);
    emit(app);
}

/// Quita de la lista las transferencias terminadas (y las fallidas, que se quedan hasta entonces).
pub fn dismiss(app: &AppHandle, id: Option<u64>) {
    let transfers = &app.state::<AppState>().transfers;
    transfers
        .jobs
        .lock()
        .unwrap()
        .retain(|j| matches!(j.info.state, State::Queued | State::Running) || id.is_some_and(|id| j.info.id != id));
    emit(app);
}

async fn worker(app: AppHandle) {
    let mut library_touched = false;
    loop {
        let next = {
            let transfers = &app.state::<AppState>().transfers;
            let mut jobs = transfers.jobs.lock().unwrap();
            jobs.iter_mut().find(|j| j.info.state == State::Queued).map(|j| {
                j.info.state = State::Running;
                (j.info.clone(), j.file_id, j.delete_source, j.cancel.clone(), j.progress.clone())
            })
        };
        let Some((info, file_id, delete_source, cancel, progress)) = next else { break };
        emit(&app);

        let (src, dest) = (PathBuf::from(&info.source), PathBuf::from(&info.dest));
        let kind = info.kind;
        let task = tauri::async_runtime::spawn_blocking(move || run(kind, &src, &dest, delete_source, &cancel, &progress));
        tokio::pin!(task);
        let result = loop {
            tokio::select! {
                r = &mut task => break r.map_err(anyhow::Error::from).and_then(|r| r),
                _ = tokio::time::sleep(Duration::from_millis(300)) => emit(&app),
            }
        };

        let (state, error) = match result {
            Ok(()) => {
                library_touched |= after_success(&app, &info, file_id).unwrap_or(false);
                library::notice(&app, "transferDone", info.name.clone(), None, Some(info.dest.clone()));
                (State::Done, None)
            }
            Err(e) if e.to_string() == "cancelled" => (State::Cancelled, None),
            Err(e) => {
                library::notice(&app, "transferError", info.name.clone(), None, Some(e.to_string()));
                (State::Error, Some(e.to_string()))
            }
        };
        {
            let transfers = &app.state::<AppState>().transfers;
            if let Some(job) = transfers.jobs.lock().unwrap().iter_mut().find(|j| j.info.id == info.id) {
                job.info.state = state;
                job.info.error = error;
            }
        }
        emit(&app);
    }
    app.state::<AppState>().transfers.running.store(false, Ordering::SeqCst);
    if library_touched {
        crate::spawn_scan(&app);
    }
    // Las terminadas se ven un momento con su marca y luego desaparecen.
    tokio::time::sleep(Duration::from_secs(4)).await;
    let transfers = &app.state::<AppState>().transfers;
    if !transfers.running.load(Ordering::SeqCst) {
        transfers.jobs.lock().unwrap().retain(|j| j.info.state != State::Done && j.info.state != State::Cancelled);
        emit(&app);
    }
}

/// Actualiza la base de datos tras copiar o mover. Devuelve `true` si hace falta reescanear.
fn after_success(app: &AppHandle, info: &TransferInfo, file_id: Option<i64>) -> Result<bool> {
    let state = app.state::<AppState>();
    let conn = state.db();
    let library = db::libraries(&conn)?
        .into_iter()
        .find(|l| Path::new(&info.dest).starts_with(&l.path) || info.dest.to_lowercase().starts_with(&(l.path.to_lowercase() + "\\")));
    match (info.kind, file_id) {
        // Un archivo de la biblioteca cambia de sitio: se conserva su película (aunque se
        // hubiera identificado a mano). Si sale de las carpetas de la biblioteca, deja de estar.
        (Kind::Move, Some(id)) => {
            match &library {
                Some(lib) => conn.execute(
                    "UPDATE files SET path = ?2, library_id = ?3 WHERE id = ?1",
                    rusqlite::params![id, info.dest, lib.id],
                )?,
                None => conn.execute("DELETE FROM files WHERE id = ?1", [id])?,
            };
            db::prune_orphans(&conn)?;
            let _ = app.emit("library-changed", ());
            Ok(false)
        }
        // Copia de un archivo de la biblioteca a otra carpeta de la biblioteca: misma película.
        (Kind::Copy, Some(id)) => {
            if let Some(lib) = &library {
                conn.execute(
                    "INSERT OR IGNORE INTO files (item_id, library_id, path, size, mtime, parsed_title, parsed_year, hint_imdb, hint_tmdb, has_subs, added_at)
                     SELECT item_id, ?3, ?2, size, mtime, parsed_title, parsed_year, hint_imdb, hint_tmdb, has_subs, datetime('now')
                     FROM files WHERE id = ?1",
                    rusqlite::params![id, info.dest, lib.id],
                )?;
                let _ = app.emit("library-changed", ());
            }
            Ok(false)
        }
        // Importación: el escaneo la detecta e identifica.
        _ => Ok(library.is_some()),
    }
}

/// "Patata.mp4" → "Patata (2).mp4", "Patata (3).mp4"… el primero libre.
fn free_name(dest: &Path, taken: &dyn Fn(&Path) -> bool) -> PathBuf {
    let stem = dest.file_stem().unwrap_or_default().to_string_lossy();
    let ext = dest.extension().map(|e| format!(".{}", e.to_string_lossy())).unwrap_or_default();
    (2..)
        .map(|n| dest.with_file_name(format!("{stem} ({n}){ext}")))
        .find(|p| !taken(p))
        .unwrap()
}

/// "Reemplazar": el archivo que había va a la papelera y sale de la biblioteca.
fn replace_existing(app: &AppHandle, dest: &Path) -> Result<()> {
    delete_to_trash(dest)?;
    let state = app.state::<AppState>();
    let conn = state.db();
    conn.execute("DELETE FROM files WHERE path = ?1", [dest.to_string_lossy()])?;
    db::prune_orphans(&conn)?;
    let _ = app.emit("library-changed", ());
    Ok(())
}

/// Destino de un subtítulo: mismo sufijo, con el nombre nuevo del vídeo
/// ("Patata.es.srt" → "Patata (2).es.srt"). `None` si ya existe.
fn companion_dest(src: &Path, extra: &Path, dest: &Path) -> Option<PathBuf> {
    let src_stem = src.file_stem()?.to_string_lossy().chars().count();
    let rest: String = extra.file_name()?.to_string_lossy().chars().skip(src_stem).collect();
    let to = dest.with_file_name(format!("{}{rest}", dest.file_stem()?.to_string_lossy()));
    (!to.exists()).then_some(to)
}

fn same_path(a: &Path, b: &Path) -> bool {
    a.to_string_lossy().eq_ignore_ascii_case(&b.to_string_lossy())
}

/// Subtítulos y .nfo con el mismo nombre que el vídeo ("Peli.srt", "Peli.es.srt"…).
fn companions(video: &Path) -> Vec<PathBuf> {
    let (Some(dir), Some(stem)) = (video.parent(), video.file_stem()) else { return vec![] };
    let stem = stem.to_string_lossy().to_lowercase();
    let Ok(entries) = std::fs::read_dir(dir) else { return vec![] };
    entries
        .flatten()
        .map(|e| e.path())
        .filter(|p| {
            let name = p.file_name().map(|n| n.to_string_lossy().to_lowercase()).unwrap_or_default();
            let ext = p.extension().map(|e| e.to_string_lossy().to_lowercase()).unwrap_or_default();
            p.is_file() && COMPANION_EXT.contains(&ext.as_str()) && name.strip_prefix(&stem).is_some_and(|r| r.starts_with('.'))
        })
        .collect()
}

fn run(kind: Kind, src: &Path, dest: &Path, delete_source: bool, cancel: &AtomicBool, progress: &AtomicU64) -> Result<()> {
    if dest.exists() {
        bail!("exists");
    }
    if let Some(dir) = dest.parent() {
        std::fs::create_dir_all(dir)?;
    }
    let extras = companions(src);
    let remove_source = kind == Kind::Move || (kind == Kind::Import && delete_source);

    // Mover dentro del mismo disco es instantáneo.
    if remove_source && std::fs::rename(src, dest).is_ok() {
        progress.store(std::fs::metadata(dest).map(|m| m.len()).unwrap_or(0), Ordering::Relaxed);
        for extra in &extras {
            if let Some(to) = companion_dest(src, extra, dest) {
                let _ = std::fs::rename(extra, to);
            }
        }
        return Ok(());
    }

    let part = dest.with_file_name(format!("{}.kmpart", dest.file_name().unwrap().to_string_lossy()));
    let copied = (|| -> Result<()> {
        let mut input = std::fs::File::open(src)?;
        let mut output = std::fs::File::create(&part)?;
        let mut buf = vec![0u8; BUFFER];
        loop {
            if cancel.load(Ordering::Relaxed) {
                bail!("cancelled");
            }
            let n = input.read(&mut buf)?;
            if n == 0 {
                break;
            }
            output.write_all(&buf[..n])?;
            progress.fetch_add(n as u64, Ordering::Relaxed);
        }
        output.sync_all()?;
        Ok(())
    })();
    if let Err(e) = copied {
        let _ = std::fs::remove_file(&part);
        return Err(e);
    }
    std::fs::rename(&part, dest)?;
    for extra in &extras {
        if let Some(to) = companion_dest(src, extra, dest) {
            let _ = std::fs::copy(extra, to);
        }
    }
    if remove_source {
        std::fs::remove_file(src)?;
        for extra in &extras {
            let _ = std::fs::remove_file(extra);
        }
    }
    Ok(())
}

/// Manda un archivo de la biblioteca (y sus subtítulos) a la papelera de reciclaje.
pub fn delete_to_trash(path: &Path) -> Result<()> {
    let mut all = vec![path.to_path_buf()];
    all.extend(companions(path));
    trash::delete_all(&all)?;
    Ok(())
}

#[cfg(test)]
mod tests {
    use super::*;

    fn tmp(name: &str) -> PathBuf {
        let dir = std::env::temp_dir().join(format!("kmovies-test-{name}-{}", std::process::id()));
        let _ = std::fs::remove_dir_all(&dir);
        std::fs::create_dir_all(dir.join("src")).unwrap();
        std::fs::create_dir_all(dir.join("dst")).unwrap();
        dir
    }

    fn run_simple(kind: Kind, src: &Path, dest: &Path, delete: bool) -> Result<()> {
        run(kind, src, dest, delete, &AtomicBool::new(false), &AtomicU64::new(0))
    }

    #[test]
    fn copy_keeps_source_and_brings_subtitles() {
        let dir = tmp("copy");
        let src = dir.join("src/Peli (2010).mkv");
        std::fs::write(&src, vec![7u8; 10 * 1024 * 1024 + 3]).unwrap();
        std::fs::write(dir.join("src/Peli (2010).es.srt"), "1").unwrap();
        std::fs::write(dir.join("src/Peli (2010) 2.srt"), "otra peli").unwrap();
        let dest = dir.join("dst/Peli (2010).mkv");
        run_simple(Kind::Copy, &src, &dest, false).unwrap();
        assert!(src.exists());
        assert_eq!(std::fs::metadata(&dest).unwrap().len(), 10 * 1024 * 1024 + 3);
        assert!(dir.join("dst/Peli (2010).es.srt").exists());
        assert!(!dir.join("dst/Peli (2010) 2.srt").exists());
        assert!(!dir.join("dst/Peli (2010).mkv.kmpart").exists());
        // No sobrescribe.
        assert_eq!(run_simple(Kind::Copy, &src, &dest, false).unwrap_err().to_string(), "exists");
    }

    #[test]
    fn move_and_import_remove_source() {
        let dir = tmp("move");
        let src = dir.join("src/A.mp4");
        std::fs::write(&src, b"video").unwrap();
        std::fs::write(dir.join("src/A.srt"), b"sub").unwrap();
        run_simple(Kind::Move, &src, &dir.join("dst/A.mp4"), false).unwrap();
        assert!(!src.exists() && !dir.join("src/A.srt").exists());
        assert!(dir.join("dst/A.mp4").exists() && dir.join("dst/A.srt").exists());

        let src = dir.join("src/B.mp4");
        std::fs::write(&src, b"video").unwrap();
        run_simple(Kind::Import, &src, &dir.join("dst/B.mp4"), false).unwrap();
        assert!(src.exists(), "importar sin borrar el original lo conserva");
    }

    #[test]
    fn cancel_leaves_nothing_behind() {
        let dir = tmp("cancel");
        let src = dir.join("src/C.mkv");
        std::fs::write(&src, b"video").unwrap();
        let dest = dir.join("dst/C.mkv");
        let err = run(Kind::Copy, &src, &dest, false, &AtomicBool::new(true), &AtomicU64::new(0)).unwrap_err();
        assert_eq!(err.to_string(), "cancelled");
        assert!(src.exists());
        assert!(std::fs::read_dir(dir.join("dst")).unwrap().next().is_none());
    }
}

#[cfg(test)]
mod name_tests {
    use super::*;

    #[test]
    fn repeated_names() {
        let dir = std::env::temp_dir().join(format!("kmovies-test-names-{}", std::process::id()));
        let _ = std::fs::remove_dir_all(&dir);
        std::fs::create_dir_all(&dir).unwrap();
        std::fs::write(dir.join("Patata.mp4"), b"1").unwrap();
        std::fs::write(dir.join("Patata (2).mp4"), b"2").unwrap();
        let free = free_name(&dir.join("Patata.mp4"), &|p: &Path| p.exists());
        assert_eq!(free.file_name().unwrap(), "Patata (3).mp4");

        // El subtítulo sigue al vídeo renombrado y no pisa el de la otra película.
        let src = Path::new("C:/inbox/Patata.mp4");
        let to = companion_dest(src, Path::new("C:/inbox/Patata.es.srt"), &dir.join("Patata (3).mp4")).unwrap();
        assert_eq!(to.file_name().unwrap(), "Patata (3).es.srt");
        std::fs::write(dir.join("Patata.srt"), b"s").unwrap();
        assert!(companion_dest(src, Path::new("C:/inbox/Patata.srt"), &dir.join("Patata.mp4")).is_none());
    }
}
