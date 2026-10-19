//! Seguimiento de lo que se está viendo: abre el reproductor (el integrado o uno externo), sabe por
//! dónde vas, guarda el progreso y marca la película como vista al pasar del umbral.

use crate::{AppState, db, library, player, players, tray};
use anyhow::{Result, anyhow, bail};
use rusqlite::OptionalExtension;
use serde::Serialize;
use std::path::PathBuf;
use std::sync::Mutex;
use std::time::{Duration, Instant};
use tauri::{AppHandle, Emitter, Manager};

const POLL: Duration = Duration::from_secs(2);
/// Cada cuánto se guarda la posición mientras se ve (por si algo se cierra mal).
const SAVE_EVERY: Duration = Duration::from_secs(15);
/// Por debajo de esto no se guarda la posición: abrir la película y cerrarla no cuenta.
const MIN_POSITION: f64 = 60.0;
/// Si no se ha podido seguir el progreso, a partir de cuánto tiempo abierta se pregunta "¿La terminaste?".
const ASK_AFTER: Duration = Duration::from_secs(20 * 60);

/// Valor del ajuste `playerPath` para el reproductor integrado.
pub const INTERNAL: &str = "internal";

/// Lo que se está reproduciendo ahora (se manda entero a la interfaz en el evento `playback`).
#[derive(Serialize, Clone)]
#[serde(rename_all = "camelCase")]
pub struct NowPlaying {
    pub item_id: i64,
    pub title: String,
    pub player: String,
    /// Se puede saber por dónde va (integrado y VLC). Con los demás solo se sabe que está abierto.
    pub tracked: bool,
    pub position: Option<f64>,
    pub duration: Option<f64>,
    pub paused: bool,
    /// Ya se ha marcado como vista en esta sesión.
    pub watched: bool,
    /// Ventana del reproductor integrado (para traerla al frente); `None` con reproductores externos.
    pub window: Option<String>,
}

#[derive(Default)]
pub struct Sessions(Mutex<Vec<NowPlaying>>);

impl Sessions {
    fn lock(&self) -> std::sync::MutexGuard<'_, Vec<NowPlaying>> {
        self.0.lock().unwrap_or_else(|e| e.into_inner())
    }
}

pub fn list(app: &AppHandle) -> Vec<NowPlaying> {
    app.state::<AppState>().sessions.lock().clone()
}

fn changed(app: &AppHandle) {
    let _ = app.emit("playback", list(app));
}

fn update(app: &AppHandle, item_id: i64, f: impl FnOnce(&mut NowPlaying)) {
    if let Some(s) = app.state::<AppState>().sessions.lock().iter_mut().find(|s| s.item_id == item_id) {
        f(s);
    }
    changed(app);
}

/// Umbral para dar una película por vista (ajuste `watchedThreshold`, en %; 90 por defecto).
fn threshold(app: &AppHandle) -> f64 {
    let value = db::setting(&app.state::<AppState>().db(), "watchedThreshold").ok().flatten();
    value.and_then(|v| v.parse::<f64>().ok()).map_or(90.0, |v| v.clamp(50.0, 99.0)) / 100.0
}

/// Qué se va a reproducir y desde dónde.
pub struct Resolved {
    pub item_id: i64,
    pub file_id: i64,
    pub file: PathBuf,
    pub start: Option<f64>,
    pub title: String,
    /// Ajuste `playerPath`: vacío (automático), `internal` o la ruta de un reproductor.
    player: Option<String>,
}

