//! La ventana principal y el escritorio: icono en el área de notificación (cerrar y minimizar a
//! él), esconderse mientras se ve una película, recordar el tamaño y la posición de la ventana e
//! iniciar con Windows.

use crate::{AppState, db, player};
use anyhow::{Result, anyhow};
use std::collections::HashMap;
use std::sync::Mutex;
use std::sync::atomic::{AtomicBool, Ordering};
use tauri::menu::{Menu, MenuItem, PredefinedMenuItem};
use tauri::tray::{MouseButton, MouseButtonState, TrayIconBuilder, TrayIconEvent};
use tauri::{AppHandle, Manager, PhysicalPosition, PhysicalSize, WebviewWindow, WindowEvent};

const TRAY: &str = "main";
/// Argumento con el que Windows arranca K-Movies al iniciar sesión.
pub const AUTOSTART_ARG: &str = "--autostart";

#[derive(Default)]
pub struct Desktop {
    /// Textos del menú del icono (los pone la interfaz, en su idioma): mostrar y salir.
    labels: Mutex<Option<(String, String)>>,
    /// Último tamaño y posición sin maximizar ni minimizar (en píxeles reales).
    normal: Mutex<Option<(i32, i32, u32, u32)>>,
    /// Se escondió o minimizó al empezar una película: al terminar se vuelve a enseñar.
    hidden_for_play: AtomicBool,
    /// "Salir" desde el icono: cerrar la ventana cierra de verdad.
    quitting: AtomicBool,
    /// La interfaz ya ha pintado y la ventana se ha enseñado (o se ha dejado escondida).
    ready: AtomicBool,
}

fn desktop(app: &AppHandle) -> &Desktop {
    &app.state::<AppState>().inner().desktop
}

fn main_window(app: &AppHandle) -> Option<WebviewWindow> {
    app.get_webview_window("main")
}

fn settings(app: &AppHandle) -> HashMap<String, String> {
    db::settings(&app.state::<AppState>().db()).unwrap_or_default()
}

fn flag(settings: &HashMap<String, String>, key: &str) -> bool {
    settings.get(key).is_some_and(|v| v == "true")
}


// ---------------------------------------------------------------------------
// Icono del área de notificación

fn menu(app: &AppHandle) -> tauri::Result<Menu<tauri::Wry>> {
    let (show, quit) = desktop(app).labels.lock().unwrap_or_else(|e| e.into_inner()).clone().unwrap_or(("K-Movies".into(), "Salir".into()));
    Menu::with_items(
        app,
        &[
            &MenuItem::with_id(app, "show", show, true, None::<&str>)?,
            &PredefinedMenuItem::separator(app)?,
            &MenuItem::with_id(app, "quit", quit, true, None::<&str>)?,
        ],
    )
}

fn create_tray(app: &AppHandle) -> Result<()> {
    let icon = app.default_window_icon().cloned().ok_or_else(|| anyhow!("sin icono"))?;
    TrayIconBuilder::with_id(TRAY)
        .icon(icon)
        .tooltip("K-Movies")
        .menu(&menu(app)?)
        .show_menu_on_left_click(false)
        .on_menu_event(|app, e| match e.id().as_ref() {
            "show" => show_main(app),
            "quit" => quit(app),
            _ => {}
        })
        .on_tray_icon_event(|tray, e| {
            if let TrayIconEvent::Click { button: MouseButton::Left, button_state: MouseButtonState::Up, .. } = e {
                show_main(tray.app_handle());
            }
        })
        .build(app)?;
    Ok(())
}

/// Se asegura de que el icono está a la vista (para poder esconder la ventana en él).
fn ensure_tray(app: &AppHandle) -> bool {
    match app.tray_by_id(TRAY) {
        Some(t) => t.set_visible(true).is_ok(),
        None => create_tray(app).map_err(|e| eprintln!("[tray] {e:#}")).is_ok(),
    }
}

/// El icono está siempre. Esto pone los textos de su menú (en el idioma de la interfaz); lo llama la
/// interfaz al arrancar y al cambiar de idioma.
pub fn update(app: &AppHandle, show: String, quit: String) -> Result<()> {
    *desktop(app).labels.lock().unwrap_or_else(|e| e.into_inner()) = Some((show, quit));
    match app.tray_by_id(TRAY) {
        Some(t) => t.set_menu(Some(menu(app)?))?,
        None => create_tray(app)?,
    }
    Ok(())
}

