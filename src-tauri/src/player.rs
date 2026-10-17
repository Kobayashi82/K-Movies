//! Reproductor integrado: mpv dibuja el vídeo dentro de una ventana de K-Movies y la interfaz web
//! (transparente) pone los controles encima.

use crate::mpv::{Event, Format, Mpv, Value};
use crate::playback::{Resolved, Tracker};
use crate::{AppState, db};
use anyhow::{Result, bail};
use serde::Serialize;
use std::sync::{Arc, Mutex};
use std::time::{Duration, Instant};
use tauri::{AppHandle, Emitter, Manager, WebviewUrl, WebviewWindowBuilder, WindowEvent};

pub const LABEL: &str = "player";

/// Lo que ve la ventana del reproductor (evento `player-state`).
#[derive(Serialize, Clone, Default)]
#[serde(rename_all = "camelCase")]
pub struct PlayerState {
    pub item_id: i64,
    pub title: String,
    pub position: Option<f64>,
    pub duration: Option<f64>,
    pub paused: bool,
    pub buffering: bool,
    pub volume: f64,
    pub mute: bool,
    /// Pistas tal cual las da mpv (`track-list`): id, tipo, idioma, título, códec…
    pub tracks: serde_json::Value,
    pub audio_delay: f64,
    pub sub_delay: f64,
    pub sub_scale: f64,
    pub sub_pos: f64,
    pub sub_background: bool,
    pub fullscreen: bool,
    pub pip: bool,
}

#[derive(Default)]
pub struct Slot {
    mpv: Mutex<Option<Arc<Mpv>>>,
    state: Mutex<PlayerState>,
    /// Tamaño y posición de la ventana antes de pasar a imagen en imagen.
    restore: Mutex<Option<(tauri::PhysicalSize<u32>, tauri::PhysicalPosition<i32>)>>,
}

fn slot(app: &AppHandle) -> &Slot {
    &app.state::<AppState>().inner().player
}

fn mpv(app: &AppHandle) -> Result<Arc<Mpv>> {
    match slot(app).mpv.lock().unwrap_or_else(|e| e.into_inner()).clone() {
        Some(m) => Ok(m),
        None => bail!("not_playing"),
    }
}

fn with_state<R>(app: &AppHandle, f: impl FnOnce(&mut PlayerState) -> R) -> R {
    f(&mut slot(app).state.lock().unwrap_or_else(|e| e.into_inner()))
}

pub fn state(app: &AppHandle) -> PlayerState {
    with_state(app, |s| s.clone())
}

fn emit_state(app: &AppHandle) {
    let _ = app.emit_to(LABEL, "player-state", state(app));
}

/// Estilo de los subtítulos (ajustes `subScale`, `subPos`, `subBackground`), para mpv.
fn sub_style(scale: f64, pos: f64, background: bool) -> Vec<(&'static str, String)> {
    vec![
        ("sub-scale", format!("{scale:.2}")),
        ("sub-pos", format!("{pos:.0}")),
        // Caja semitransparente detrás del texto (lo que no hace Jellyfin) o el contorno clásico.
        ("sub-border-style", if background { "background-box" } else { "outline-and-shadow" }.into()),
        ("sub-back-color", "#A6000000".into()),
    ]
}

