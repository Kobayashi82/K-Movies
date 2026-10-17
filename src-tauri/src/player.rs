//! Reproductor integrado: mpv dibuja el vídeo dentro de una ventana de K-Movies y la interfaz web
//! (transparente) pone los controles encima.

use crate::mpv::{Event, Format, Mpv, Value};
use crate::playback::{Resolved, Tracker};
use crate::{AppState, db};
use anyhow::{Result, bail};
use serde::Serialize;
use std::collections::HashMap;
use std::sync::atomic::{AtomicBool, Ordering};
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
    /// Se han cambiado a mano las pistas o los retardos: al cerrar se guardan para ese archivo.
    touched: AtomicBool,
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

/// Códigos con los que suelen venir etiquetadas las pistas de cada idioma.
fn lang_tags(code: &str) -> Option<&'static str> {
    Some(match code {
        "es" => "es,spa,esp,cas",
        "en" => "en,eng",
        "fr" => "fr,fre,fra",
        "de" => "de,ger,deu",
        "it" => "it,ita",
        "pt" => "pt,por",
        "ca" => "ca,cat",
        "ja" => "ja,jpn",
        "ko" => "ko,kor",
        "zh" => "zh,chi,zho",
        "ru" => "ru,rus",
        _ => return None,
    })
}

/// Idiomas y estilo de los subtítulos según los ajustes. Se aplican sin abortar si alguno falla,
/// por si la libmpv instalada es de otra versión.
fn preferences(settings: &HashMap<String, String>, scale: f64, pos: f64, background: bool) -> Vec<(&'static str, String)> {
    let get = |k: &str| settings.get(k).map(String::as_str);
    let mut out: Vec<(&str, String)> = vec![
        ("sub-scale", format!("{scale:.2}")),
        ("sub-pos", format!("{pos:.0}")),
        // Caja semitransparente detrás del texto (lo que no hace Jellyfin) o el contorno clásico.
        ("sub-border-style", if background { "background-box" } else { "outline-and-shadow" }.into()),
        ("sub-back-color", "#A6000000".into()),
        ("sub-color", get("subColor").filter(|c| c.starts_with('#')).unwrap_or("#FFFFFF").into()),
        // Los ASS traen su propio estilo (karaoke, carteles); solo se impone el nuestro si se pide.
        ("sub-ass-override", if get("subAssOverride") == Some("true") { "force" } else { "scale" }.into()),
        // Subtítulos externos con nombre parecido al del vídeo (`Película.es.srt`).
        ("sub-auto", "fuzzy".into()),
    ];
    if let Some(tags) = get("audioLang").and_then(lang_tags) {
        out.push(("alang", tags.into()));
    }
    match get("subLang") {
        Some("none") => out.push(("sid", "no".into())),
        Some(code) => {
            if let Some(tags) = lang_tags(code) {
                out.push(("slang", tags.into()));
                // "Solo si hace falta": si el audio ya está en ese idioma, solo los forzados (carteles).
                let only_needed = get("subsOnlyIfNeeded") != Some("false");
                out.push(("subs-with-matching-audio", if only_needed { "forced" } else { "yes" }.into()));
            }
        }
        None => {}
    }
    out
}

pub fn open(app: &AppHandle, r: Resolved) -> Result<()> {
    if app.get_webview_window(LABEL).is_some() {
        bail!("already_playing");
    }
    let settings = db::settings(&app.state::<AppState>().db())?;
    let num = |k: &str, default: f64| settings.get(k).and_then(|v| v.parse().ok()).unwrap_or(default);
    let (scale, pos, volume) = (num("subScale", 1.0), num("subPos", 100.0), num("playerVolume", 100.0));
    let background = settings.get("subBackground").is_none_or(|v| v == "true");
    let file_prefs = db::file_prefs(&app.state::<AppState>().db(), r.file_id)?;

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
        ("volume", format!("{volume:.0}")),
    ];
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
    for (k, v) in preferences(&settings, scale, pos, background) {
        if let Err(e) = mpv.set(k, &v) {
            eprintln!("[player] {e:#}");
        }
    }
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
    slot(app).touched.store(false, Ordering::SeqCst);
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
    let session = Session { file_id: r.file_id, prefs: file_prefs, hwnd };
    let app = app.clone();
    std::thread::spawn(move || event_loop(app, mpv, tracker, session));
    Ok(())
}

struct Session {
    file_id: i64,
    /// Pistas y retardos que se eligieron la última vez para este archivo.
    prefs: Option<db::FilePrefs>,
    hwnd: isize,
}

/// Recupera las pistas y retardos de la última vez, si esas pistas siguen existiendo.
fn restore_prefs(mpv: &Mpv, p: &db::FilePrefs) {
    let tracks: Vec<serde_json::Value> = mpv.get("track-list").and_then(|j| serde_json::from_str(&j).ok()).unwrap_or_default();
    let has = |kind: &str, id: &str| tracks.iter().any(|t| t["type"] == kind && t["id"].to_string() == id);
    if let Some(aid) = p.aid.as_deref().filter(|id| has("audio", id)) {
        let _ = mpv.set("aid", aid);
    }
    if let Some(sid) = p.sid.as_deref().filter(|id| *id == "no" || has("sub", id)) {
        let _ = mpv.set("sid", sid);
    }
    let _ = mpv.set("audio-delay", &p.audio_delay.to_string());
    let _ = mpv.set("sub-delay", &p.sub_delay.to_string());
}