/// Por defecto continúa donde se quedó; `from_start` empieza de cero. Sin `file_id` usa el
/// archivo con el que se empezó a ver o, si no, el más grande.
fn resolve(app: &AppHandle, item_id: i64, file_id: Option<i64>, from_start: bool) -> Result<Resolved> {
    let state = app.state::<AppState>();
    let conn = state.db();
    let resume: Option<(Option<i64>, f64)> = conn
        .query_row("SELECT file_id, position FROM playback WHERE item_id = ?1", [item_id], |r| {
            Ok((r.get(0)?, r.get(1)?))
        })
        .optional()?;
    let wanted = file_id.or(resume.and_then(|r| r.0));
    let chosen: Option<(i64, String)> = match wanted {
        Some(id) => conn
            .query_row("SELECT id, path FROM files WHERE id = ?1 AND item_id = ?2", [id, item_id], |r| {
                Ok((r.get(0)?, r.get(1)?))
            })
            .optional()?,
        None => None,
    };
    let (file_id, file) = match chosen {
        Some(f) => f,
        None => db::largest_file(&conn, item_id)?.ok_or_else(|| anyhow!("file_missing"))?,
    };
    let file = PathBuf::from(file);
    if !file.exists() {
        bail!("file_missing");
    }
    Ok(Resolved {
        item_id,
        file_id,
        file,
        // La posición vale aunque se elija otra versión del archivo: es la misma película.
        start: if from_start { None } else { resume.map(|r| r.1) },
        title: conn.query_row("SELECT title FROM items WHERE id = ?1", [item_id], |r| r.get(0))?,
        player: db::setting(&conn, "playerPath")?.filter(|p| !p.is_empty()),
    })
}

/// Abre la película en el reproductor de los ajustes: el integrado (si está libmpv), VLC u otro.
/// Se pueden ver varias películas a la vez; si esa ya se está viendo en el integrado, se trae su
/// ventana al frente.
pub fn play(app: &AppHandle, item_id: i64, file_id: Option<i64>, from_start: bool) -> Result<()> {
    let window = app.state::<AppState>().sessions.lock().iter().find(|s| s.item_id == item_id).map(|s| s.window.clone());
    match window {
        Some(Some(label)) => {
            player::focus(app, &label);
            return Ok(());
        }
        Some(None) => bail!("already_playing"),
        None => {}
    }
    let r = resolve(app, item_id, file_id, from_start)?;
    let internal = match r.player.as_deref() {
        Some(INTERNAL) => true,
        None => crate::mpv::available(),
        Some(_) => false,
    };
    if internal {
        player::open(app, r)?;
        tray::playback_started(app);
        return Ok(());
    }
    let path = r.player.as_ref().map(PathBuf::from).filter(|p| p.is_file());
    let Some(path) = path.or_else(|| players::detect().into_iter().next().map(|p| PathBuf::from(p.path))) else {
        // Sin reproductor configurado ni detectado: el predeterminado de Windows, sin seguimiento.
        return tauri_plugin_opener::open_path(&r.file, None::<&str>).map_err(|e| anyhow!(e.to_string()));
    };
    let launched = players::launch(&path, &r.file, r.start)?;
    let tracker = Tracker::begin(app, &r, players::name_of(&path), launched.vlc.is_some(), None);
    let name = r.file.file_name().unwrap_or_default().to_string_lossy().into_owned();
    tauri::async_runtime::spawn(async move { track_external(tracker, name, launched).await });
    tray::playback_started(app);
    Ok(())
}

/// Reproductor externo: con VLC se le pregunta por dónde va; con los demás solo se espera a que se cierre.
async fn track_external(mut tracker: Tracker, file_name: String, mut launched: players::Launched) {
    loop {
        tokio::time::sleep(POLL).await;
        if !matches!(launched.child.try_wait(), Ok(None)) {
            break;
        }
        let Some(vlc) = &launched.vlc else { continue };
        // Mientras VLC arranca, la interfaz HTTP aún no responde: se reintenta en la siguiente vuelta.
        let Ok(st) = vlc.status().await else { continue };
        // Solo cuenta nuestra película: parada (al acabar vuelve a 0) u otro archivo abierto en ese VLC, no.
        if !matches!(st.state.as_str(), "playing" | "paused") || !st.is_file(&file_name) {
            continue;
        }
        tracker.status(st.time, st.length, st.state == "paused");
    }
    tracker.finish();
}

