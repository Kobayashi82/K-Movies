//! Letterboxd: exportar el historial en el CSV que acepta su importador e importar la exportación
//! de tu cuenta (vistas, diario, notas y, si se pide, la watchlist a Pendientes).
//!
//! Letterboxd no tiene API abierta: todo va por archivos. Formato de importación comprobado en
//! <https://letterboxd.com/about/importing-data/> (columnas por nombre, fechas `AAAA-MM-DD`, notas
//! de 0,5 a 5, archivos de 1 MB como mucho).

use crate::parse::normalize;
use crate::tmdb::{self, Tmdb};
use crate::{AppState, db, library};
use anyhow::{Result, anyhow, bail};
use futures::StreamExt;
use rusqlite::{Connection, OptionalExtension, params};
use serde::Serialize;
use std::collections::HashMap;
use std::io::Read;
use std::path::{Path, PathBuf};
use std::sync::atomic::{AtomicUsize, Ordering};
use tauri::{AppHandle, Emitter, Manager};

/// El importador de Letterboxd no acepta archivos de más de 1 MB.
const MAX_BYTES: usize = 1_000_000;

// ---------------------------------------------------------------------------
// Exportar

#[derive(Serialize)]
#[serde(rename_all = "camelCase")]
pub struct ExportResult {
    /// Visionados exportados.
    pub count: usize,
    /// Archivos escritos (más de uno si no cabe en 1 MB).
    pub files: Vec<String>,
}

struct Row {
    id: i64,
    fields: [String; 7],
}

const HEADER: [&str; 7] = ["tmdbID", "imdbID", "Title", "Year", "WatchedDate", "Rating", "Rewatch"];

/// Visionados que se exportan, del más antiguo al más reciente. Con `only_new`, solo los que no se
/// han exportado nunca (ni vienen de Letterboxd).
fn rows(conn: &Connection, only_new: bool) -> Result<Vec<Row>> {
    let sql = format!(
        "SELECT h.id, i.tmdb_id, i.imdb_id, i.title, i.year, {} AS day, h.rating, h.rewatch
         FROM history h JOIN items i ON i.id = h.item_id
         WHERE ?1 = 0 OR h.exported_at IS NULL
         ORDER BY day, h.id",
        db::LOCAL_DAY.replace("watched_at", "h.watched_at")
    );
    let mut stmt = conn.prepare(&sql)?;
    let rows = stmt.query_map([only_new], |r| {
        let text = |v: Option<String>| v.unwrap_or_default();
        Ok(Row {
            id: r.get(0)?,
            fields: [
                text(r.get::<_, Option<i64>>(1)?.map(|v| v.to_string())),
                text(r.get(2)?),
                r.get(3)?,
                text(r.get::<_, Option<i64>>(4)?.map(|v| v.to_string())),
                r.get(5)?,
                text(r.get::<_, Option<f64>>(6)?.map(|v| v.to_string())),
                r.get::<_, bool>(7)?.to_string(),
            ],
        })
    })?;
    Ok(rows.collect::<rusqlite::Result<_>>()?)
}

fn csv_line<I: IntoIterator<Item = T>, T: AsRef<[u8]>>(fields: I) -> Result<Vec<u8>> {
    let mut w = csv::Writer::from_writer(Vec::new());
    w.write_record(fields)?;
    w.into_inner().map_err(|e| anyhow!(e.to_string()))
}

/// Parte las filas en archivos de 1 MB como mucho, cada uno con su cabecera.
fn chunks(rows: &[Row], max: usize) -> Result<Vec<Vec<u8>>> {
    let header = csv_line(HEADER)?;
    let mut out = vec![header.clone()];
    for row in rows {
        let line = csv_line(&row.fields)?;
        let current = out.last_mut().expect("siempre hay uno");
        if current.len() > header.len() && current.len() + line.len() > max {
            out.push(header.clone());
        }
        out.last_mut().expect("siempre hay uno").extend_from_slice(&line);
    }
    Ok(out)
}

/// `historial.csv`, `historial (2).csv`…
fn part_path(path: &Path, n: usize) -> PathBuf {
    if n == 0 {
        return path.to_path_buf();
    }
    let stem = path.file_stem().unwrap_or_default().to_string_lossy();
    let ext = path.extension().map(|e| format!(".{}", e.to_string_lossy())).unwrap_or_default();
    path.with_file_name(format!("{stem} ({}){ext}", n + 1))
}

