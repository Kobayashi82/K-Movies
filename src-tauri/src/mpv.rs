//! Enlace mínimo con libmpv (`libmpv-2.dll`), cargada en tiempo de ejecución: si no está, la app
//! funciona igual y reproduce con el reproductor externo.

use anyhow::{Result, anyhow, bail};
use libloading::Library;
use std::ffi::{CStr, CString, c_char, c_int, c_void};
use std::path::{Path, PathBuf};
use std::sync::OnceLock;

type Handle = *mut c_void;

#[repr(C)]
struct RawEvent {
    event_id: c_int,
    error: c_int,
    reply_userdata: u64,
    data: *mut c_void,
}

#[repr(C)]
struct RawProperty {
    name: *const c_char,
    format: c_int,
    data: *mut c_void,
}

#[repr(C)]
struct RawEndFile {
    reason: c_int,
    error: c_int,
}

const FORMAT_NONE: c_int = 0;
const FORMAT_FLAG: c_int = 3;
const FORMAT_DOUBLE: c_int = 5;

const EVENT_SHUTDOWN: c_int = 1;
const EVENT_END_FILE: c_int = 7;
const EVENT_FILE_LOADED: c_int = 8;
const EVENT_VIDEO_RECONFIG: c_int = 17;
const EVENT_PLAYBACK_RESTART: c_int = 21;
const EVENT_PROPERTY_CHANGE: c_int = 22;

struct Api {
    create: unsafe extern "C" fn() -> Handle,
    initialize: unsafe extern "C" fn(Handle) -> c_int,
    set_option_string: unsafe extern "C" fn(Handle, *const c_char, *const c_char) -> c_int,
    set_property_string: unsafe extern "C" fn(Handle, *const c_char, *const c_char) -> c_int,
    get_property_string: unsafe extern "C" fn(Handle, *const c_char) -> *mut c_char,
    command: unsafe extern "C" fn(Handle, *mut *const c_char) -> c_int,
    observe_property: unsafe extern "C" fn(Handle, u64, *const c_char, c_int) -> c_int,
    wait_event: unsafe extern "C" fn(Handle, f64) -> *mut RawEvent,
    wakeup: unsafe extern "C" fn(Handle),
    terminate_destroy: unsafe extern "C" fn(Handle),
    free: unsafe extern "C" fn(*mut c_void),
    error_string: unsafe extern "C" fn(c_int) -> *const c_char,
    _lib: Library,
}

static API: OnceLock<Option<Api>> = OnceLock::new();

/// libmpv comprimida dentro del ejecutable (ver `build.rs`). Vacía en desarrollo o si no estaba
/// la DLL al compilar.
static EMBEDDED: &[u8] = include_bytes!(concat!(env!("OUT_DIR"), "/libmpv-2.dll.zst"));
/// Nombre con el que se descomprime (lleva la huella de esa versión).
const EMBEDDED_NAME: &str = env!("KMOVIES_LIBMPV_NAME");

/// Dónde se busca la DLL suelta: junto al ejecutable (instalaciones antiguas, o si se quiere
/// probar otra versión) y, en desarrollo, en `src-tauri/lib`.
fn candidates() -> Vec<PathBuf> {
    let mut out = Vec::new();
    if let Some(dir) = std::env::current_exe().ok().and_then(|p| p.parent().map(Path::to_path_buf)) {
        out.push(dir.join("libmpv-2.dll"));
    }
    if cfg!(debug_assertions) {
        out.push(Path::new(env!("CARGO_MANIFEST_DIR")).join("lib").join("libmpv-2.dll"));
    }
    out
}

/// Carpeta donde se descomprime (`<datos>\lib`); la fija `prepare` al arrancar.
static LIB_DIR: OnceLock<PathBuf> = OnceLock::new();

/// Descomprime la libmpv del ejecutable en `<datos>\lib` (solo la primera vez de cada versión) y
/// devuelve su ruta. Borra las de versiones anteriores.
fn extract_embedded() -> Option<PathBuf> {
    if EMBEDDED.is_empty() {
        return None;
    }
    let dir = LIB_DIR.get()?.clone();
    let path = dir.join(EMBEDDED_NAME);
    if path.is_file() {
        return Some(path);
    }
    // A un nombre temporal y luego se renombra: una descompresión cortada nunca deja una DLL a medias.
    let part = dir.join(format!("{EMBEDDED_NAME}.{}.part", std::process::id()));
    let written = std::fs::create_dir_all(&dir).and_then(|()| {
        let mut file = std::fs::File::create(&part)?;
        zstd::stream::copy_decode(EMBEDDED, &mut file)?;
        file.sync_all()
    });
    if let Err(e) = written.and_then(|()| std::fs::rename(&part, &path)) {
        let _ = std::fs::remove_file(&part);
        // Otra ventana de K-Movies pudo descomprimirla a la vez.
        if !path.is_file() {
            eprintln!("[mpv] no se pudo descomprimir libmpv: {e}");
            return None;
        }
    }
    for entry in std::fs::read_dir(&dir).into_iter().flatten().flatten() {
        let name = entry.file_name().to_string_lossy().into_owned();
        if name.starts_with("libmpv-2-") && name != EMBEDDED_NAME {
            let _ = std::fs::remove_file(entry.path());
        }
    }
    Some(path)
}

