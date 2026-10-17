//! Reproductores externos: detección, arranque y lectura del estado (por dónde vas).

use anyhow::{Result, bail};
use serde::{Deserialize, Serialize};
use std::path::{Path, PathBuf};
use std::process::{Child, Command};
use std::time::Duration;

#[derive(Serialize, Clone)]
#[serde(rename_all = "camelCase")]
pub struct Player {
    /// `vlc` | `mpv` | `mpc-hc` | `mpc-be` | `potplayer` | `other`
    pub kind: String,
    pub name: String,
    pub path: String,
}

const KNOWN: &[(&str, &str, &[&str])] = &[
    ("vlc", "VLC", &["VideoLAN\\VLC\\vlc.exe"]),
    ("mpv", "mpv.net", &["mpv.net\\mpvnet.exe"]),
    ("mpv", "mpv", &["mpv\\mpv.exe"]),
    ("mpc-hc", "MPC-HC", &["MPC-HC\\mpc-hc64.exe", "MPC-HC\\mpc-hc.exe", "K-Lite Codec Pack\\MPC-HC64\\mpc-hc64.exe"]),
    ("mpc-be", "MPC-BE", &["MPC-BE x64\\mpc-be64.exe", "MPC-BE\\mpc-be.exe"]),
    ("potplayer", "PotPlayer", &["DAUM\\PotPlayer\\PotPlayerMini64.exe", "DAUM\\PotPlayer\\PotPlayerMini.exe"]),
];

/// Busca los reproductores habituales en Archivos de programa y en AppData.
pub fn detect() -> Vec<Player> {
    let roots: Vec<PathBuf> = ["ProgramFiles", "ProgramFiles(x86)", "LOCALAPPDATA"]
        .iter()
        .filter_map(|v| std::env::var_os(v).map(PathBuf::from))
        .chain(std::env::var_os("LOCALAPPDATA").map(|p| PathBuf::from(p).join("Programs")))
        .collect();
    let mut found: Vec<Player> = Vec::new();
    for (kind, name, rels) in KNOWN {
        for root in &roots {
            for rel in *rels {
                let path = root.join(rel);
                let path_s = path.to_string_lossy().into_owned();
                if path.is_file() && !found.iter().any(|p| p.path.eq_ignore_ascii_case(&path_s)) {
                    found.push(Player { kind: kind.to_string(), name: name.to_string(), path: path_s });
                }
            }
        }
    }
    found
}

pub fn kind_of(path: &Path) -> &'static str {
    let exe = path.file_name().map(|n| n.to_string_lossy().to_lowercase()).unwrap_or_default();
    match exe.as_str() {
        "vlc.exe" => "vlc",
        "mpv.exe" | "mpvnet.exe" => "mpv",
        e if e.starts_with("mpc-hc") => "mpc-hc",
        e if e.starts_with("mpc-be") => "mpc-be",
        e if e.starts_with("potplayer") => "potplayer",
        _ => "other",
    }
}

/// Nombre para mostrar: el de la lista de conocidos o, si no, el del ejecutable.
pub fn name_of(path: &Path) -> String {
    let kind = kind_of(path);
    KNOWN
        .iter()
        .find(|(k, ..)| *k == kind)
        .map(|(_, name, _)| name.to_string())
        .unwrap_or_else(|| path.file_stem().unwrap_or_default().to_string_lossy().into_owned())
}

/// Reproductor abierto por K-Movies. `vlc` está si se puede preguntar a VLC por dónde va.
pub struct Launched {
    pub child: Child,
    pub vlc: Option<VlcHttp>,
}