pub fn open(app: &AppHandle, r: Resolved) -> Result<()> {
    if app.get_webview_window(LABEL).is_some() {
        bail!("already_playing");
    }
    let settings = db::settings(&app.state::<AppState>().db())?;
    let num = |k: &str, default: f64| settings.get(k).and_then(|v| v.parse().ok()).unwrap_or(default);
    let (scale, pos, volume) = (num("subScale", 1.0), num("subPos", 100.0), num("playerVolume", 100.0));
    let background = settings.get("subBackground").is_none_or(|v| v == "true");

    let window = WebviewWindowBuilder::new(app, LABEL, WebviewUrl::App("index.html#player".into()))
        .title(&r.title)
        .inner_size(1280.0, 720.0)
        .min_inner_size(320.0, 180.0)
        .center()
        .decorations(false)
        .transparent(true)
        .focused(true)
        .build()?;
    let hwnd = window.hwnd()?.0 as isize;

    let mut options: Vec<(&str, String)> = vec![
        ("wid", hwnd.to_string()),
        ("vo", "gpu-next".into()),
        ("hwdec", "auto-safe".into()),
        ("force-window", "yes".into()),
        ("keep-open", "no".into()),
        // El teclado y el ratón los lleva la interfaz web, no mpv.
        ("input-default-bindings", "no".into()),
        ("input-vo-keyboard", "no".into()),
        ("input-cursor", "no".into()),
        ("osc", "no".into()),
        ("osd-level", "0".into()),
        // Subtítulos externos con nombre parecido al del vídeo (`Película.es.srt`).
        ("sub-auto", "fuzzy".into()),
        ("volume", format!("{volume:.0}")),
    ];
    options.extend(sub_style(scale, pos, background));
    if let Some(s) = r.start.filter(|s| *s > 1.0) {
        options.push(("start", format!("{s:.0}")));
    }
    let opts: Vec<(&str, &str)> = options.iter().map(|(k, v)| (*k, v.as_str())).collect();
    let mpv = match Mpv::new(&opts) {
        Ok(m) => Arc::new(m),
        Err(e) => {
            let _ = window.destroy();
            return Err(e);
        }
    };
    for (name, format) in [
        ("time-pos", Format::Double),
        ("duration", Format::Double),
        ("pause", Format::Flag),
        ("paused-for-cache", Format::Flag),
        ("volume", Format::Double),
        ("mute", Format::Flag),
        ("track-list", Format::None),
        ("audio-delay", Format::Double),
        ("sub-delay", Format::Double),
        ("sub-scale", Format::Double),
        ("sub-pos", Format::Double),
    ] {
        mpv.observe(name, format)?;
    }
    mpv.command(&["loadfile", &r.file.to_string_lossy()])?;

    let fresh = PlayerState {
        item_id: r.item_id,
        title: r.title.clone(),
        position: r.start,
        volume,
        sub_scale: scale,
        sub_pos: pos,
        sub_background: background,
        ..Default::default()
    };
    with_state(app, |s| *s = fresh);
    *slot(app).mpv.lock().unwrap_or_else(|e| e.into_inner()) = Some(mpv.clone());

    // Cerrar la ventana (Alt+F4, botón) para mpv primero; la ventana se destruye al acabar.
    let handle = app.clone();
    window.on_window_event(move |e| {
        if let WindowEvent::CloseRequested { api, .. } = e {
            api.prevent_close();
            close(&handle);
        }
    });

    let tracker = Tracker::begin(app, &r, "K-Movies".into(), true);
    let app = app.clone();
    std::thread::spawn(move || event_loop(app, mpv, tracker, hwnd));
    Ok(())
}

/// Lee los eventos de mpv hasta que termina la película o se cierra el reproductor.
fn event_loop(app: AppHandle, mpv: Arc<Mpv>, mut tracker: Tracker, hwnd: isize) {
    let mut dirty = true;
    let mut emitted = Instant::now();
    let mut tracked = Instant::now();
    loop {
        match mpv.wait_event(0.25) {
            Some(Event::Shutdown) => break,
            Some(Event::EndFile { reason, error }) => {
                if reason == 4 {
                    let _ = app.emit_to(LABEL, "player-error", error);
                    eprintln!("[player] end-file error {error}");
                }
                break;
            }
            Some(Event::FileLoaded | Event::VideoReconfig) => {
                video_to_back(hwnd);
                dirty = true;
            }
            Some(Event::Property { name, value }) => {
                if name == "track-list" {
                    let tracks = mpv.get("track-list").and_then(|j| serde_json::from_str(&j).ok());
                    with_state(&app, |s| s.tracks = tracks.unwrap_or_default());
                } else {
                    remember(&app, &name, &value);
                    with_state(&app, |s| apply(s, &name, value));
                }
                dirty = true;
            }
            _ => {}
        }
        // La interfaz se actualiza 4 veces por segundo como mucho; el seguimiento, una.
        if dirty && emitted.elapsed() >= Duration::from_millis(250) {
            emit_state(&app);
            emitted = Instant::now();
            dirty = false;
        }
        if tracked.elapsed() >= Duration::from_secs(1) {
            let s = state(&app);
            if let (Some(pos), Some(len)) = (s.position, s.duration) {
                tracker.status(pos, len, s.paused);
            }
            tracked = Instant::now();
        }
    }
    let s = state(&app);
    if let (Some(pos), Some(len)) = (s.position, s.duration) {
        tracker.status(pos, len, s.paused);
    }
    tracker.finish();
    // Primero mpv (destruye su ventana de vídeo) y después la nuestra.
    *slot(&app).mpv.lock().unwrap_or_else(|e| e.into_inner()) = None;
    *slot(&app).restore.lock().unwrap_or_else(|e| e.into_inner()) = None;
    drop(mpv);
    if let Some(w) = app.get_webview_window(LABEL) {
        let _ = w.destroy();
    }
}

/// Volumen y estilo de los subtítulos se recuerdan para la próxima película.
fn remember(app: &AppHandle, name: &str, value: &Value) {
    let key = match name {
        "volume" => "playerVolume",
        "sub-scale" => "subScale",
        "sub-pos" => "subPos",
        _ => return,
    };
    if let Value::Double(v) = value {
        let changed = with_state(app, |s| match name {
            "volume" => (s.volume - v).abs() > 0.001,
            "sub-scale" => (s.sub_scale - v).abs() > 0.001,
            _ => (s.sub_pos - v).abs() > 0.001,
        });
        if changed {
            let _ = db::set_setting(&app.state::<AppState>().db(), key, &format!("{v:.2}"));
        }
    }
}