/// Pistas elegidas y retardos ahora mismo, para guardarlos con el archivo.
fn current_prefs(s: &PlayerState) -> db::FilePrefs {
    let selected = |kind: &str| {
        s.tracks.as_array().and_then(|ts| {
            ts.iter().find(|t| t["type"] == kind && t["selected"] == true).map(|t| t["id"].to_string())
        })
    };
    db::FilePrefs {
        aid: selected("audio"),
        sid: Some(selected("sub").unwrap_or_else(|| "no".into())),
        audio_delay: s.audio_delay,
        sub_delay: s.sub_delay,
    }
}

/// Lee los eventos de mpv hasta que termina la película o se cierra el reproductor.
fn event_loop(app: AppHandle, mpv: Arc<Mpv>, mut tracker: Tracker, session: Session) {
    let hwnd = session.hwnd;
    let mut dirty = true;
    let mut emitted = Instant::now();
    let mut tracked = Instant::now();
    let mut last_tracks = serde_json::Value::Null;
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
            Some(Event::FileLoaded) => {
                if let Some(p) = &session.prefs {
                    restore_prefs(&mpv, p);
                }
                video_to_back(hwnd);
                dirty = true;
            }
            Some(Event::VideoReconfig) => {
                video_to_back(hwnd);
                dirty = true;
            }
            Some(Event::Property { name, value }) => {
                if name == "track-list" {
                    let tracks: serde_json::Value =
                        mpv.get("track-list").and_then(|j| serde_json::from_str(&j).ok()).unwrap_or_default();
                    // Al cerrar, mpv descarga el archivo y la lista llega vacía: para recordar las
                    // pistas elegidas vale la última que tenía algo.
                    if tracks.as_array().is_some_and(|t| !t.is_empty()) {
                        last_tracks = tracks.clone();
                    }
                    with_state(&app, |s| s.tracks = tracks);
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
    if slot(&app).touched.swap(false, Ordering::SeqCst) {
        let snapshot = PlayerState { tracks: last_tracks, ..s };
        if let Err(e) = db::save_file_prefs(&app.state::<AppState>().db(), session.file_id, &current_prefs(&snapshot)) {
            eprintln!("[player] {e:#}");
        }
    }
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

/// Lo que se recuerda por archivo si se cambia a mano.
const PER_FILE: &[&str] = &["aid", "sid", "audio-delay", "sub-delay"];

pub fn command(app: &AppHandle, args: &[String]) -> Result<()> {
    let args: Vec<&str> = args.iter().map(String::as_str).collect();
    if args.first() == Some(&"add") && args.get(1).is_some_and(|p| PER_FILE.contains(p)) {
        slot(app).touched.store(true, Ordering::SeqCst);
    }
    mpv(app)?.command(&args)
}

/// Propiedades que la interfaz puede cambiar (no todas las de mpv).
const SETTABLE: &[&str] =
    &["pause", "volume", "mute", "aid", "sid", "audio-delay", "sub-delay", "sub-scale", "sub-pos", "sub-margin-y"];

pub fn set(app: &AppHandle, name: &str, value: &str) -> Result<()> {
    if !SETTABLE.contains(&name) {
        bail!("property not allowed: {name}");
    }
    if PER_FILE.contains(&name) {
        slot(app).touched.store(true, Ordering::SeqCst);
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

#[cfg(test)]
mod tests {
    use super::*;

    fn opts(pairs: &[(&str, &str)]) -> HashMap<&'static str, String> {
        let settings: HashMap<String, String> = pairs.iter().map(|(k, v)| (k.to_string(), v.to_string())).collect();
        preferences(&settings, 1.0, 100.0, true).into_iter().collect()
    }

    #[test]
    fn spanish_audio_else_original_with_spanish_subs() {
        let o = opts(&[("audioLang", "es"), ("subLang", "es")]);
        assert_eq!(o["alang"], "es,spa,esp,cas");
        assert_eq!(o["slang"], "es,spa,esp,cas");
        // Si el audio ya es castellano, solo los subtítulos forzados.
        assert_eq!(o["subs-with-matching-audio"], "forced");
        assert_eq!(o["sub-border-style"], "background-box");
        assert_eq!(o["sub-ass-override"], "scale");
        assert!(!o.contains_key("sid"));
    }

    #[test]
    fn original_audio_and_no_subtitles() {
        let o = opts(&[("audioLang", ""), ("subLang", "none"), ("subAssOverride", "true"), ("subColor", "#FFD83D")]);
        assert!(!o.contains_key("alang"));
        assert!(!o.contains_key("slang"));
        assert_eq!(o["sid"], "no");
        assert_eq!(o["sub-ass-override"], "force");
        assert_eq!(o["sub-color"], "#FFD83D");
    }

    #[test]
    fn subtitles_always_when_asked() {
        let o = opts(&[("subLang", "en"), ("subsOnlyIfNeeded", "false")]);
        assert_eq!(o["slang"], "en,eng");
        assert_eq!(o["subs-with-matching-audio"], "yes");
    }

    #[test]
    fn saved_prefs_use_the_selected_tracks() {
        let s = PlayerState {
            tracks: serde_json::json!([
                { "id": 1, "type": "video", "selected": true },
                { "id": 1, "type": "audio", "selected": false },
                { "id": 2, "type": "audio", "selected": true },
                { "id": 1, "type": "sub", "selected": false }
            ]),
            audio_delay: 0.1,
            sub_delay: -0.2,
            ..Default::default()
        };
        let p = current_prefs(&s);
        assert_eq!(p.aid.as_deref(), Some("2"));
        assert_eq!(p.sid.as_deref(), Some("no"));
        assert_eq!((p.audio_delay, p.sub_delay), (0.1, -0.2));
    }
}