pub fn show_main(app: &AppHandle) {
    if let Some(w) = main_window(app) {
        let _ = w.unminimize();
        let _ = w.show();
        let _ = w.set_focus();
    }
}

/// "Salir" desde el icono: cierra las películas (guardan por dónde iban) y la ventana.
fn quit(app: &AppHandle) {
    desktop(app).quitting.store(true, Ordering::SeqCst);
    player::close_all(app);
    match main_window(app) {
        Some(w) => {
            let _ = w.close();
        }
        None => app.exit(0),
    }
}

// ---------------------------------------------------------------------------
// Ventana principal

/// ¿Se ve esa ventana en alguna pantalla? (por si se desconectó el monitor donde estaba)
pub fn on_screen(app: &AppHandle, x: i32, y: i32, width: u32) -> bool {
    let (px, py) = (x + width as i32 / 2, y + 16);
    app.available_monitors().unwrap_or_default().iter().any(|m| {
        let (p, s) = (m.position(), m.size());
        px >= p.x && px < p.x + s.width as i32 && py >= p.y && py < p.y + s.height as i32
    })
}

/// Al arrancar (con la ventana aún oculta): el tamaño y la posición de la última vez.
pub fn restore_geometry(app: &AppHandle) {
    let Some(w) = main_window(app) else { return };
    let s = settings(app);
    let v: Vec<i64> = s.get("mainWindow").map(|g| g.split(',').filter_map(|n| n.trim().parse().ok()).collect()).unwrap_or_default();
    if let [x, y, width, height] = v[..]
        && width >= 600
        && height >= 400
        && on_screen(app, x as i32, y as i32, width as u32)
    {
        let _ = w.set_size(PhysicalSize::new(width as u32, height as u32));
        let _ = w.set_position(PhysicalPosition::new(x as i32, y as i32));
        *desktop(app).normal.lock().unwrap_or_else(|e| e.into_inner()) = Some((x as i32, y as i32, width as u32, height as u32));
    }
}

fn save_geometry(app: &AppHandle, w: &WebviewWindow) {
    let state = app.state::<AppState>();
    let conn = state.db();
    if let Some((x, y, width, height)) = *desktop(app).normal.lock().unwrap_or_else(|e| e.into_inner()) {
        let _ = db::set_setting(&conn, "mainWindow", &format!("{x},{y},{width},{height}"));
    }
    if let Ok(max) = w.is_maximized() {
        let _ = db::set_setting(&conn, "mainMaximized", if max { "true" } else { "false" });
    }
}

/// La interfaz ya ha pintado: se enseña la ventana (arranca oculta para evitar el destello blanco).
/// Con "Iniciar minimizado" (al abrirla a mano o al iniciar Windows) se queda en el icono (o
/// minimizada, si no hay icono).
pub fn main_ready(app: &AppHandle) {
    if desktop(app).ready.swap(true, Ordering::SeqCst) {
        return;
    }
    let Some(w) = main_window(app) else { return };
    let s = settings(app);
    if flag(&s, "startMinimized") {
        if ensure_tray(app) {
            return;
        }
        let _ = w.show();
        let _ = w.minimize();
        return;
    }
    let _ = w.show();
    if flag(&s, "mainMaximized") {
        let _ = w.maximize();
    }
}

/// Eventos de la ventana principal: cerrar o minimizar al icono, y recordar su tamaño y posición.
pub fn on_main_event(app: &AppHandle, w: &WebviewWindow, event: &WindowEvent) {
    match event {
        WindowEvent::CloseRequested { api, .. } => {
            save_geometry(app, w);
            if !desktop(app).quitting.load(Ordering::SeqCst) && flag(&settings(app), "closeToTray") && ensure_tray(app) {
                api.prevent_close();
                let _ = w.hide();
                return;
            }
            // Cerrar K-Movies cierra también las películas (y guarda por dónde ibas).
            player::close_all(app);
        }
        WindowEvent::Moved(_) | WindowEvent::Resized(_) => {
            if w.is_minimized().unwrap_or(false) {
                if w.is_visible().unwrap_or(false) && flag(&settings(app), "minimizeToTray") && ensure_tray(app) {
                    let _ = w.hide();
                }
                return;
            }
            if !w.is_maximized().unwrap_or(false)
                && w.is_visible().unwrap_or(false)
                && let (Ok(p), Ok(s)) = (w.outer_position(), w.inner_size())
            {
                *desktop(app).normal.lock().unwrap_or_else(|e| e.into_inner()) = Some((p.x, p.y, s.width, s.height));
            }
        }
        _ => {}
    }
}