fn apply(s: &mut PlayerState, name: &str, value: Value) {
    match (name, value) {
        ("time-pos", Value::Double(v)) => s.position = Some(v),
        ("duration", Value::Double(v)) => s.duration = Some(v),
        ("pause", Value::Flag(v)) => s.paused = v,
        ("paused-for-cache", Value::Flag(v)) => s.buffering = v,
        ("volume", Value::Double(v)) => s.volume = v,
        ("mute", Value::Flag(v)) => s.mute = v,
        ("audio-delay", Value::Double(v)) => s.audio_delay = v,
        ("sub-delay", Value::Double(v)) => s.sub_delay = v,
        ("sub-scale", Value::Double(v)) => s.sub_scale = v,
        ("sub-pos", Value::Double(v)) => s.sub_pos = v,
        _ => {}
    }
}

/// mpv crea su ventana de vídeo encima de todo; se manda al fondo para que los controles
/// (la interfaz web, transparente) queden por delante.
fn video_to_back(parent: isize) {
    use windows_sys::Win32::UI::WindowsAndMessaging::{
        FindWindowExW, HWND_BOTTOM, SWP_NOACTIVATE, SWP_NOMOVE, SWP_NOSIZE, SetWindowPos,
    };
    let class: Vec<u16> = "mpv\0".encode_utf16().collect();
    // SAFETY: `parent` es la ventana del reproductor, viva mientras dura la sesión.
    unsafe {
        let child = FindWindowExW(parent as _, std::ptr::null_mut(), class.as_ptr(), std::ptr::null());
        if !child.is_null() {
            SetWindowPos(child, HWND_BOTTOM, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
        }
    }
}

// ---------------------------------------------------------------------------
// Órdenes desde la interfaz

pub fn command(app: &AppHandle, args: &[String]) -> Result<()> {
    let args: Vec<&str> = args.iter().map(String::as_str).collect();
    mpv(app)?.command(&args)
}

/// Propiedades que la interfaz puede cambiar (no todas las de mpv).
const SETTABLE: &[&str] =
    &["pause", "volume", "mute", "aid", "sid", "audio-delay", "sub-delay", "sub-scale", "sub-pos", "sub-margin-y"];

pub fn set(app: &AppHandle, name: &str, value: &str) -> Result<()> {
    if !SETTABLE.contains(&name) {
        bail!("property not allowed: {name}");
    }
    mpv(app)?.set(name, value)
}

/// Caja de fondo de los subtítulos sí o no (se guarda como ajuste junto con tamaño y posición).
pub fn set_sub_background(app: &AppHandle, on: bool) -> Result<()> {
    let m = mpv(app)?;
    m.set("sub-border-style", if on { "background-box" } else { "outline-and-shadow" })?;
    with_state(app, |s| s.sub_background = on);
    emit_state(app);
    Ok(())
}

pub fn close(app: &AppHandle) {
    match mpv(app) {
        Ok(m) => {
            let _ = m.command(&["quit"]);
            m.wakeup();
        }
        // Sin mpv (p. ej. falló al arrancar): se cierra la ventana sin más.
        Err(_) => {
            if let Some(w) = app.get_webview_window(LABEL) {
                let _ = w.destroy();
            }
        }
    }
}

pub fn set_fullscreen(app: &AppHandle, on: bool) -> Result<()> {
    let Some(w) = app.get_webview_window(LABEL) else { bail!("not_playing") };
    if on && state(app).pip {
        set_pip(app, false)?;
    }
    w.set_fullscreen(on)?;
    with_state(app, |s| s.fullscreen = on);
    emit_state(app);
    Ok(())
}

/// Imagen en imagen: ventana pequeña, siempre encima, en la esquina inferior derecha.
pub fn set_pip(app: &AppHandle, on: bool) -> Result<()> {
    let Some(w) = app.get_webview_window(LABEL) else { bail!("not_playing") };
    let restore = &slot(app).restore;
    if on {
        if state(app).fullscreen {
            w.set_fullscreen(false)?;
        }
        *restore.lock().unwrap_or_else(|e| e.into_inner()) = Some((w.inner_size()?, w.outer_position()?));
        let monitor = w.current_monitor()?.or(w.primary_monitor()?);
        if let Some(m) = monitor {
            let area = m.work_area();
            let scale = m.scale_factor();
            let width = (480.0 * scale) as u32;
            let height = (270.0 * scale) as u32;
            let margin = (16.0 * scale) as i32;
            w.set_size(tauri::PhysicalSize::new(width, height))?;
            w.set_position(tauri::PhysicalPosition::new(
                area.position.x + area.size.width as i32 - width as i32 - margin,
                area.position.y + area.size.height as i32 - height as i32 - margin,
            ))?;
        }
        w.set_always_on_top(true)?;
    } else {
        w.set_always_on_top(false)?;
        if let Some((size, pos)) = restore.lock().unwrap_or_else(|e| e.into_inner()).take() {
            w.set_size(size)?;
            w.set_position(pos)?;
        }
    }
    with_state(app, |s| {
        s.pip = on;
        s.fullscreen = false;
    });
    emit_state(app);
    Ok(())
}