/// Escribe el CSV de importación de Letterboxd y apunta esos visionados como exportados.
pub fn export(conn: &Connection, path: &Path, only_new: bool) -> Result<ExportResult> {
    let rows = rows(conn, only_new)?;
    if rows.is_empty() {
        return Ok(ExportResult { count: 0, files: vec![] });
    }
    let mut files = Vec::new();
    for (n, bytes) in chunks(&rows, MAX_BYTES)?.into_iter().enumerate() {
        let file = part_path(path, n);
        std::fs::write(&file, bytes)?;
        files.push(file.to_string_lossy().into_owned());
    }
    let tx = conn.unchecked_transaction()?;
    for row in &rows {
        tx.execute("UPDATE history SET exported_at = datetime('now') WHERE id = ?1", [row.id])?;
    }
    tx.commit()?;
    Ok(ExportResult { count: rows.len(), files })
}

// ---------------------------------------------------------------------------
// Importar

/// Una película de la exportación de Letterboxd, juntando lo que dicen sus distintos archivos.
#[derive(Default, Debug)]
struct Film {
    name: String,
    year: Option<i32>,
    /// Cuándo se marcó como vista (`watched.csv`; ahí no hay fecha de visionado).
    logged: Option<String>,
    /// Visionados del diario: día y nota.
    diary: Vec<(String, Option<f64>)>,
    /// Nota de la película (`ratings.csv`) y cuándo se puso.
    rating: Option<f64>,
    rated_on: Option<String>,
    /// En la watchlist desde ese día.
    watchlist: Option<String>,
}

impl Film {
    fn watched(&self) -> bool {
        !self.diary.is_empty() || self.logged.is_some() || self.rating.is_some()
    }

    fn label(&self) -> String {
        match self.year {
            Some(y) => format!("{} ({y})", self.name),
            None => self.name.clone(),
        }
    }
}

#[derive(Clone, Copy, PartialEq, Debug)]
enum Kind {
    Diary,
    Watched,
    Ratings,
    Watchlist,
}

const FILES: [(&str, Kind); 4] =
    [("diary.csv", Kind::Diary), ("watched.csv", Kind::Watched), ("ratings.csv", Kind::Ratings), ("watchlist.csv", Kind::Watchlist)];

fn is_day(s: &str) -> bool {
    s.len() == 10 && s.bytes().enumerate().all(|(i, b)| if i == 4 || i == 7 { b == b'-' } else { b.is_ascii_digit() })
}

fn rating(s: Option<&str>) -> Option<f64> {
    s.and_then(|v| v.parse::<f64>().ok()).filter(|v| (0.5..=5.0).contains(v))
}

/// Lee un CSV de la exportación y lo junta en `films` (por título normalizado y año).
fn read_csv(bytes: &[u8], kind: Kind, films: &mut HashMap<(String, Option<i32>), Film>) -> Result<()> {
    let bytes = bytes.strip_prefix(b"\xEF\xBB\xBF").unwrap_or(bytes);
    let mut reader = csv::ReaderBuilder::new().flexible(true).from_reader(bytes);
    let columns: HashMap<String, usize> =
        reader.headers()?.iter().enumerate().map(|(i, h)| (h.trim().to_lowercase(), i)).collect();
    for record in reader.records() {
        let record = record?;
        let get = |name: &str| columns.get(name).and_then(|&i| record.get(i)).map(str::trim).filter(|v| !v.is_empty());
        let Some(name) = get("name") else { continue };
        let year = get("year").and_then(|y| y.parse().ok());
        let date = get("date").filter(|d| is_day(d)).map(str::to_string);
        let film = films.entry((normalize(name), year)).or_insert_with(|| Film { name: name.to_string(), year, ..Default::default() });
        match kind {
            Kind::Diary => {
                let day = get("watched date").filter(|d| is_day(d)).map(str::to_string).or(date);
                if let Some(day) = day {
                    film.diary.push((day, rating(get("rating"))));
                }
            }
            Kind::Watched => film.logged = film.logged.take().or(date),
            Kind::Ratings => {
                film.rating = rating(get("rating")).or(film.rating);
                film.rated_on = date.or(film.rated_on.take());
            }
            Kind::Watchlist => film.watchlist = date.or_else(|| Some(String::new())),
        }
    }
    Ok(())
}