fn load() -> Option<Api> {
    candidates().iter().filter(|p| p.is_file()).find_map(|p| open(p)).or_else(|| open(&extract_embedded()?))
}

fn open(path: &Path) -> Option<Api> {
    {
        // SAFETY: es libmpv; los símbolos se declaran con las firmas de `mpv/client.h` (API 2.x).
        unsafe {
            let lib = Library::new(path).ok()?;
            macro_rules! sym {
                ($name:literal) => {
                    lib.get($name).ok().map(|s| *s)?
                };
            }
            Some(Api {
                create: sym!(b"mpv_create\0"),
                initialize: sym!(b"mpv_initialize\0"),
                set_option_string: sym!(b"mpv_set_option_string\0"),
                set_property_string: sym!(b"mpv_set_property_string\0"),
                get_property_string: sym!(b"mpv_get_property_string\0"),
                command: sym!(b"mpv_command\0"),
                observe_property: sym!(b"mpv_observe_property\0"),
                wait_event: sym!(b"mpv_wait_event\0"),
                wakeup: sym!(b"mpv_wakeup\0"),
                terminate_destroy: sym!(b"mpv_terminate_destroy\0"),
                free: sym!(b"mpv_free\0"),
                error_string: sym!(b"mpv_error_string\0"),
                _lib: lib,
            })
        }
    }
}

fn api() -> Option<&'static Api> {
    API.get_or_init(load).as_ref()
}

/// ¿Está libmpv disponible? La primera vez la carga (y, si va dentro del ejecutable, la descomprime).
pub fn available() -> bool {
    api().is_some()
}

/// Deja libmpv lista en segundo plano al arrancar (descomprimida en `lib_dir` si va dentro del
/// ejecutable), para que el primer "Reproducir" no espere.
pub fn prepare(lib_dir: PathBuf) {
    let _ = LIB_DIR.set(lib_dir);
    std::thread::spawn(available);
}

#[derive(Debug, Clone)]
pub enum Value {
    None,
    Flag(bool),
    Double(f64),
}

#[derive(Debug)]
pub enum Event {
    Shutdown,
    FileLoaded,
    VideoReconfig,
    /// Empieza a reproducirse (al cargar el archivo o tras un salto): ya hay imagen.
    PlaybackRestart,
    /// `reason`: 0 fin del archivo, 2 parada, 3 salir, 4 error.
    EndFile { reason: i32, error: i32 },
    Property { name: String, value: Value },
    Other,
}

/// Formato en el que se quiere recibir una propiedad observada.
#[derive(Clone, Copy)]
pub enum Format {
    /// Solo el aviso de que cambió (se lee aparte, p. ej. listas como JSON).
    None,
    Flag,
    Double,
}

/// Una instancia de mpv. La API de cliente de mpv es segura entre hilos.
pub struct Mpv {
    api: &'static Api,
    handle: Handle,
}

unsafe impl Send for Mpv {}
unsafe impl Sync for Mpv {}

fn cstr(s: &str) -> Result<CString> {
    CString::new(s).map_err(|_| anyhow!("nul in string"))
}

impl Mpv {
    /// Crea mpv con estas opciones (antes de inicializar), p. ej. `wid` para dibujar en una ventana.
    pub fn new(options: &[(&str, &str)]) -> Result<Self> {
        let api = api().ok_or_else(|| anyhow!("libmpv_missing"))?;
        // SAFETY: llamadas según la documentación de client.h; `handle` se destruye en Drop.
        unsafe {
            let handle = (api.create)();
            if handle.is_null() {
                bail!("mpv_create failed");
            }
            let mpv = Mpv { api, handle };
            for (k, v) in options {
                let (k, v) = (cstr(k)?, cstr(v)?);
                mpv.check((api.set_option_string)(handle, k.as_ptr(), v.as_ptr()), k.to_str().unwrap_or(""))?;
            }
            mpv.check((api.initialize)(handle), "initialize")?;
            Ok(mpv)
        }
    }

    fn check(&self, code: c_int, what: &str) -> Result<()> {
        if code >= 0 {
            return Ok(());
        }
        // SAFETY: mpv_error_string devuelve una cadena estática.
        let msg = unsafe { CStr::from_ptr((self.api.error_string)(code)) }.to_string_lossy();
        bail!("mpv {what}: {msg}")
    }

