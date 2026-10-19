//! Reproductor integrado: mpv dibuja el vídeo dentro de una ventana de K-Movies y la interfaz web
//! (transparente) pone los controles encima. Puede haber varias películas a la vez, cada una en su
//! ventana (`player-1`, `player-2`…) con su propio mpv.

use crate::mpv::{Event, Format, Mpv, Value};
use crate::playback::{Resolved, Tracker};
use crate::{AppState, db, library, tray};
use anyhow::{Result, bail};
use serde::Serialize;
use std::collections::HashMap;
use std::sync::atomic::{AtomicBool, AtomicU64, Ordering};
use std::sync::{Arc, Mutex};
use std::time::{Duration, Instant};
use tauri::{AppHandle, Emitter, Manager, PhysicalPosition, PhysicalSize, WebviewUrl, WebviewWindowBuilder, WindowEvent};

/// Las ventanas del reproductor se llaman `player-1`, `player-2`…
pub const PREFIX: &str = "player-";

/// Si no llega la imagen (p. ej. un archivo solo de audio), la ventana se enseña igualmente.
const REVEAL_AFTER: Duration = Duration::from_secs(3);

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
    /// Distancia de los subtítulos al borde inferior, en píxeles de un vídeo de 720 de alto.
    pub sub_margin: f64,
    pub sub_background: bool,
    pub fullscreen: bool,
    /// Siempre encima de las demás ventanas.
    pub ontop: bool,
    /// Dónde queda la imagen en la ventana (cambia con la proporción, el recorte y el tamaño).
    pub frame: Frame,
    /// Proporción forzada (`16:9`) y recorte (`2.39:1`) de esta sesión; vacío = por defecto.
    pub aspect: String,
    pub crop: String,
}

/// Alto de la ventana y bandas negras de arriba y abajo, en píxeles reales (`osd-dimensions`).
#[derive(Serialize, Clone, Copy, Default, PartialEq)]
pub struct Frame {
    pub h: f64,
    pub mt: f64,
    pub mb: f64,
}

/// Una película abierta en el reproductor integrado.
struct Session {
    item_id: i64,
    mpv: Mutex<Option<Arc<Mpv>>>,
    state: Mutex<PlayerState>,
    /// Se han cambiado a mano pistas, retardos o aspecto de los subtítulos: al cerrar se guardan
    /// para ese archivo.
    touched: AtomicBool,
    /// La ventana ya se ha enseñado (nace oculta hasta que hay imagen).
    shown: AtomicBool,
    /// Ajuste "Abrir en pantalla completa": se pone al enseñar la ventana.
    fullscreen_on_start: bool,
}

impl Session {
    fn mpv(&self) -> Result<Arc<Mpv>> {
        match self.mpv.lock().unwrap_or_else(|e| e.into_inner()).clone() {
            Some(m) => Ok(m),
            None => bail!("not_playing"),
        }
    }

    fn with_state<R>(&self, f: impl FnOnce(&mut PlayerState) -> R) -> R {
        f(&mut self.state.lock().unwrap_or_else(|e| e.into_inner()))
    }

    fn state(&self) -> PlayerState {
        self.with_state(|s| s.clone())
    }
}

#[derive(Default)]
pub struct Players {
    sessions: Mutex<HashMap<String, Arc<Session>>>,
    next: AtomicU64,
}

fn players(app: &AppHandle) -> &Players {
    &app.state::<AppState>().inner().player
}

fn sessions(app: &AppHandle) -> std::sync::MutexGuard<'_, HashMap<String, Arc<Session>>> {
    players(app).sessions.lock().unwrap_or_else(|e| e.into_inner())
}

fn session(app: &AppHandle, label: &str) -> Result<Arc<Session>> {
    match sessions(app).get(label) {
        Some(s) => Ok(s.clone()),
        None => bail!("not_playing"),
    }
}