/// La exportación de Letterboxd: el ZIP entero o uno de sus CSV sueltos.
fn read_export(path: &Path) -> Result<Vec<Film>> {
    let mut films = HashMap::new();
    let is_zip = path.extension().is_some_and(|e| e.eq_ignore_ascii_case("zip"));
    if is_zip {
        let mut archive = zip::ZipArchive::new(std::fs::File::open(path)?)?;
        let mut found = false;
        for (name, kind) in FILES {
            // Solo los de la raíz: en `deleted/` y `orphaned/` está lo que borraste.
            let Ok(mut file) = archive.by_name(name) else { continue };
            let mut bytes = Vec::new();
            file.read_to_end(&mut bytes)?;
            read_csv(&bytes, kind, &mut films)?;
            found = true;
        }
        if !found {
            bail!("letterboxd_not_export");
        }
    } else {
        let bytes = std::fs::read(path)?;
        let name = path.file_name().unwrap_or_default().to_string_lossy().to_lowercase();
        let kind = FILES.iter().find(|(f, _)| name.contains(f.trim_end_matches(".csv"))).map(|(_, k)| *k);
        let kind = match kind {
            Some(k) => k,
            // Con otro nombre, por las columnas.
            None => {
                let head = String::from_utf8_lossy(&bytes[..bytes.len().min(300)]).to_lowercase();
                if head.contains("watched date") {
                    Kind::Diary
                } else if head.contains("rating") {
                    Kind::Ratings
                } else {
                    Kind::Watched
                }
            }
        };
        read_csv(&bytes, kind, &mut films)?;
    }
    Ok(films.into_values().collect())
}

#[derive(Serialize, Default, Debug)]
#[serde(rename_all = "camelCase")]
pub struct ImportSummary {
    /// Películas en la exportación (sin la watchlist si no se pidió).
    pub films: usize,
    /// Fichas nuevas creadas (películas que no estaban en K-Movies).
    pub added: usize,
    pub viewings: usize,
    pub ratings: usize,
    /// Añadidas a Pendientes desde la watchlist.
    pub pending: usize,
    /// "Título (año)" de las que no se encontraron.
    pub unmatched: Vec<String>,
}

struct Local {
    id: i64,
    titles: Vec<String>,
    year: Option<i32>,
}

fn local_index(conn: &Connection) -> Result<Vec<Local>> {
    let mut stmt = conn.prepare("SELECT id, title, original_title, year FROM items WHERE kind = 'movie'")?;
    let rows = stmt.query_map([], |r| {
        let mut titles = vec![normalize(&r.get::<_, String>(1)?)];
        if let Some(o) = r.get::<_, Option<String>>(2)? {
            titles.push(normalize(&o));
        }
        Ok(Local { id: r.get(0)?, titles, year: r.get(3)? })
    })?;
    Ok(rows.collect::<rusqlite::Result<_>>()?)
}

/// Busca la película entre las que ya hay: mismo título (o título original) y año (±1).
fn match_local(local: &[Local], name: &str, year: Option<i32>) -> Option<i64> {
    let wanted = normalize(name);
    let distance = |l: &Local| match (year, l.year) {
        (Some(a), Some(b)) => (a - b).abs(),
        _ => 1,
    };
    local
        .iter()
        .filter(|l| l.titles.contains(&wanted) && distance(l) <= 1)
        .min_by_key(|l| distance(l))
        .map(|l| l.id)
}

/// El item de K-Movies de una película de Letterboxd: el que ya había o uno nuevo desde TMDb.
async fn resolve(app: &AppHandle, client: Option<&(Tmdb, String)>, local: &[Local], film: &Film) -> Result<Option<(i64, bool)>> {
    if let Some(id) = match_local(local, &film.name, film.year) {
        return Ok(Some((id, false)));
    }
    let Some((client, language)) = client else { return Ok(None) };
    // Letterboxd da los títulos en inglés: se busca en inglés para compararlos bien.
    let candidates = client.search(&film.name, film.year, "en-US").await?;
    let Some(tmdb_id) = tmdb::best_match(&candidates, &film.name, film.year) else { return Ok(None) };
    library::ensure_item(app, client, language, tmdb_id).await.map(Some)
}

