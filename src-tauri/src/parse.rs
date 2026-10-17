//! Extrae título y año del nombre de un archivo o carpeta de película.

use regex::Regex;
use std::path::Path;
use std::sync::LazyLock;

#[derive(Debug, Clone, PartialEq)]
pub struct Parsed {
    pub title: String,
    pub year: Option<i32>,
    pub imdb_id: Option<String>,
}

static IMDB_ID: LazyLock<Regex> = LazyLock::new(|| Regex::new(r"\btt\d{7,9}\b").unwrap());
static BRACKET_YEAR: LazyLock<Regex> = LazyLock::new(|| Regex::new(r"[\[(]((?:19|20)\d{2})[\])]").unwrap());
static BRACKETS: LazyLock<Regex> = LazyLock::new(|| Regex::new(r"\[[^\]]*\]|\{[^}]*\}").unwrap());
static EMPTY_PARENS: LazyLock<Regex> = LazyLock::new(|| Regex::new(r"\(\s*\)").unwrap());
// Sufijo de copia que ponen Windows o K-Movies al repetir un nombre: "Patata (2)".
static COPY_SUFFIX: LazyLock<Regex> = LazyLock::new(|| Regex::new(r"\s*\(\d{1,2}\)\s*$").unwrap());
static YEAR: LazyLock<Regex> = LazyLock::new(|| Regex::new(r"\b((?:19|20)\d{2})\b").unwrap());
// Solo palabras inequívocamente técnicas: "Web", "Dual" o "Limited" pueden ser parte de un título.
static JUNK: LazyLock<Regex> = LazyLock::new(|| {
    Regex::new(concat!(
        r"(?i)\b(",
        r"2160p|1080p|1080i|720p|576p|480p|4k|uhd|hdr10?\+?|hdr|dolby\s?vision|",
        r"blu-?ray|bdrip|brrip|bdremux|remux|web-?dl|web-?rip|hdtv|hdrip|dvdrip|dvdscr|",
        r"x264|x265|h\s?264|h\s?265|hevc|xvid|divx|10bit|",
        r"aac|ac3|eac3|dd\+?5\s1|ddp5\s1|dts(?:-hd)?|truehd|atmos|",
        r"proper|repack|extended|unrated|remastered|directors\s?cut",
        r")\b"
    ))
    .unwrap()
});
static SPACES: LazyLock<Regex> = LazyLock::new(|| Regex::new(r"\s{2,}").unwrap());
static GENERIC: LazyLock<Regex> =
    LazyLock::new(|| Regex::new(r"(?i)^(movie|video|film|pelicula|cd\s?\d|disc\s?\d|part\s?\d|vts_\d+_\d+)$").unwrap());

/// Analiza un nombre sin extensión (p. ej. `Blade.Runner.2049.2017.1080p.BluRay`).
pub fn parse_name(name: &str) -> Parsed {
    let imdb_id = IMDB_ID.find(name).map(|m| m.as_str().to_string());
    let mut s = COPY_SUFFIX.replace(name, "").into_owned();
    s = IMDB_ID.replace_all(&s, " ").into_owned();
    s = BRACKET_YEAR.replace_all(&s, " $1 ").into_owned();
    s = BRACKETS.replace_all(&s, " ").into_owned();
    s = EMPTY_PARENS.replace_all(&s, " ").into_owned();
    s = s.replace(['_', '.'], " ");
    s = SPACES.replace_all(&s, " ").trim().to_string();

    // El año válido es el último que no esté al principio ("2001 A Space Odyssey 1968").
    let mut year = None;
    let mut cut = s.len();
    for m in YEAR.find_iter(&s) {
        if m.start() == 0 {
            continue;
        }
        year = m.as_str().parse().ok();
        cut = m.start();
    }
    let mut title = s[..cut].to_string();
    // Sin año, lo que sigue al título suelen ser datos técnicos ("Dune Part Two 1080p WEB-DL").
    // Con año también puede haberlos antes ("Alien Directors Cut 1979").
    if let Some(m) = JUNK.find(&title).filter(|m| m.start() > 0) {
        title.truncate(m.start());
    }
    let title = title
        .trim_end_matches(|c: char| c == '(' || c == '-' || c == '[' || c.is_whitespace())
        .trim()
        .to_string();

    Parsed { title, year, imdb_id }
}

