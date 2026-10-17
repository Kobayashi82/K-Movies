//! Vigila las carpetas de la biblioteca: si aparece, desaparece o se renombra un vídeo (o sus
//! subtítulos o su `.nfo`), se reescanea solo. Se espera a que los cambios lleven un rato quietos,
//! porque una copia larga avisa muchas veces mientras dura.
//!
//! Las carpetas que no están (un disco de red apagado) se vuelven a mirar cada minuto; cuando
//! vuelven, se empiezan a vigilar y se reescanea.

use crate::{AppState, db, scanner};
use notify::{Event, EventKind, RecommendedWatcher, RecursiveMode, Watcher as _};
use std::collections::HashSet;
use std::path::{Path, PathBuf};
use std::sync::Mutex;
use std::sync::mpsc::{self, RecvTimeoutError};
use std::time::{Duration, Instant};
use tauri::{AppHandle, Manager};

/// Tiempo sin cambios antes de reescanear.
const QUIET: Duration = Duration::from_secs(8);
/// Cada cuánto se miran las carpetas que no estaban disponibles.
const RECHECK: Duration = Duration::from_secs(60);

#[derive(Default)]
pub struct Watch {
    watcher: Mutex<Option<RecommendedWatcher>>,
    /// Carpetas vigiladas ahora mismo.
    roots: Mutex<HashSet<PathBuf>>,
}

/// ¿Este cambio puede afectar a la biblioteca? Vídeos, subtítulos, `.nfo` y `.txt` del 4.0, y
/// rutas sin extensión (casi siempre carpetas: renombrar o borrar una carpeta solo avisa de ella).
/// No cuentan las copias a medias de K-Movies (`.kmpart`): al terminar se renombran y eso sí avisa.
fn relevant(event: &Event) -> bool {
    if matches!(event.kind, EventKind::Access(_)) {
        return false;
    }
    event.paths.iter().any(|p| match p.extension().and_then(|e| e.to_str()).map(str::to_lowercase) {
        None => true,
        Some(ext) => {
            scanner::VIDEO_EXT.contains(&ext.as_str()) || scanner::SUB_EXT.contains(&ext.as_str()) || ext == "nfo" || ext == "txt"
        }
    })
}

/// Empieza a vigilar (al arrancar la app). El primer escaneo lo lanza la interfaz al abrirse.
pub fn start(app: &AppHandle) {
    let (tx, rx) = mpsc::channel();
    match notify::recommended_watcher(tx) {
        Ok(w) => *app.state::<AppState>().watch.watcher.lock().unwrap_or_else(|e| e.into_inner()) = Some(w),
        Err(e) => {
            eprintln!("[watch] {e}");
            return;
        }
    }
    refresh(app);
    let app = app.clone();
    std::thread::spawn(move || {
        let mut due: Option<Instant> = None;
        let mut checked = Instant::now();
        loop {
            let wait = due.map_or(RECHECK, |d| d.saturating_duration_since(Instant::now())).min(RECHECK);
            match rx.recv_timeout(wait) {
                Ok(Ok(event)) if relevant(&event) => due = Some(Instant::now() + QUIET),
                Ok(Ok(_)) => {}
                Ok(Err(e)) => eprintln!("[watch] {e}"),
                Err(RecvTimeoutError::Timeout) => {}
                Err(RecvTimeoutError::Disconnected) => break,
            }
            if checked.elapsed() >= RECHECK {
                checked = Instant::now();
                if refresh(&app) > 0 {
                    due = Some(Instant::now());
                }
            }
            if due.is_some_and(|d| Instant::now() >= d) {
                // Si ya hay un escaneo (o una importación) en marcha, se reintenta en un momento:
                // ese escaneo pudo pasar por la carpeta antes del cambio.
                due = if crate::spawn_scan(&app) { None } else { Some(Instant::now() + Duration::from_secs(5)) };
            }
        }
    });
}

/// Ajusta lo vigilado a las carpetas de la biblioteca (tras añadir o quitar una, y cada minuto).
/// Devuelve cuántas carpetas que antes no estaban disponibles se vigilan ahora.
pub fn refresh(app: &AppHandle) -> usize {
    let state = app.state::<AppState>();
    let wanted: HashSet<PathBuf> = match db::libraries(&state.db()) {
        Ok(libs) => libs.into_iter().map(|l| PathBuf::from(l.path)).collect(),
        Err(e) => {
            eprintln!("[watch] {e:#}");
            return 0;
        }
    };
    let mut guard = state.watch.watcher.lock().unwrap_or_else(|e| e.into_inner());
    let Some(watcher) = guard.as_mut() else { return 0 };
    let mut roots = state.watch.roots.lock().unwrap_or_else(|e| e.into_inner());
    // Fuera las que se quitaron de la biblioteca y las que han desaparecido (disco desconectado).
    roots.retain(|root| {
        let keep = wanted.contains(root) && root.is_dir();
        if !keep {
            let _ = watcher.unwatch(root);
        }
        keep
    });
    let mut added = 0;
    for root in wanted {
        if roots.contains(&root) || !Path::new(&root).is_dir() {
            continue;
        }
        match watcher.watch(&root, RecursiveMode::Recursive) {
            Ok(()) => {
                roots.insert(root);
                added += 1;
            }
            Err(e) => eprintln!("[watch] {}: {e}", root.display()),
        }
    }
    added
}

#[cfg(test)]
mod tests {
    use super::*;
    use notify::event::{CreateKind, ModifyKind, RemoveKind};

    fn event(kind: EventKind, path: &str) -> Event {
        Event::new(kind).add_path(PathBuf::from(path))
    }

    #[test]
    fn only_library_files_count() {
        assert!(relevant(&event(EventKind::Create(CreateKind::Any), r"V:\Peliculas\Heat (1995).mkv")));
        assert!(relevant(&event(EventKind::Remove(RemoveKind::Any), r"V:\Peliculas\Heat (1995).es.srt")));
        assert!(relevant(&event(EventKind::Modify(ModifyKind::Any), r"V:\Peliculas\Heat (1995)")));
        assert!(!relevant(&event(EventKind::Create(CreateKind::Any), r"V:\Peliculas\Heat (1995).mkv.kmpart")));
        assert!(!relevant(&event(EventKind::Create(CreateKind::Any), r"V:\Peliculas\Thumbs.db")));
    }

    /// Windows avisa de verdad de un vídeo nuevo en una subcarpeta y de su borrado.
    #[test]
    fn real_folder_changes_arrive() {
        let dir = std::env::temp_dir().join(format!("kmovies-watch-{}", std::process::id()));
        std::fs::create_dir_all(dir.join("Heat (1995)")).unwrap();
        let (tx, rx) = mpsc::channel();
        let mut watcher = notify::recommended_watcher(tx).unwrap();
        watcher.watch(&dir, RecursiveMode::Recursive).unwrap();
        let wait_relevant = || loop {
            match rx.recv_timeout(Duration::from_secs(3)) {
                Ok(Ok(e)) if relevant(&e) => return true,
                Ok(_) => {}
                Err(_) => return false,
            }
        };
        let video = dir.join("Heat (1995)").join("Heat (1995).mkv");
        std::fs::write(&video, b"x").unwrap();
        assert!(wait_relevant(), "sin aviso al crear");
        std::fs::remove_file(&video).unwrap();
        assert!(wait_relevant(), "sin aviso al borrar");
        drop(watcher);
        std::fs::remove_dir_all(&dir).unwrap();
    }
}
