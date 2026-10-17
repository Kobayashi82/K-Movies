//! Recorre las carpetas de la biblioteca y detecta vídeos y archivos asociados.

use crate::parse::{self, Parsed};
use regex::Regex;
use std::collections::{BTreeSet, HashSet};
use std::path::{Path, PathBuf};
use std::sync::LazyLock;
use std::time::UNIX_EPOCH;
use walkdir::WalkDir;

const VIDEO_EXT: &[&str] = &[
    "mp4", "mkv", "avi", "m4v", "mov", "wmv", "mpg", "mpeg", "ts", "m2ts", "webm", "flv", "ogm", "divx",
];
const SUB_EXT: &[&str] = &["srt", "ass", "ssa", "sub", "vtt"];
/// Carpetas que nunca contienen la película principal (`cache` es la del K-Movies 4.0).
const SKIP_DIRS: &[&str] = &[
    "cache", "extras", "featurettes", "trailers", "behind the scenes", "deleted scenes", "interviews",
    "shorts", "sample", "samples", "$recycle.bin", "system volume information", ".actors",
];

pub struct Video {
    pub path: PathBuf,
    pub size: u64,
    pub mtime: i64,
}

pub struct Hints {
    pub parsed: Parsed,
    pub imdb_id: Option<String>,
    pub tmdb_id: Option<i64>,
    pub has_subs: bool,
}

/// Resultado de recorrer una carpeta. Las rutas van en minúsculas para comprobar subtítulos
/// y .nfo sin volver a tocar el disco (importante en unidades de red).
pub struct Walk {
    pub videos: Vec<Video>,
    pub files: HashSet<String>,
    /// Rutas de subtítulos sin extensión, ordenadas para buscar por prefijo.
    pub subs: BTreeSet<String>,
}

pub fn walk(root: &Path) -> Walk {
    let mut videos = Vec::new();
    let mut all = HashSet::new();
    let mut subs = BTreeSet::new();
    let walker = WalkDir::new(root).follow_links(true).into_iter().filter_entry(|e| {
        let name = e.file_name().to_string_lossy().to_lowercase();
        !(e.file_type().is_dir() && e.depth() > 0 && (SKIP_DIRS.contains(&name.as_str()) || name.starts_with('.')))
    });
    for entry in walker.filter_map(Result::ok) {
        if !entry.file_type().is_file() {
            continue;
        }
        let path = entry.path();
        all.insert(path.to_string_lossy().to_lowercase());
        let ext = path.extension().and_then(|e| e.to_str()).unwrap_or_default().to_lowercase();
        if SUB_EXT.contains(&ext.as_str()) {
            subs.insert(path.with_extension("").to_string_lossy().to_lowercase());
            continue;
        }
        if !VIDEO_EXT.contains(&ext.as_str()) {
            continue;
        }
        let Ok(meta) = entry.metadata() else { continue };
        let stem = path.file_stem().unwrap_or_default().to_string_lossy().to_lowercase();
        // Muestras de las descargas: "sample.mkv", "pelicula-sample.mkv".
        if (stem == "sample" || stem.ends_with("-sample") || stem.ends_with(".sample")) && meta.len() < 500_000_000 {
            continue;
        }
        let mtime = meta
            .modified()
            .ok()
            .and_then(|t| t.duration_since(UNIX_EPOCH).ok())
            .map(|d| d.as_secs() as i64)
            .unwrap_or(0);
        videos.push(Video { path: path.to_path_buf(), size: meta.len(), mtime });
    }
    Walk { videos, files: all, subs }
}

static NFO_IMDB: LazyLock<Regex> = LazyLock::new(|| {
    Regex::new(r#"(?i)<imdb_?id>\s*(tt\d+)|<uniqueid[^>]*type="imdb"[^>]*>\s*(tt\d+)|imdb\.com/title/(tt\d+)"#).unwrap()
});
static NFO_TMDB: LazyLock<Regex> =
    LazyLock::new(|| Regex::new(r#"(?i)<tmdbid>\s*(\d+)|<uniqueid[^>]*type="tmdb"[^>]*>\s*(\d+)"#).unwrap());
static KM4_TXT: LazyLock<Regex> = LazyLock::new(|| Regex::new(r"^\s*(tt\d{7,9})\|").unwrap());

fn first_capture(re: &Regex, text: &str) -> Option<String> {
    re.captures(text)
        .and_then(|c| c.iter().skip(1).flatten().next().map(|m| m.as_str().to_string()))
}

/// Pistas para identificar un vídeo nuevo: nombre, `.txt` del K-Movies 4.0 y `.nfo` (Jellyfin/Kodi).
pub fn hints(video: &Path, root: &Path, walk: &Walk) -> Hints {
    let all = &walk.files;
    let parsed = parse::parse_path(video, root);
    let base = video.with_extension("");
    let base_l = base.to_string_lossy().to_lowercase();
    let dir_l = video.parent().map(|p| p.to_string_lossy().to_lowercase()).unwrap_or_default();

    // "Pelicula.srt", "Pelicula.es.srt", "Pelicula.spa.forced.srt"...
    let has_subs = walk
        .subs
        .range(base_l.clone()..)
        .take_while(|s| s.starts_with(&base_l))
        .any(|s| s.len() == base_l.len() || s[base_l.len()..].starts_with('.'));

    let mut imdb_id = parsed.imdb_id.clone();
    let mut tmdb_id = None;

    let txt = format!("{base_l}.txt");
    if imdb_id.is_none() && all.contains(&txt) {
        if let Ok(text) = std::fs::read_to_string(base.with_extension("txt")) {
            imdb_id = first_capture(&KM4_TXT, &text);
        }
    }

    let nfo_candidates = [(format!("{base_l}.nfo"), base.with_extension("nfo")), {
        let p = video.with_file_name("movie.nfo");
        (format!("{dir_l}\\movie.nfo"), p)
    }];
    for (key, path) in nfo_candidates {
        if (imdb_id.is_some() && tmdb_id.is_some()) || !all.contains(&key) {
            continue;
        }
        if let Ok(bytes) = std::fs::read(&path) {
            let text = String::from_utf8_lossy(&bytes);
            imdb_id = imdb_id.or_else(|| first_capture(&NFO_IMDB, &text));
            tmdb_id = tmdb_id.or_else(|| first_capture(&NFO_TMDB, &text).and_then(|s| s.parse().ok()));
        }
    }

    Hints { parsed, imdb_id, tmdb_id, has_subs }
}