pub fn state(app: &AppHandle, label: &str) -> PlayerState {
    session(app, label).map(|s| s.state()).unwrap_or_default()
}

fn emit_state(app: &AppHandle, label: &str, s: &Session) {
    let _ = app.emit_to(label, "player-state", s.state());
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

/// Aspecto de los subtítulos, de Ajustes generales. Lo que se cambie durante una película (tamaño,
/// altura, fondo) vale solo para esa sesión.
#[derive(Debug, Clone, PartialEq)]
pub struct SubStyle {
    pub scale: f64,
    /// Distancia al borde inferior, en % del alto de la imagen.
    pub height: f64,
    pub background: bool,
    /// Opacidad de la caja de fondo, en %.
    pub back_opacity: f64,
    pub color: String,
    /// Vacío = la letra por defecto de mpv (sin serifa del sistema).
    pub font: String,
    pub bold: bool,
    /// Contorno negro alrededor de las letras.
    pub outline: bool,
    /// Grosor del contorno (con caja de fondo, también el relleno de la caja).
    pub border: f64,
    pub shadow: f64,
    pub ass_override: bool,
}

impl SubStyle {
    pub fn from_settings(settings: &HashMap<String, String>) -> Self {
        let num = |k: &str, default: f64, min: f64, max: f64| {
            settings.get(k).and_then(|v| v.parse::<f64>().ok()).unwrap_or(default).clamp(min, max)
        };
        let flag = |k: &str, default: bool| settings.get(k).map_or(default, |v| v == "true");
        SubStyle {
            scale: num("subScale", 1.0, 0.3, 3.0),
            height: num("subHeight", 3.0, 0.0, 40.0),
            background: flag("subBackground", true),
            back_opacity: num("subBackOpacity", 65.0, 0.0, 100.0),
            color: settings.get("subColor").filter(|c| c.len() == 7 && c.starts_with('#')).cloned().unwrap_or("#FFFFFF".into()),
            font: settings.get("subFont").cloned().unwrap_or_default(),
            bold: flag("subBold", false),
            outline: flag("subOutline", false),
            border: num("subBorder", 3.0, 0.0, 8.0),
            shadow: num("subShadow", 0.0, 0.0, 6.0),
            ass_override: flag("subAssOverride", false),
        }
    }

    /// `sub-margin-y` (píxeles de un vídeo de 720 de alto) para la altura elegida.
    pub fn margin(&self) -> f64 {
        self.height * 7.2
    }

    fn options(&self) -> Vec<(&'static str, String)> {
        let alpha = (self.back_opacity / 100.0 * 255.0).round() as u8;
        let mut out = vec![
            ("sub-scale", format!("{:.2}", self.scale)),
            ("sub-pos", "100".into()),
            ("sub-margin-y", format!("{:.0}", self.margin())),
            // Caja semitransparente detrás del texto (lo que no hace Jellyfin) o el contorno clásico.
            ("sub-border-style", if self.background { "background-box" } else { "outline-and-shadow" }.into()),
            ("sub-back-color", format!("#{alpha:02X}000000")),
            // El contorno de las letras se quita haciéndolo transparente: el grosor sigue sirviendo
            // de relleno de la caja.
            ("sub-outline-color", if self.outline { "#FF000000" } else { "#00000000" }.into()),
            ("sub-color", self.color.clone()),
            ("sub-bold", if self.bold { "yes" } else { "no" }.into()),
            ("sub-border-size", format!("{:.1}", self.border)),
            ("sub-shadow-offset", format!("{:.1}", self.shadow)),
            // Los ASS traen su propio estilo (karaoke, carteles); solo se impone el nuestro si se pide.
            ("sub-ass-override", if self.ass_override { "force" } else { "scale" }.into()),
        ];
        if !self.font.is_empty() {
            out.push(("sub-font", self.font.clone()));
        }
        out
    }
}

/// Idiomas y estilo de los subtítulos según los ajustes. Se aplican sin abortar si alguno falla,
/// por si la libmpv instalada es de otra versión.
fn preferences(settings: &HashMap<String, String>, style: &SubStyle) -> Vec<(&'static str, String)> {
    let get = |k: &str| settings.get(k).map(String::as_str);
    let mut out = style.options();
    // Subtítulos externos con nombre parecido al del vídeo (`Película.es.srt`).
    out.push(("sub-auto", "fuzzy".into()));
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

/// Tamaño y posición de la última ventana del reproductor que se cerró (en píxeles reales).
fn saved_geometry(settings: &HashMap<String, String>) -> Option<(i32, i32, u32, u32)> {
    let v: Vec<i64> = settings.get("playerWindow")?.split(',').filter_map(|n| n.trim().parse().ok()).collect();
    match v[..] {
        [x, y, w, h] if w >= 320 && h >= 180 => Some((x as i32, y as i32, w as u32, h as u32)),
        _ => None,
    }
}

pub fn open(app: &AppHandle, r: Resolved) -> Result<()> {
    let n = players(app).next.fetch_add(1, Ordering::SeqCst) + 1;
    let label = format!("{PREFIX}{n}");
    let settings = db::settings(&app.state::<AppState>().db())?;
    let volume = settings.get("playerVolume").and_then(|v| v.parse::<f64>().ok()).unwrap_or(100.0);
    let ontop = settings.get("playerOntop").is_some_and(|v| v == "true");
    let file_prefs = db::file_prefs(&app.state::<AppState>().db(), r.file_id)?;
    let general = SubStyle::from_settings(&settings);
    let style = general.clone().with_file(file_prefs.as_ref());

    // Nace oculta y se enseña cuando ya hay imagen: si no, se verían un momento solo los bordes
    // de una ventana transparente.
    let window = WebviewWindowBuilder::new(app, &label, WebviewUrl::App("index.html#player".into()))
        .title(&r.title)
        .inner_size(1280.0, 720.0)
        .min_inner_size(320.0, 180.0)
        .center()
        .decorations(false)
        .transparent(true)
        .always_on_top(ontop)
        .visible(false)
        .build()?;
    // Donde estaba la última; si ya hay otra abierta, un poco desplazada para que no se tapen.
    if let Some((x, y, w, h)) = saved_geometry(&settings) {
        let offset = 32 * sessions(app).len() as i32;
        if tray::on_screen(app, x + offset, y + offset, w) {
            let _ = window.set_size(PhysicalSize::new(w, h));
            let _ = window.set_position(PhysicalPosition::new(x + offset, y + offset));
        }
    }
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
    for (k, v) in preferences(&settings, &style) {
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
        ("osd-dimensions", Format::None),
    ] {
        mpv.observe(name, format)?;
    }
    mpv.command(&["loadfile", &r.file.to_string_lossy()])?;

    let session = Arc::new(Session {
        item_id: r.item_id,
        mpv: Mutex::new(Some(mpv.clone())),
        state: Mutex::new(PlayerState {
            item_id: r.item_id,
            title: r.title.clone(),
            position: r.start,
            volume,
            sub_scale: style.scale,
            sub_margin: style.margin(),
            sub_background: style.background,
            ontop,
            ..Default::default()
        }),
        touched: AtomicBool::new(false),
        shown: AtomicBool::new(false),
        fullscreen_on_start: settings.get("playerFullscreen").is_some_and(|v| v == "true"),
    });
    sessions(app).insert(label.clone(), session.clone());

    // Cerrar la ventana (Alt+F4, botón) para mpv primero; la ventana se destruye al acabar.
    let (handle, l) = (app.clone(), label.clone());
    window.on_window_event(move |e| {
        if let WindowEvent::CloseRequested { api, .. } = e {
            api.prevent_close();
            close(&handle, &l);
        }
    });
    // Por si la imagen no llega nunca (solo audio, un error): se enseña igualmente.
    {
        let (app, label, session) = (app.clone(), label.clone(), session.clone());
        std::thread::spawn(move || {
            std::thread::sleep(REVEAL_AFTER);
            reveal(&app, &label, &session);
        });
    }

    let tracker = Tracker::begin(app, &r, "K-Movies".into(), true, Some(label.clone()));
    let context = Context { label, file_id: r.file_id, prefs: file_prefs, general, hwnd };
    let app = app.clone();
    std::thread::spawn(move || event_loop(app, mpv, session, tracker, context));
    Ok(())
}

/// Enseña la ventana la primera vez (nace oculta).
fn reveal(app: &AppHandle, label: &str, session: &Session) {
    if session.shown.swap(true, Ordering::SeqCst) {
        return;
    }
    if let Some(w) = app.get_webview_window(label) {
        let _ = w.show();
        let _ = w.set_focus();
        if session.fullscreen_on_start && w.set_fullscreen(true).is_ok() {
            session.with_state(|s| s.fullscreen = true);
            emit_state(app, label, session);
        }
    }
}

struct Context {
    label: String,
    file_id: i64,
    /// Lo que se eligió la última vez para este archivo.
    prefs: Option<db::FilePrefs>,
    /// Aspecto de los subtítulos de Ajustes: lo que coincida con él no se guarda con el archivo.
    general: SubStyle,
    hwnd: isize,
}

impl SubStyle {
    /// Lo general con lo que se cambió para este archivo por encima.
    fn with_file(mut self, p: Option<&db::FilePrefs>) -> Self {
        if let Some(p) = p {
            self.scale = p.sub_scale.unwrap_or(self.scale);
            self.height = p.sub_margin.map_or(self.height, |m| m / 7.2);
            self.background = p.sub_background.unwrap_or(self.background);
        }
        self
    }
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

/// Pistas, retardos y aspecto de los subtítulos ahora mismo, para guardarlos con el archivo. Del
/// aspecto solo se guarda lo que difiere de Ajustes (lo demás sigue lo general aunque se cambie).
fn current_prefs(s: &PlayerState, general: &SubStyle) -> db::FilePrefs {
    let selected = |kind: &str| {
        s.tracks.as_array().and_then(|ts| {
            ts.iter().find(|t| t["type"] == kind && t["selected"] == true).map(|t| t["id"].to_string())
        })
    };
    let differs = |a: f64, b: f64| (a - b).abs() > 0.001;
    db::FilePrefs {
        aid: selected("audio"),
        sid: Some(selected("sub").unwrap_or_else(|| "no".into())),
        audio_delay: s.audio_delay,
        sub_delay: s.sub_delay,
        sub_scale: differs(s.sub_scale, general.scale).then_some(s.sub_scale),
        sub_margin: differs(s.sub_margin, general.margin()).then_some(s.sub_margin),
        sub_background: (s.sub_background != general.background).then_some(s.sub_background),
    }
}

/// Lee los eventos de mpv hasta que termina la película o se cierra el reproductor.
fn event_loop(app: AppHandle, mpv: Arc<Mpv>, session: Arc<Session>, mut tracker: Tracker, ctx: Context) {
    let label = ctx.label.as_str();
    let mut dirty = true;
    let mut emitted = Instant::now();
    let mut tracked = Instant::now();
    let mut last_tracks = serde_json::Value::Null;
    loop {
        match mpv.wait_event(0.25) {
            Some(Event::Shutdown) => break,
            Some(Event::EndFile { reason, error }) => {
                if reason == 4 {
                    eprintln!("[player] end-file error {error}");
                    let title = session.with_state(|s| s.title.clone());
                    library::notice(&app, "playError", title, Some(session.item_id), Some("play_failed".into()));
                }
                break;
            }
            Some(Event::FileLoaded) => {
                if let Some(p) = &ctx.prefs {
                    restore_prefs(&mpv, p);
                }
                video_to_back(ctx.hwnd);
                dirty = true;
            }
            Some(Event::VideoReconfig) => {
                video_to_back(ctx.hwnd);
                dirty = true;
            }
            Some(Event::PlaybackRestart) => reveal(&app, label, &session),
            Some(Event::Property { name, value }) => {
                if name == "track-list" {
                    let tracks: serde_json::Value =
                        mpv.get("track-list").and_then(|j| serde_json::from_str(&j).ok()).unwrap_or_default();
                    // Al cerrar, mpv descarga el archivo y la lista llega vacía: para recordar las
                    // pistas elegidas vale la última que tenía algo.
                    if tracks.as_array().is_some_and(|t| !t.is_empty()) {
                        last_tracks = tracks.clone();
                    }
                    session.with_state(|s| s.tracks = tracks);
                } else if name == "osd-dimensions" {
                    let num = |k: &str| mpv.get(&format!("osd-dimensions/{k}")).and_then(|v| v.parse::<f64>().ok()).unwrap_or(0.0);
                    let frame = Frame { h: num("h"), mt: num("mt"), mb: num("mb") };
                    session.with_state(|s| s.frame = frame);
                } else {
                    remember(&app, &session, &name, &value);
                    session.with_state(|s| apply(s, &name, value));
                }
                dirty = true;
            }
            _ => {}
        }
        // La interfaz se actualiza 4 veces por segundo como mucho; el seguimiento, una.
        if dirty && emitted.elapsed() >= Duration::from_millis(250) {
            emit_state(&app, label, &session);
            emitted = Instant::now();
            dirty = false;
        }
        if tracked.elapsed() >= Duration::from_secs(1) {
            let s = session.state();
            if let (Some(pos), Some(len)) = (s.position, s.duration) {
                tracker.status(pos, len, s.paused);
            }
            tracked = Instant::now();
        }
    }
    let s = session.state();
    if let (Some(pos), Some(len)) = (s.position, s.duration) {
        tracker.status(pos, len, s.paused);
    }
    if session.touched.swap(false, Ordering::SeqCst) {
        let snapshot = PlayerState { tracks: last_tracks, ..s.clone() };
        let prefs = current_prefs(&snapshot, &ctx.general);
        if let Err(e) = db::save_file_prefs(&app.state::<AppState>().db(), ctx.file_id, &prefs) {
            eprintln!("[player] {e:#}");
        }
    }
    // Primero mpv (destruye su ventana de vídeo) y después la nuestra.
    *session.mpv.lock().unwrap_or_else(|e| e.into_inner()) = None;
    drop(mpv);
    if let Some(w) = app.get_webview_window(label) {
        // La próxima ventana del reproductor se abre donde estaba esta (salvo en pantalla completa).
        if !s.fullscreen
            && session.shown.load(Ordering::SeqCst)
            && let (Ok(pos), Ok(size)) = (w.outer_position(), w.inner_size())
        {
            let value = format!("{},{},{},{}", pos.x, pos.y, size.width, size.height);
            let _ = db::set_setting(&app.state::<AppState>().db(), "playerWindow", &value);
        }
        let _ = w.destroy();
    }
    sessions(&app).remove(label);
    // Al final: si era la última película, K-Movies vuelve a enseñarse (si se escondió al reproducir).
    tracker.finish();
}

/// El volumen se recuerda para la próxima película. Lo demás que se cambie durante la película
/// (tamaño y altura de los subtítulos, fondo) vale solo para esa sesión: lo general va en Ajustes.
fn remember(app: &AppHandle, session: &Session, name: &str, value: &Value) {
    if let ("volume", Value::Double(v)) = (name, value)
        && session.with_state(|s| (s.volume - v).abs() > 0.001)
    {
        let _ = db::set_setting(&app.state::<AppState>().db(), "playerVolume", &format!("{v:.0}"));
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
// Órdenes desde la interfaz (`label`: la ventana del reproductor que las manda)

/// Lo que se recuerda por archivo si se cambia a mano.
const PER_FILE: &[&str] = &["aid", "sid", "audio-delay", "sub-delay", "sub-scale"];

pub fn command(app: &AppHandle, label: &str, args: &[String]) -> Result<()> {
    let s = session(app, label)?;
    let args: Vec<&str> = args.iter().map(String::as_str).collect();
    if args.first() == Some(&"add") && args.get(1).is_some_and(|p| PER_FILE.contains(p)) {
        s.touched.store(true, Ordering::SeqCst);
    }
    s.mpv()?.command(&args)
}

/// Propiedades que la interfaz puede cambiar (no todas las de mpv).
const SETTABLE: &[&str] =
    &["pause", "volume", "mute", "aid", "sid", "audio-delay", "sub-delay", "sub-scale", "sub-margin-y", "sub-pos"];

pub fn set(app: &AppHandle, label: &str, name: &str, value: &str) -> Result<()> {
    if !SETTABLE.contains(&name) {
        bail!("property not allowed: {name}");
    }
    let s = session(app, label)?;
    if PER_FILE.contains(&name) {
        s.touched.store(true, Ordering::SeqCst);
    }
    s.mpv()?.set(name, value)
}

/// Caja de fondo de los subtítulos sí o no, para esta película.
pub fn set_sub_background(app: &AppHandle, label: &str, on: bool) -> Result<()> {
    let s = session(app, label)?;
    s.mpv()?.set("sub-border-style", if on { "background-box" } else { "outline-and-shadow" })?;
    s.touched.store(true, Ordering::SeqCst);
    s.with_state(|st| st.sub_background = on);
    emit_state(app, label, &s);
    Ok(())
}

/// Sube o baja los subtítulos `percent` puntos (de 1 en 1, entre 0 y 40 % del alto), para esta
/// película. Se suma aquí, sobre el valor real, para que los clics rápidos no se pisen. La interfaz
/// lo aplica con `sub-margin-y`, porque mientras se ven los controles los sube más.
pub fn add_sub_margin(app: &AppHandle, label: &str, percent: f64) -> Result<()> {
    let s = session(app, label)?;
    s.mpv()?;
    s.touched.store(true, Ordering::SeqCst);
    s.with_state(|st| st.sub_margin = ((st.sub_margin / 7.2).round() + percent).clamp(0.0, 40.0) * 7.2);
    emit_state(app, label, &s);
    Ok(())
}

/// Proporción forzada (`16:9`; vacío = la del vídeo), como la tecla A de VLC. Quita el recorte.
/// Vale solo para esta sesión.
pub fn set_aspect(app: &AppHandle, label: &str, ratio: &str) -> Result<()> {
    let s = session(app, label)?;
    let m = s.mpv()?;
    m.set("video-crop", "")?;
    m.set("video-aspect-override", if ratio.is_empty() { "no" } else { ratio })?;
    s.with_state(|st| {
        st.aspect = ratio.to_string();
        st.crop.clear();
    });
    emit_state(app, label, &s);
    Ok(())
}

/// Recorta la imagen a una proporción (`2.39:1`; vacío = sin recorte), como la tecla C de VLC.
/// Quita la proporción forzada. Vale solo para esta sesión.
pub fn set_crop(app: &AppHandle, label: &str, ratio: &str) -> Result<()> {
    let s = session(app, label)?;
    let m = s.mpv()?;
    m.set("video-aspect-override", "no")?;
    let value = ratio_value(ratio);
    if value <= 0.0 {
        m.set("video-crop", "")?;
    } else {
        let num = |k: &str| m.get(&format!("video-params/{k}")).and_then(|v| v.parse::<f64>().ok());
        let (Some(w), Some(h)) = (num("w"), num("h")) else { bail!("no_video") };
        m.set("video-crop", &crop_rect(w, h, num("par").unwrap_or(1.0), value))?;
    }
    s.with_state(|st| {
        st.crop = if value > 0.0 { ratio.to_string() } else { String::new() };
        st.aspect.clear();
    });
    emit_state(app, label, &s);
    Ok(())
}

/// `2.39:1` → 2,39; `16:9` → 1,78. Vacío o mal escrito → 0.
fn ratio_value(ratio: &str) -> f64 {
    let mut parts = ratio.split(':').map(|p| p.trim().parse::<f64>().ok());
    match (parts.next().flatten(), parts.next().unwrap_or(Some(1.0))) {
        (Some(w), Some(h)) if w > 0.0 && h > 0.0 => w / h,
        _ => 0.0,
    }
}

/// Rectángulo `video-crop` (`AnchoxAlto+x+y`, en píxeles del vídeo) que deja la imagen en
/// `ratio`, centrado. `par` es la proporción de cada píxel (los DVD anamórficos no son cuadrados).
fn crop_rect(w: f64, h: f64, par: f64, ratio: f64) -> String {
    let display = w * par / h;
    let (cw, ch) = if display > ratio { ((w * ratio / display).round(), h) } else { (w, (h * display / ratio).round()) };
    format!("{cw}x{ch}+{}+{}", ((w - cw) / 2.0).round(), ((h - ch) / 2.0).round())
}

pub fn close(app: &AppHandle, label: &str) {
    match session(app, label).and_then(|s| s.mpv()) {
        Ok(m) => {
            let _ = m.command(&["quit"]);
            m.wakeup();
        }
        // Sin mpv (p. ej. falló al arrancar): se cierra la ventana sin más.
        Err(_) => {
            if let Some(w) = app.get_webview_window(label) {
                let _ = w.destroy();
            }
        }
    }
}

/// Cierra todas las películas (cada una guarda por dónde iba).
pub fn close_all(app: &AppHandle) {
    let labels: Vec<String> = sessions(app).keys().cloned().collect();
    for label in labels {
        close(app, &label);
    }
}

/// Trae al frente la ventana del reproductor.
pub fn focus(app: &AppHandle, label: &str) {
    if let Some(w) = app.get_webview_window(label) {
        let _ = w.unminimize();
        let _ = w.show();
        let _ = w.set_focus();
    }
}

/// Trae al frente el reproductor de esa película, si se está viendo en el integrado.
pub fn focus_item(app: &AppHandle, item_id: i64) -> bool {
    let label = sessions(app).iter().find(|(_, s)| s.item_id == item_id).map(|(l, _)| l.clone());
    match label {
        Some(l) => {
            focus(app, &l);
            true
        }
        None => false,
    }
}

pub fn set_fullscreen(app: &AppHandle, label: &str, on: bool) -> Result<()> {
    let s = session(app, label)?;
    let Some(w) = app.get_webview_window(label) else { bail!("not_playing") };
    w.set_fullscreen(on)?;
    s.with_state(|st| st.fullscreen = on);
    emit_state(app, label, &s);
    Ok(())
}

/// Siempre encima de las demás ventanas. Se recuerda para las próximas películas.
pub fn set_ontop(app: &AppHandle, label: &str, on: bool) -> Result<()> {
    let s = session(app, label)?;
    let Some(w) = app.get_webview_window(label) else { bail!("not_playing") };
    w.set_always_on_top(on)?;
    s.with_state(|st| st.ontop = on);
    let _ = db::set_setting(&app.state::<AppState>().db(), "playerOntop", if on { "true" } else { "false" });
    emit_state(app, label, &s);
    Ok(())
}

#[cfg(test)]
mod tests {
    use super::*;

    fn opts(pairs: &[(&str, &str)]) -> HashMap<&'static str, String> {
        let settings: HashMap<String, String> = pairs.iter().map(|(k, v)| (k.to_string(), v.to_string())).collect();
        preferences(&settings, &SubStyle::from_settings(&settings)).into_iter().collect()
    }

    #[test]
    fn default_subtitle_style() {
        let o = opts(&[]);
        assert_eq!(o["sub-scale"], "1.00");
        // 3 % del alto: el margen de siempre de mpv (22 de 720).
        assert_eq!(o["sub-margin-y"], "22");
        assert_eq!(o["sub-border-style"], "background-box");
        assert_eq!(o["sub-back-color"], "#A6000000");
        assert_eq!(o["sub-bold"], "no");
        // Sin contorno en las letras por defecto.
        assert_eq!(o["sub-outline-color"], "#00000000");
        assert!(!o.contains_key("sub-font"));
    }

    #[test]
    fn custom_subtitle_style_and_limits() {
        let o = opts(&[
            ("subScale", "1.25"),
            ("subHeight", "10"),
            ("subBackground", "false"),
            ("subBackOpacity", "100"),
            ("subFont", "Verdana"),
            ("subBold", "true"),
            ("subOutline", "true"),
            ("subBorder", "50"),
            ("subColor", "#FFD83D"),
        ]);
        assert_eq!(o["sub-scale"], "1.25");
        assert_eq!(o["sub-margin-y"], "72");
        assert_eq!(o["sub-border-style"], "outline-and-shadow");
        assert_eq!(o["sub-back-color"], "#FF000000");
        assert_eq!(o["sub-outline-color"], "#FF000000");
        assert_eq!(o["sub-font"], "Verdana");
        assert_eq!(o["sub-bold"], "yes");
        // Valores fuera de rango se recortan.
        assert_eq!(o["sub-border-size"], "8.0");
        assert_eq!(o["sub-color"], "#FFD83D");
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
            sub_scale: 1.0,
            sub_margin: 22.0 * 2.0,
            sub_background: true,
            ..Default::default()
        };
        let general = SubStyle::from_settings(&HashMap::new());
        let p = current_prefs(&s, &general);
        assert_eq!(p.aid.as_deref(), Some("2"));
        assert_eq!(p.sid.as_deref(), Some("no"));
        assert_eq!((p.audio_delay, p.sub_delay), (0.1, -0.2));
        // Tamaño y fondo como en Ajustes: no se guardan (siguen lo general). La altura sí cambió.
        assert_eq!(p.sub_scale, None);
        assert_eq!(p.sub_background, None);
        assert!(p.sub_margin.is_some_and(|m| (m - 44.0).abs() < 0.001));
    }

    #[test]
    fn crop_keeps_the_middle() {
        // 2,39:1 de un vídeo 16:9 de 1920x1080: bandas arriba y abajo fuera.
        assert_eq!(crop_rect(1920.0, 1080.0, 1.0, 2.39), "1920x803+0+139");
        // 4:3 de un 16:9: se quitan los lados.
        assert_eq!(crop_rect(1920.0, 1080.0, 1.0, 4.0 / 3.0), "1440x1080+240+0");
        // DVD anamórfico (720x576 que se ve 16:9) recortado a 4:3.
        assert_eq!(crop_rect(720.0, 576.0, 64.0 / 45.0, 4.0 / 3.0), "540x576+90+0");
    }

    #[test]
    fn ratios() {
        assert!((ratio_value("2.39:1") - 2.39).abs() < 1e-9);
        assert!((ratio_value("16:9") - 16.0 / 9.0).abs() < 1e-9);
        assert_eq!(ratio_value(""), 0.0);
        assert_eq!(ratio_value("x:1"), 0.0);
    }

    #[test]
    fn file_style_goes_over_the_general_one() {
        let general = SubStyle::from_settings(&HashMap::new());
        let file = db::FilePrefs { sub_scale: Some(1.5), sub_margin: Some(72.0), ..Default::default() };
        let style = general.clone().with_file(Some(&file));
        assert_eq!(style.scale, 1.5);
        assert!((style.margin() - 72.0).abs() < 0.001);
        // Lo que no se cambió para el archivo, de Ajustes.
        assert_eq!(style.background, general.background);
    }
}