fn insert(conn: &Connection, item: i64, day: Option<&str>, rating: Option<f64>) -> Result<()> {
    // Ya están en Letterboxd: se apuntan como exportados para no volver a mandarlos.
    conn.execute(
        "INSERT INTO history (item_id, watched_at, rating, source, exported_at)
         VALUES (?1, COALESCE(?2, date('now', 'localtime')), ?3, 'letterboxd', datetime('now'))",
        params![item, day, rating],
    )?;
    Ok(())
}

/// Pasa lo de una película de Letterboxd a su item, sin duplicar lo que ya hubiera (se puede
/// importar la misma exportación varias veces).
fn apply(conn: &Connection, item: i64, film: &Film, with_watchlist: bool, s: &mut ImportSummary) -> Result<()> {
    let tx = conn.unchecked_transaction()?;
    let same_day = format!("SELECT id, rating FROM history WHERE item_id = ?1 AND {} = ?2", db::LOCAL_DAY);
    for (day, rating) in &film.diary {
        let existing: Option<(i64, Option<f64>)> =
            tx.query_row(&same_day, params![item, day], |r| Ok((r.get(0)?, r.get(1)?))).optional()?;
        match existing {
            // Ya estaba ese visionado: solo se le pone la nota si no tenía.
            Some((id, None)) if rating.is_some() => {
                db::set_rating(&tx, id, *rating)?;
                s.ratings += 1;
            }
            Some(_) => {}
            None => {
                insert(&tx, item, Some(day), *rating)?;
                s.viewings += 1;
                s.ratings += usize::from(rating.is_some());
            }
        }
    }
    let count: i64 = tx.query_row("SELECT COUNT(*) FROM history WHERE item_id = ?1", [item], |r| r.get(0))?;
    if film.watched() && count == 0 {
        // Vista sin fecha de visionado: cuenta el día en que se marcó en Letterboxd.
        insert(&tx, item, film.logged.as_deref().or(film.rated_on.as_deref()), None)?;
        s.viewings += 1;
    }
    if let Some(r) = film.rating {
        let rated: bool =
            tx.query_row("SELECT EXISTS (SELECT 1 FROM history WHERE item_id = ?1 AND rating IS NOT NULL)", [item], |r| r.get(0))?;
        if !rated {
            let latest: i64 = tx.query_row(
                "SELECT id FROM history WHERE item_id = ?1 ORDER BY watched_at DESC, id DESC LIMIT 1",
                [item],
                |r| r.get(0),
            )?;
            db::set_rating(&tx, latest, Some(r))?;
            s.ratings += 1;
        }
    }
    if with_watchlist && film.watchlist.is_some() && !film.watched() {
        // Pendientes es "conseguirla": solo si no está en disco ni la has visto.
        let changed = tx.execute(
            "UPDATE items SET watchlist = 1, watchlist_at = COALESCE(NULLIF(?2, ''), datetime('now'))
             WHERE id = ?1 AND watchlist = 0
               AND NOT EXISTS (SELECT 1 FROM files WHERE item_id = ?1)
               AND NOT EXISTS (SELECT 1 FROM history WHERE item_id = ?1)",
            params![item, film.watchlist],
        )?;
        s.pending += changed;
    }
    db::fix_rewatch(&tx, item)?;
    tx.commit()?;
    Ok(())
}