/// Lleva la cuenta de una sesión de reproducción, venga del reproductor que venga.
pub struct Tracker {
    app: AppHandle,
    item_id: i64,
    file_id: i64,
    threshold: f64,
    last: Option<(f64, f64)>, // posición y duración
    paused: bool,
    watched: bool,
    saved_at: Instant,
    started: Instant,
}

impl Tracker {
    /// Apunta la sesión (aparece en la interfaz como "reproduciendo").
    pub fn begin(app: &AppHandle, r: &Resolved, player: String, tracked: bool, window: Option<String>) -> Self {
        app.state::<AppState>().sessions.lock().push(NowPlaying {
            item_id: r.item_id,
            title: r.title.clone(),
            player,
            tracked,
            position: r.start,
            duration: None,
            paused: false,
            watched: false,
            window,
        });
        changed(app);
        Tracker {
            app: app.clone(),
            item_id: r.item_id,
            file_id: r.file_id,
            threshold: threshold(app),
            last: None,
            paused: false,
            watched: false,
            saved_at: Instant::now(),
            started: Instant::now(),
        }
    }

    /// Estado leído del reproductor (en segundos).
    pub fn status(&mut self, position: f64, duration: f64, paused: bool) {
        if duration <= 0.0 {
            return;
        }
        self.last = Some((position, duration));
        if !self.watched && position / duration >= self.threshold {
            self.watched = true;
            mark_watched(&self.app, self.item_id);
        } else if !self.watched
            && position >= MIN_POSITION
            && (self.saved_at.elapsed() >= SAVE_EVERY || paused != self.paused)
        {
            save(&self.app, self.item_id, self.file_id, position, duration);
            self.saved_at = Instant::now();
        }
        self.paused = paused;
        let watched = self.watched;
        update(&self.app, self.item_id, |s| {
            s.position = Some(position);
            s.duration = Some(duration);
            s.paused = paused;
            s.watched = watched;
        });
    }

    /// El reproductor se ha cerrado: se guarda dónde se quedó (o se marca como vista).
    pub fn finish(self) {
        let app = &self.app;
        match self.last {
            Some((pos, len)) if !self.watched && pos / len >= self.threshold => mark_watched(app, self.item_id),
            Some((pos, len)) if !self.watched && pos >= MIN_POSITION => save(app, self.item_id, self.file_id, pos, len),
            None if self.started.elapsed() >= ASK_AFTER => {
                let title = list(app).into_iter().find(|s| s.item_id == self.item_id).map(|s| s.title).unwrap_or_default();
                library::notice(app, "askFinished", title, Some(self.item_id), None);
            }
            _ => {}
        }
        let state = app.state::<AppState>();
        let left = {
            let mut sessions = state.sessions.lock();
            sessions.retain(|s| s.item_id != self.item_id);
            sessions.len()
        };
        changed(app);
        let _ = app.emit("library-changed", ());
        if left == 0 {
            tray::playback_ended(app);
        }
    }
}

fn save(app: &AppHandle, item_id: i64, file_id: i64, position: f64, duration: f64) {
    if let Err(e) = db::save_position(&app.state::<AppState>().db(), item_id, file_id, position, duration) {
        eprintln!("[playback] {e:#}");
    }
}

fn mark_watched(app: &AppHandle, item_id: i64) {
    let state = app.state::<AppState>();
    let (title, entry) = {
        let conn = state.db();
        let entry = match db::add_watch(&conn, item_id, None, "player") {
            Ok(id) => id,
            Err(e) => {
                eprintln!("[playback] {e:#}");
                return;
            }
        };
        let title = conn.query_row("SELECT title FROM items WHERE id = ?1", [item_id], |r| r.get::<_, String>(0));
        (title.unwrap_or_default(), entry)
    };
    // El aviso lleva el visionado para poder ponerle nota desde ahí mismo.
    library::notice(app, "markedWatched", title, Some(item_id), Some(entry.to_string()));
    let _ = app.emit("library-changed", ());
}