/// Al empezar una película: minimizar K-Movies o esconderlo en el icono, si así se ha elegido.
pub fn playback_started(app: &AppHandle) {
    let Some(w) = main_window(app) else { return };
    if !w.is_visible().unwrap_or(false) || w.is_minimized().unwrap_or(false) {
        return;
    }
    let done = match settings(app).get("playMinimize").map(String::as_str) {
        Some("minimize") => w.minimize().is_ok(),
        Some("tray") if ensure_tray(app) => w.hide().is_ok(),
        Some("tray") => w.minimize().is_ok(),
        _ => false,
    };
    if done {
        desktop(app).hidden_for_play.store(true, Ordering::SeqCst);
    }
}

/// Ya no se ve ninguna película: si K-Movies se escondió al empezar, vuelve (salvo que se haya
/// quitado "Al terminar, volver a mostrar K-Movies").
pub fn playback_ended(app: &AppHandle) {
    if !desktop(app).hidden_for_play.swap(false, Ordering::SeqCst) {
        return;
    }
    if settings(app).get("playRestore").is_some_and(|v| v == "false") {
        return;
    }
    if let Some(w) = main_window(app)
        && (!w.is_visible().unwrap_or(true) || w.is_minimized().unwrap_or(false))
    {
        show_main(app);
    }
}

// ---------------------------------------------------------------------------
// Iniciar con Windows: valor en `HKCU\...\Run` (solo para este usuario, sin pedir administrador).

const RUN_KEY: &str = r"Software\Microsoft\Windows\CurrentVersion\Run";
const RUN_VALUE: &str = "K-Movies";

fn wide(s: &str) -> Vec<u16> {
    s.encode_utf16().chain(std::iter::once(0)).collect()
}

fn autostart_command() -> Result<String> {
    Ok(format!("\"{}\" {AUTOSTART_ARG}", std::env::current_exe()?.display()))
}

fn read_run() -> Option<String> {
    use windows_sys::Win32::System::Registry::{HKEY_CURRENT_USER, RRF_RT_REG_SZ, RegGetValueW};
    let (key, value) = (wide(RUN_KEY), wide(RUN_VALUE));
    let mut buf = vec![0u16; 2048];
    let mut size = (buf.len() * 2) as u32;
    // SAFETY: cadenas terminadas en 0 y un búfer del tamaño que se indica.
    let rc = unsafe {
        RegGetValueW(
            HKEY_CURRENT_USER,
            key.as_ptr(),
            value.as_ptr(),
            RRF_RT_REG_SZ,
            std::ptr::null_mut(),
            buf.as_mut_ptr().cast(),
            &mut size,
        )
    };
    if rc != 0 {
        return None;
    }
    let len = (size as usize / 2).saturating_sub(1);
    Some(String::from_utf16_lossy(&buf[..len.min(buf.len())]))
}

pub fn autostart_enabled() -> bool {
    read_run().is_some()
}

pub fn set_autostart(on: bool) -> Result<()> {
    use windows_sys::Win32::System::Registry::{HKEY_CURRENT_USER, REG_SZ, RegDeleteKeyValueW, RegSetKeyValueW};
    let (key, value) = (wide(RUN_KEY), wide(RUN_VALUE));
    // SAFETY: cadenas terminadas en 0; el dato es la cadena con su 0 final.
    let rc = unsafe {
        if on {
            let data = wide(&autostart_command()?);
            RegSetKeyValueW(HKEY_CURRENT_USER, key.as_ptr(), value.as_ptr(), REG_SZ, data.as_ptr().cast(), (data.len() * 2) as u32)
        } else {
            match RegDeleteKeyValueW(HKEY_CURRENT_USER, key.as_ptr(), value.as_ptr()) {
                2 => 0, // ERROR_FILE_NOT_FOUND: ya no estaba
                rc => rc,
            }
        }
    };
    if rc != 0 {
        return Err(anyhow!("registro: error {rc}"));
    }
    Ok(())
}

/// K-Movies es un solo ejecutable que se puede mover: si está apuntado para iniciar con Windows
/// desde otra carpeta, se corrige la ruta.
pub fn refresh_autostart() {
    // En desarrollo no: apuntaría al ejecutable de pruebas.
    if cfg!(debug_assertions) {
        return;
    }
    if let (Some(current), Ok(wanted)) = (read_run(), autostart_command())
        && current != wanted
        && let Err(e) = set_autostart(true)
    {
        eprintln!("[autostart] {e:#}");
    }
}