/// Analiza la ruta de un vídeo. Si el nombre del archivo es genérico o no tiene año
/// y la carpeta sí, usa la carpeta (`Matrix (1999)/movie.mkv`).
pub fn parse_path(path: &Path, library_root: &Path) -> Parsed {
    let stem = path.file_stem().and_then(|s| s.to_str()).unwrap_or_default();
    let from_file = parse_name(stem);
    let parent = path.parent().filter(|p| *p != library_root && p.starts_with(library_root));
    let Some(dir_name) = parent.and_then(|p| p.file_name()).and_then(|s| s.to_str()) else {
        return from_file;
    };
    let from_dir = parse_name(dir_name);
    let generic = GENERIC.is_match(stem.trim()) || from_file.title.chars().count() < 2;
    let imdb_id = from_file.imdb_id.clone().or(from_dir.imdb_id.clone());
    if generic || (from_file.year.is_none() && from_dir.year.is_some()) {
        Parsed { imdb_id, ..from_dir }
    } else {
        Parsed { imdb_id, ..from_file }
    }
}

/// Normaliza un título para compararlo: minúsculas, sin acentos ni puntuación.
pub fn normalize(s: &str) -> String {
    let mut out = String::with_capacity(s.len());
    for c in s.chars().flat_map(char::to_lowercase) {
        let c = match c {
            'á' | 'à' | 'â' | 'ä' | 'ã' | 'å' => 'a',
            'é' | 'è' | 'ê' | 'ë' => 'e',
            'í' | 'ì' | 'î' | 'ï' => 'i',
            'ó' | 'ò' | 'ô' | 'ö' | 'õ' => 'o',
            'ú' | 'ù' | 'û' | 'ü' => 'u',
            'ñ' => 'n',
            'ç' => 'c',
            '\'' | '’' => continue,
            c => c,
        };
        if c.is_alphanumeric() {
            out.push(c);
        } else if !out.is_empty() && !out.ends_with(' ') {
            out.push(' ');
        }
    }
    out.trim_end().to_string()
}

#[cfg(test)]
mod tests {
    use super::*;

    fn p(name: &str) -> (String, Option<i32>) {
        let r = parse_name(name);
        (r.title, r.year)
    }

    #[test]
    fn typical_names() {
        assert_eq!(p("Inception (2010)"), ("Inception".into(), Some(2010)));
        assert_eq!(p("The.Dark.Knight.2008.1080p.BluRay.x264-GROUP"), ("The Dark Knight".into(), Some(2008)));
        assert_eq!(p("Blade.Runner.2049.2017.2160p.UHD"), ("Blade Runner 2049".into(), Some(2017)));
        assert_eq!(p("2001 A Space Odyssey (1968)"), ("2001 A Space Odyssey".into(), Some(1968)));
        assert_eq!(p("1917"), ("1917".into(), None));
        assert_eq!(p("Buddy"), ("Buddy".into(), None));
        assert_eq!(p("Amélie [2001] [BDRip]"), ("Amélie".into(), Some(2001)));
        assert_eq!(p("Toy_Story_1995"), ("Toy Story".into(), Some(1995)));
        assert_eq!(p("Alien - 1979"), ("Alien".into(), Some(1979)));
        assert_eq!(p("Dune.Part.Two.1080p.WEB-DL"), ("Dune Part Two".into(), None));
        assert_eq!(p("Sacrifice (tt1234567)"), ("Sacrifice".into(), None));
        assert_eq!(p("Charlotte's Web"), ("Charlotte's Web".into(), None));
        assert_eq!(p("Back.to.the.Future.1985.REMASTERED.1080p"), ("Back to the Future".into(), Some(1985)));
        assert_eq!(p("Ocean's Eleven 2001 DD5.1"), ("Ocean's Eleven".into(), Some(2001)));
        assert_eq!(p("Patata (2)"), ("Patata".into(), None));
        assert_eq!(p("Inception (2010) (3)"), ("Inception".into(), Some(2010)));
        assert_eq!(p("Inception (2010) (3)"), ("Inception".into(), Some(2010)));
    }

    #[test]
    fn imdb_in_name() {
        assert_eq!(parse_name("Sacrifice (tt1234567)").imdb_id.as_deref(), Some("tt1234567"));
    }

    #[test]
    fn uses_folder_when_better() {
        let root = Path::new("V:/Videos/Peliculas");
        let r = parse_path(Path::new("V:/Videos/Peliculas/The Matrix (1999)/movie.mkv"), root);
        assert_eq!((r.title.as_str(), r.year), ("The Matrix", Some(1999)));
        let r = parse_path(Path::new("V:/Videos/Peliculas/Heat (1995)/Heat.mkv"), root);
        assert_eq!((r.title.as_str(), r.year), ("Heat", Some(1995)));
        let r = parse_path(Path::new("V:/Videos/Peliculas/Buddy.mp4"), root);
        assert_eq!((r.title.as_str(), r.year), ("Buddy", None));
    }

    #[test]
    fn normalizes() {
        assert_eq!(normalize("Amélie: El Fabuloso Destino"), "amelie el fabuloso destino");
        assert_eq!(normalize("Fast & Furious"), "fast furious");
        assert_eq!(normalize("Ocean's Eleven"), "oceans eleven");
    }
}