/// Importa la exportación de Letterboxd (`path`: el ZIP o uno de sus CSV). Las películas que no
/// están en K-Movies se buscan en TMDb y se añaden (a Vistas, o a Pendientes desde la watchlist).
/// El progreso va como el del escaneo (fase `letterboxd`); el `done` lo manda quien la llama.
pub async fn import(app: &AppHandle, path: &Path, with_watchlist: bool) -> Result<ImportSummary> {
    let films: Vec<Film> = read_export(path)?.into_iter().filter(|f| f.watched() || with_watchlist).collect();
    if films.is_empty() {
        bail!("letterboxd_empty");
    }
    let client = library::client(app).ok();
    let local = local_index(&app.state::<AppState>().db())?;
    let total = films.len();
    let done = AtomicUsize::new(0);
    library::emit(app, "letterboxd", 0, total, None);
    let resolved: Vec<(Film, Result<Option<(i64, bool)>>)> = futures::stream::iter(films)
        .map(|film| {
            let (client, local, done) = (client.as_ref(), &local, &done);
            async move {
                let r = resolve(app, client, local, &film).await;
                let n = done.fetch_add(1, Ordering::Relaxed) + 1;
                library::emit(app, "letterboxd", n, total, Some(film.name.clone()));
                (film, r)
            }
        })
        .buffer_unordered(4)
        .collect()
        .await;

    // Si no se ha podido buscar nada (sin conexión, clave mala), mejor decirlo que dar todo por no encontrado.
    if resolved.iter().all(|(_, r)| r.is_err()) {
        let (_, first) = resolved.into_iter().next().expect("hay películas");
        return Err(first.err().expect("todas fallaron"));
    }

    let mut summary = ImportSummary { films: total, ..Default::default() };
    let mut new_imdb = Vec::new();
    {
        let state = app.state::<AppState>();
        let conn = state.db();
        for (film, r) in &resolved {
            match r {
                Ok(Some((item, created))) => {
                    if *created {
                        summary.added += 1;
                        let imdb: Option<String> =
                            conn.query_row("SELECT imdb_id FROM items WHERE id = ?1", [item], |r| r.get(0))?;
                        new_imdb.extend(imdb);
                    }
                    apply(&conn, *item, film, with_watchlist, &mut summary)?;
                }
                Ok(None) => summary.unmatched.push(film.label()),
                Err(e) => {
                    eprintln!("[letterboxd] {}: {e:#}", film.label());
                    summary.unmatched.push(film.label());
                }
            }
        }
    }
    summary.unmatched.sort_by_key(|l| l.to_lowercase());
    if !new_imdb.is_empty() {
        let _ = library::update_imdb_ratings(app, Some(new_imdb)).await;
    }
    let _ = app.emit("library-changed", ());
    Ok(summary)
}

#[cfg(test)]
mod tests {
    use super::*;

    const DIARY: &str = "\u{feff}Date,Name,Year,Letterboxd URI,Rating,Rewatch,Tags,Watched Date\n\
        2024-03-02,Parasite,2019,https://boxd.it/a,4.5,,,2024-03-01\n\
        2025-01-10,Parasite,2019,https://boxd.it/b,,Yes,,2025-01-09\n\
        2024-05-05,\"Crouching Tiger, Hidden Dragon\",2000,https://boxd.it/c,4,,,2024-05-04\n";
    const WATCHED: &str = "Date,Name,Year,Letterboxd URI\n2023-01-01,Parasite,2019,https://boxd.it/x\n2023-02-02,Heat,1995,https://boxd.it/y\n";
    const RATINGS: &str = "Date,Name,Year,Letterboxd URI,Rating\n2023-02-03,Heat,1995,https://boxd.it/y,5\n";
    const WATCHLIST: &str = "Date,Name,Year,Letterboxd URI\n2022-06-06,Dune: Part Two,2024,https://boxd.it/z\n";

    fn films() -> HashMap<(String, Option<i32>), Film> {
        let mut films = HashMap::new();
        for (csv, kind) in [(DIARY, Kind::Diary), (WATCHED, Kind::Watched), (RATINGS, Kind::Ratings), (WATCHLIST, Kind::Watchlist)] {
            read_csv(csv.as_bytes(), kind, &mut films).unwrap();
        }
        films
    }

    #[test]
    fn reads_the_export_files_together() {
        let f = films();
        let parasite = &f[&("parasite".to_string(), Some(2019))];
        assert_eq!(parasite.diary, vec![("2024-03-01".to_string(), Some(4.5)), ("2025-01-09".to_string(), None)]);
        assert_eq!(parasite.logged.as_deref(), Some("2023-01-01"));
        let tiger = &f[&("crouching tiger hidden dragon".to_string(), Some(2000))];
        assert_eq!(tiger.name, "Crouching Tiger, Hidden Dragon");
        let heat = &f[&("heat".to_string(), Some(1995))];
        assert_eq!((heat.rating, heat.rated_on.as_deref()), (Some(5.0), Some("2023-02-03")));
        let dune = &f[&("dune part two".to_string(), Some(2024))];
        assert!(!dune.watched() && dune.watchlist.is_some());
    }

    fn conn() -> Connection {
        let conn = db::open(Path::new(":memory:")).unwrap();
        conn.execute_batch(
            "INSERT INTO items (id, title, original_title, year) VALUES (1, 'Parásitos', 'Gisaengchung', 2019);
             INSERT INTO items (id, title, year) VALUES (2, 'Heat', 1995);
             INSERT INTO items (id, title, year) VALUES (3, 'Dune: Parte dos', 2024);
             INSERT INTO history (item_id, watched_at, source) VALUES (1, '2024-03-01', 'manual');",
        )
        .unwrap();
        conn
    }

