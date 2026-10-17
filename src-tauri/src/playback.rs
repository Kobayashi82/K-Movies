//! Seguimiento de lo que se está viendo: abre el reproductor, pregunta cada pocos segundos por
//! dónde vas, guarda el progreso y marca la película como vista al pasar del umbral.

use crate::{AppState, db, library, players};
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

/// Lo que se está reproduciendo ahora (se manda entero a la interfaz en el evento `playback`).
#[derive(Serialize, Clone)]
#[serde(rename_all = "camelCase")]
pub struct NowPlaying {
    pub item_id: i64,
    pub title: String,
    pub player: String,
    /// Se puede saber por dónde va (VLC). Con los demás reproductores solo se sabe que está abierto.
    pub tracked: bool,
    pub position: Option<f64>,
    pub duration: Option<f64>,
    pub paused: bool,
    /// Ya se ha marcado como vista en esta sesión.
    pub watched: bool,
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

/// Abre la película. Por defecto continúa donde se quedó; `from_start` empieza de cero.
/// Sin `file_id` usa el archivo con el que se empezó a ver o, si no, el más grande.
pub fn play(app: &AppHandle, item_id: i64, file_id: Option<i64>, from_start: bool) -> Result<()> {
    let state = app.state::<AppState>();
    if state.sessions.lock().iter().any(|s| s.item_id == item_id) {
        bail!("already_playing");
    }
    let (file_id, file, start, title, player) = {
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
        // La posición vale aunque se elija otra versión del archivo: es la misma película.
        let start = if from_start { None } else { resume.map(|r| r.1) };
        let title: String = conn.query_row("SELECT title FROM items WHERE id = ?1", [item_id], |r| r.get(0))?;
        let player = db::setting(&conn, "playerPath")?.filter(|p| !p.is_empty()).map(PathBuf::from);
        (file_id, PathBuf::from(file), start, title, player)
    };
    let player = player.or_else(|| players::detect().into_iter().next().map(|p| PathBuf::from(p.path)));
    let Some(player) = player else {
        // Sin reproductor configurado ni detectado: el predeterminado de Windows, sin seguimiento.
        if !file.exists() {
            bail!("file_missing");
        }
        return tauri_plugin_opener::open_path(&file, None::<&str>).map_err(|e| anyhow!(e.to_string()));
    };
    let launched = players::launch(&player, &file, start)?;
    state.sessions.lock().push(NowPlaying {
        item_id,
        title,
        player: players::name_of(&player),
        tracked: launched.vlc.is_some(),
        position: start,
        duration: None,
        paused: false,
        watched: false,
    });
    changed(app);
    let app = app.clone();
    let name = file.file_name().unwrap_or_default().to_string_lossy().into_owned();
    tauri::async_runtime::spawn(async move { track(app, item_id, file_id, name, launched).await });
    Ok(())
}

async fn track(app: AppHandle, item_id: i64, file_id: i64, file_name: String, mut launched: players::Launched) {
    let started = Instant::now();
    let threshold = threshold(&app);
    let mut last: Option<(f64, f64)> = None; // posición y duración
    let mut paused = false;
    let mut watched = false;
    let mut saved_at = Instant::now();
    loop {
        tokio::time::sleep(POLL).await;
        if !matches!(launched.child.try_wait(), Ok(None)) {
            break;
        }
        let Some(vlc) = &launched.vlc else { continue };
        // Mientras VLC arranca, la interfaz HTTP aún no responde: se reintenta en la siguiente vuelta.
        let Ok(st) = vlc.status().await else { continue };
        // Solo cuenta nuestra película: parada (al acabar vuelve a 0) u otro archivo abierto en ese VLC, no.
        if st.length <= 0.0 || !matches!(st.state.as_str(), "playing" | "paused") || !st.is_file(&file_name) {
            continue;
        }
        let now_paused = st.state == "paused";
        last = Some((st.time, st.length));
        if !watched && st.time / st.length >= threshold {
            watched = true;
            mark_watched(&app, item_id);
        } else if !watched && st.time >= MIN_POSITION && (saved_at.elapsed() >= SAVE_EVERY || now_paused != paused) {
            save(&app, item_id, file_id, st.time, st.length);
            saved_at = Instant::now();
        }
        paused = now_paused;
        update(&app, item_id, |s| {
            s.position = Some(st.time);
            s.duration = Some(st.length);
            s.paused = paused;
            s.watched = watched;
        });
    }

    // Al cerrar el reproductor.
    match last {
        Some((pos, len)) if !watched && pos / len >= threshold => mark_watched(&app, item_id),
        Some((pos, len)) if !watched && pos >= MIN_POSITION => save(&app, item_id, file_id, pos, len),
        None if started.elapsed() >= ASK_AFTER => {
            let title = list(&app).into_iter().find(|s| s.item_id == item_id).map(|s| s.title).unwrap_or_default();
            library::notice(&app, "askFinished", title, Some(item_id), None);
        }
        _ => {}
    }
    app.state::<AppState>().sessions.lock().retain(|s| s.item_id != item_id);
    changed(&app);
    let _ = app.emit("library-changed", ());
}

fn save(app: &AppHandle, item_id: i64, file_id: i64, position: f64, duration: f64) {
    if let Err(e) = db::save_position(&app.state::<AppState>().db(), item_id, file_id, position, duration) {
        eprintln!("[playback] {e:#}");
    }
}

fn mark_watched(app: &AppHandle, item_id: i64) {
    let state = app.state::<AppState>();
    let title = {
        let conn = state.db();
        if let Err(e) = db::add_watch(&conn, item_id, None, "player") {
            eprintln!("[playback] {e:#}");
            return;
        }
        conn.query_row("SELECT title FROM items WHERE id = ?1", [item_id], |r| r.get::<_, String>(0)).unwrap_or_default()
    };
    library::notice(app, "markedWatched", title, Some(item_id), None);
    let _ = app.emit("library-changed", ());
}
