//! Reproductores externos: detección y arranque. El seguimiento del progreso llega en la fase 3.

use anyhow::{Result, bail};
use serde::Serialize;
use std::path::{Path, PathBuf};
use std::process::Command;

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

/// Abre `file` en el reproductor, empezando en `start` segundos si se puede.
pub fn launch(player: &Path, file: &Path, start: Option<f64>) -> Result<()> {
    if !file.exists() {
        bail!("file_missing");
    }
    let mut cmd = Command::new(player);
    let start = start.filter(|s| *s > 1.0);
    match (kind_of(player), start) {
        ("vlc", Some(s)) => {
            cmd.arg(format!("--start-time={s:.0}"));
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
    cmd.spawn()?;
    Ok(())
}