    pub fn command(&self, args: &[&str]) -> Result<()> {
        let owned: Vec<CString> = args.iter().map(|a| cstr(a)).collect::<Result<_>>()?;
        let mut ptrs: Vec<*const c_char> = owned.iter().map(|c| c.as_ptr()).collect();
        ptrs.push(std::ptr::null());
        // SAFETY: lista terminada en NULL; las cadenas viven hasta que vuelve la llamada.
        self.check(unsafe { (self.api.command)(self.handle, ptrs.as_mut_ptr()) }, args.first().copied().unwrap_or(""))
    }

    pub fn set(&self, name: &str, value: &str) -> Result<()> {
        let (n, v) = (cstr(name)?, cstr(value)?);
        // SAFETY: cadenas válidas durante la llamada.
        self.check(unsafe { (self.api.set_property_string)(self.handle, n.as_ptr(), v.as_ptr()) }, name)
    }

    /// Valor de una propiedad como texto (las listas, como `track-list`, vienen en JSON).
    pub fn get(&self, name: &str) -> Option<String> {
        let n = cstr(name).ok()?;
        // SAFETY: lo que devuelve mpv se copia y se libera con mpv_free.
        unsafe {
            let p = (self.api.get_property_string)(self.handle, n.as_ptr());
            if p.is_null() {
                return None;
            }
            let s = CStr::from_ptr(p).to_string_lossy().into_owned();
            (self.api.free)(p as *mut c_void);
            Some(s)
        }
    }

    pub fn observe(&self, name: &str, format: Format) -> Result<()> {
        let n = cstr(name)?;
        let f = match format {
            Format::None => FORMAT_NONE,
            Format::Flag => FORMAT_FLAG,
            Format::Double => FORMAT_DOUBLE,
        };
        // SAFETY: nombre válido durante la llamada.
        self.check(unsafe { (self.api.observe_property)(self.handle, 0, n.as_ptr(), f) }, name)
    }

    /// Espera el siguiente evento (`timeout` en segundos; negativo = sin límite).
    pub fn wait_event(&self, timeout: f64) -> Option<Event> {
        // SAFETY: el evento lo gestiona mpv y es válido hasta la siguiente llamada a wait_event;
        // todo lo que se usa se copia antes de volver.
        unsafe {
            let ev = &*(self.api.wait_event)(self.handle, timeout);
            Some(match ev.event_id {
                0 => return None,
                EVENT_SHUTDOWN => Event::Shutdown,
                EVENT_FILE_LOADED => Event::FileLoaded,
                EVENT_VIDEO_RECONFIG => Event::VideoReconfig,
                EVENT_PLAYBACK_RESTART => Event::PlaybackRestart,
                EVENT_END_FILE => {
                    let end = &*(ev.data as *const RawEndFile);
                    Event::EndFile { reason: end.reason, error: end.error }
                }
                EVENT_PROPERTY_CHANGE => {
                    let prop = &*(ev.data as *const RawProperty);
                    let name = CStr::from_ptr(prop.name).to_string_lossy().into_owned();
                    let value = if prop.data.is_null() {
                        Value::None
                    } else {
                        match prop.format {
                            FORMAT_FLAG => Value::Flag(*(prop.data as *const c_int) != 0),
                            FORMAT_DOUBLE => Value::Double(*(prop.data as *const f64)),
                            _ => Value::None,
                        }
                    };
                    Event::Property { name, value }
                }
                _ => Event::Other,
            })
        }
    }

    /// Despierta un `wait_event` que esté esperando.
    pub fn wakeup(&self) {
        // SAFETY: handle válido.
        unsafe { (self.api.wakeup)(self.handle) }
    }
}

impl Drop for Mpv {
    fn drop(&mut self) {
        // SAFETY: último uso del handle.
        unsafe { (self.api.terminate_destroy)(self.handle) }
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    /// Las opciones que usa K-Movies existen en la libmpv instalada (se salta si no está la DLL).
    #[test]
    fn options_exist() {
        if !available() {
            eprintln!("libmpv no está: prueba saltada");
            return;
        }
        let mpv = Mpv::new(&[("vo", "null"), ("ao", "null")]).unwrap();
        for (k, v) in [
            ("sub-border-style", "background-box"),
            ("sub-back-color", "#A6000000"),
            ("sub-color", "#FFD83D"),
            ("sub-bold", "yes"),
            ("sub-border-size", "3.0"),
            ("sub-shadow-offset", "1.0"),
            ("sub-outline-color", "#00000000"),
            ("sub-font", "Verdana"),
            ("sub-ass-override", "force"),
            ("sub-margin-y", "22"),
            ("alang", "es,spa,esp"),
            ("slang", "es,spa,esp"),
            ("subs-with-matching-audio", "forced"),
            ("subs-fallback", "default"),
            ("sub-auto", "fuzzy"),
            ("hwdec", "auto-safe"),
            ("sub-pos", "90"),
            ("video-aspect-override", "16:9"),
            ("video-aspect-override", "no"),
            ("video-crop", "100x100+10+10"),
            ("video-crop", ""),
        ] {
            mpv.set(k, v).unwrap_or_else(|e| panic!("{k}={v}: {e}"));
        }
    }
}