    fn history(conn: &Connection, item: i64) -> Vec<(String, Option<f64>, bool, String)> {
        let mut stmt = conn
            .prepare("SELECT watched_at, rating, rewatch, source FROM history WHERE item_id = ?1 ORDER BY watched_at")
            .unwrap();
        stmt.query_map([item], |r| Ok((r.get(0)?, r.get(1)?, r.get(2)?, r.get(3)?))).unwrap().map(Result::unwrap).collect()
    }

    #[test]
    fn import_adds_viewings_and_ratings_without_duplicates() {
        let conn = conn();
        let f = films();
        let mut s = ImportSummary::default();
        let parasite = &f[&("parasite".to_string(), Some(2019))];
        let heat = &f[&("heat".to_string(), Some(1995))];
        let dune = &f[&("dune part two".to_string(), Some(2024))];
        for _ in 0..2 {
            apply(&conn, 1, parasite, true, &mut s).unwrap();
            apply(&conn, 2, heat, true, &mut s).unwrap();
            apply(&conn, 3, dune, true, &mut s).unwrap();
        }
        // El visionado del 1 de marzo ya estaba: se le pone la nota. El de 2025 es nuevo y es revisionado.
        assert_eq!(
            history(&conn, 1),
            vec![
                ("2024-03-01".into(), Some(4.5), false, "manual".into()),
                ("2025-01-09".into(), None, true, "letterboxd".into()),
            ]
        );
        // Sin diario: el día en que se marcó como vista, con la nota de la película.
        assert_eq!(history(&conn, 2), vec![("2023-02-02".into(), Some(5.0), false, "letterboxd".into())]);
        // La watchlist va a Pendientes (una vez).
        assert!(history(&conn, 3).is_empty());
        assert_eq!(conn.query_row("SELECT watchlist FROM items WHERE id = 3", [], |r| r.get::<_, bool>(0)).unwrap(), true);
        assert_eq!((s.viewings, s.ratings, s.pending), (2, 2, 1));
    }

    #[test]
    fn local_match_uses_title_or_original_and_year() {
        let index = local_index(&conn()).unwrap();
        assert_eq!(match_local(&index, "Gisaengchung", Some(2019)), Some(1));
        assert_eq!(match_local(&index, "Heat", Some(1996)), Some(2));
        assert_eq!(match_local(&index, "Heat", Some(2010)), None);
    }

    #[test]
    fn export_writes_only_new_and_splits() {
        let conn = conn();
        conn.execute("UPDATE items SET tmdb_id = 496243, imdb_id = 'tt6751668' WHERE id = 1", []).unwrap();
        conn.execute("INSERT INTO history (item_id, watched_at, rating, source) VALUES (2, '2024-06-01', 3.5, 'manual')", []).unwrap();
        let dir = std::env::temp_dir().join(format!("kmovies-lb-{}", std::process::id()));
        std::fs::create_dir_all(&dir).unwrap();
        let path = dir.join("letterboxd.csv");
        let r = export(&conn, &path, true).unwrap();
        assert_eq!(r.count, 2);
        let text = std::fs::read_to_string(&path).unwrap();
        assert_eq!(
            text,
            "tmdbID,imdbID,Title,Year,WatchedDate,Rating,Rewatch\n\
             496243,tt6751668,Parásitos,2019,2024-03-01,,false\n\
             ,,Heat,1995,2024-06-01,3.5,false\n"
        );
        // Ya exportados: la segunda vez no hay nada nuevo.
        assert_eq!(export(&conn, &path, true).unwrap().count, 0);
        assert_eq!(export(&conn, &path, false).unwrap().count, 2);
        std::fs::remove_dir_all(&dir).unwrap();

        let rows = rows(&conn, false).unwrap();
        let parts = chunks(&rows, 90).unwrap();
        assert_eq!(parts.len(), 2);
        assert!(parts.iter().all(|p| p.starts_with(b"tmdbID,")));
        assert_eq!(part_path(Path::new("C:/x/lb.csv"), 1), Path::new("C:/x/lb (2).csv"));
    }
}