/// Abre `file` en el reproductor, empezando en `start` segundos si se puede.
pub fn launch(player: &Path, file: &Path, start: Option<f64>) -> Result<Launched> {
    if !file.exists() {
        bail!("file_missing");
    }
    let mut cmd = Command::new(player);
    let start = start.filter(|s| *s > 1.0);
    let mut vlc = None;
    match (kind_of(player), start) {
        ("vlc", start) => {
            let http = VlcHttp::new()?;
            // Una instancia propia aunque VLC ya esté abierto o configurado como instancia única
            // (si no, el archivo iría a la otra ventana y no podríamos seguirlo), sin su pregunta
            // de "¿continuar?" (ya empieza donde toca) y que se cierre al terminar la película.
            // Nada de esto toca la configuración de VLC: solo vale para esta ejecución.
            cmd.args([
                "--no-one-instance",
                "--no-one-instance-when-started-from-file",
                "--qt-continue=0",
                "--play-and-exit",
                "--extraintf=http",
                "--http-host=127.0.0.1",
            ]);
            cmd.arg(format!("--http-port={}", http.port)).arg(format!("--http-password={}", http.password));
            if let Some(s) = start {
                cmd.arg(format!("--start-time={s:.0}"));
            }
            vlc = Some(http);
        }
        ("mpv", Some(s)) => {
            cmd.arg(format!("--start={s:.0}"));
        }
        ("mpc-hc" | "mpc-be", Some(s)) => {
            cmd.arg("/start").arg(format!("{:.0}", s * 1000.0));
        }
        ("potplayer", Some(s)) => {
            let s = s as u64;
            cmd.arg(format!("/seek={:02}:{:02}:{:02}", s / 3600, (s / 60) % 60, s % 60));
        }
        _ => {}
    }
    cmd.arg(file);
    Ok(Launched { child: cmd.spawn()?, vlc })
}

// ---------------------------------------------------------------------------
// VLC: interfaz HTTP (`/requests/status.json`), solo en 127.0.0.1 y con contraseña de un solo uso.

pub struct VlcHttp {
    port: u16,
    password: String,
    client: reqwest::Client,
}

#[derive(Deserialize)]
pub struct VlcStatus {
    /// `playing` | `paused` | `stopped`
    pub state: String,
    /// Segundos.
    #[serde(default)]
    pub time: f64,
    #[serde(default)]
    pub length: f64,
    #[serde(default)]
    information: Option<VlcInformation>,
}

#[derive(Deserialize)]
struct VlcInformation {
    category: Option<VlcCategory>,
}

#[derive(Deserialize)]
struct VlcCategory {
    meta: Option<VlcMeta>,
}

#[derive(Deserialize)]
struct VlcMeta {
    filename: Option<String>,
}

impl VlcStatus {
    /// ¿Está reproduciendo el archivo `name`? (Si VLC no dice el nombre, se da por bueno.)
    pub fn is_file(&self, name: &str) -> bool {
        let meta = self.information.as_ref().and_then(|i| i.category.as_ref()).and_then(|c| c.meta.as_ref());
        meta.and_then(|m| m.filename.as_deref()).is_none_or(|f| f.eq_ignore_ascii_case(name))
    }
}

impl VlcHttp {
    fn new() -> Result<Self> {
        // Puerto libre que elige Windows; se suelta justo antes de que VLC lo ocupe.
        let port = std::net::TcpListener::bind(("127.0.0.1", 0))?.local_addr()?.port();
        // Sin proxy: si Windows tiene uno configurado, no debe meterse en las peticiones a 127.0.0.1.
        let client = reqwest::Client::builder().no_proxy().timeout(Duration::from_secs(2)).build()?;
        Ok(Self { port, password: random_password(), client })
    }

    pub async fn status(&self) -> Result<VlcStatus> {
        Ok(self
            .client
            .get(format!("http://127.0.0.1:{}/requests/status.json", self.port))
            .basic_auth("", Some(&self.password))
            .send()
            .await?
            .error_for_status()?
            .json()
            .await?)
    }
}

/// Contraseña aleatoria para la interfaz HTTP (solo escucha en 127.0.0.1 y dura lo que dura VLC).
fn random_password() -> String {
    use std::hash::{BuildHasher, Hasher};
    let nanos = std::time::SystemTime::now().duration_since(std::time::UNIX_EPOCH).unwrap_or_default().as_nanos();
    let part = |salt: u128| {
        let mut h = std::collections::hash_map::RandomState::new().build_hasher();
        h.write_u128(nanos ^ salt);
        h.finish()
    };
    format!("{:016x}{:016x}", part(0), part(u128::MAX))
}
