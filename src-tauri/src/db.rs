//! SQLite: apertura, migraciones y consultas.

use anyhow::Result;
use rusqlite::{Connection, OptionalExtension, params};
use serde::Serialize;
use std::collections::HashMap;
use std::path::Path;

const MIGRATIONS: &[&str] =
    &[include_str!("../migrations/001_init.sql"), include_str!("../migrations/002_file_prefs.sql")];

pub fn open(path: &Path) -> Result<Connection> {
    let conn = Connection::open(path)?;
    conn.execute_batch("PRAGMA journal_mode = WAL; PRAGMA foreign_keys = ON; PRAGMA synchronous = NORMAL;")?;
    migrate(&conn)?;
    Ok(conn)
}

fn migrate(conn: &Connection) -> Result<()> {
    let version: i64 = conn.query_row("PRAGMA user_version", [], |r| r.get(0))?;
    for (i, sql) in MIGRATIONS.iter().enumerate().skip(version as usize) {
        let tx = conn.unchecked_transaction()?;
        tx.execute_batch(sql)?;
        tx.pragma_update(None, "user_version", (i + 1) as i64)?;
        tx.commit()?;
    }
    Ok(())
}

// ---------------------------------------------------------------------------
// Ajustes

pub fn settings(conn: &Connection) -> Result<HashMap<String, String>> {
    let mut stmt = conn.prepare("SELECT key, value FROM settings")?;
    let rows = stmt.query_map([], |r| Ok((r.get(0)?, r.get(1)?)))?;
    Ok(rows.collect::<rusqlite::Result<_>>()?)
}

pub fn setting(conn: &Connection, key: &str) -> Result<Option<String>> {
    Ok(conn
        .query_row("SELECT value FROM settings WHERE key = ?1", [key], |r| r.get(0))
        .optional()?)
}

pub fn set_setting(conn: &Connection, key: &str, value: &str) -> Result<()> {
    conn.execute(
        "INSERT INTO settings (key, value) VALUES (?1, ?2) ON CONFLICT(key) DO UPDATE SET value = excluded.value",
        params![key, value],
    )?;
    Ok(())
}

// ---------------------------------------------------------------------------
// Bibliotecas

#[derive(Serialize, Clone)]
#[serde(rename_all = "camelCase")]
pub struct Library {
    pub id: i64,
    pub path: String,
    pub kind: String,
    pub last_scan: Option<String>,
    pub file_count: i64,
}

pub fn libraries(conn: &Connection) -> Result<Vec<Library>> {
    let mut stmt = conn.prepare(
        "SELECT l.id, l.path, l.kind, l.last_scan, (SELECT COUNT(*) FROM files f WHERE f.library_id = l.id)
         FROM libraries l ORDER BY l.path",
    )?;
    let rows = stmt.query_map([], |r| {
        Ok(Library { id: r.get(0)?, path: r.get(1)?, kind: r.get(2)?, last_scan: r.get(3)?, file_count: r.get(4)? })
    })?;
    Ok(rows.collect::<rusqlite::Result<_>>()?)
}

/// Borra los items que ya no pintan nada: sin archivos, sin historial y fuera de Pendientes.
pub fn prune_orphans(conn: &Connection) -> Result<usize> {
    Ok(conn.execute(
        "DELETE FROM items WHERE watchlist = 0
           AND NOT EXISTS (SELECT 1 FROM files f WHERE f.item_id = items.id)
           AND NOT EXISTS (SELECT 1 FROM history h WHERE h.item_id = items.id)",
        [],
    )?)
}

// ---------------------------------------------------------------------------
// Items

/// Lo justo para la rejilla, la ordenación y los filtros (se filtra en el frontend).
#[derive(Serialize)]
#[serde(rename_all = "camelCase")]
pub struct ItemSummary {
    pub id: i64,
    pub kind: String,
    pub tmdb_id: Option<i64>,
    pub imdb_id: Option<String>,
    pub title: String,
    pub original_title: Option<String>,
    pub year: Option<i32>,
    pub release_date: Option<String>,
    pub runtime: Option<i64>,
    pub certification: Option<String>,
    pub genres: serde_json::Value,
    pub countries: serde_json::Value,
    pub languages: serde_json::Value,
    pub imdb_rating: Option<f64>,
    pub imdb_votes: Option<i64>,
    pub tmdb_rating: Option<f64>,
    pub poster: Option<String>,
    pub backdrop: Option<String>,
    pub matched: i64,
    pub watchlist: bool,
    pub added_at: String,
    pub file_count: i64,
    pub total_size: i64,
    pub has_subs: bool,
    pub directors: serde_json::Value,
    pub cast: serde_json::Value,
    pub watch_count: i64,
    pub last_watched: Option<String>,
    pub position: Option<f64>,
    pub duration: Option<f64>,
    /// Cuándo se guardó la posición (para ordenar "Seguir viendo").
    pub playback_at: Option<String>,
}

fn json_col(r: &rusqlite::Row, idx: usize) -> rusqlite::Result<serde_json::Value> {
    let s: Option<String> = r.get(idx)?;
    Ok(s.and_then(|s| serde_json::from_str(&s).ok()).unwrap_or(serde_json::Value::Array(vec![])))
}

pub fn list_items(conn: &Connection, cache_dir: &Path) -> Result<Vec<ItemSummary>> {
    let mut stmt = conn.prepare(
        "SELECT i.id, i.kind, i.tmdb_id, i.imdb_id, i.title, i.original_title, i.year, i.release_date, i.runtime,
                i.certification, i.genres, i.countries, i.languages, i.imdb_rating, i.imdb_votes, i.tmdb_rating,
                i.poster, i.backdrop, i.matched, i.watchlist,
                COALESCE((SELECT MIN(f.added_at) FROM files f WHERE f.item_id = i.id), i.watchlist_at, i.added_at),
                (SELECT COUNT(*) FROM files f WHERE f.item_id = i.id),
                (SELECT COALESCE(SUM(f.size), 0) FROM files f WHERE f.item_id = i.id),
                (SELECT COALESCE(MAX(f.has_subs), 0) FROM files f WHERE f.item_id = i.id),
                (SELECT json_group_array(name) FROM (
                    SELECT p.name FROM item_people ip JOIN people p ON p.id = ip.person_id
                    WHERE ip.item_id = i.id AND ip.role = 'director' ORDER BY ip.ord)),
                (SELECT json_group_array(name) FROM (
                    SELECT p.name FROM item_people ip JOIN people p ON p.id = ip.person_id
                    WHERE ip.item_id = i.id AND ip.role = 'cast' ORDER BY ip.ord LIMIT 15)),
                (SELECT COUNT(*) FROM history h WHERE h.item_id = i.id),
                (SELECT MAX(h.watched_at) FROM history h WHERE h.item_id = i.id),
                pb.position, pb.duration, pb.updated_at
         FROM items i LEFT JOIN playback pb ON pb.item_id = i.id",
    )?;
    let cache = |name: Option<String>| name.map(|n| cache_dir.join(n).to_string_lossy().into_owned());
    let rows = stmt.query_map([], |r| {
        Ok(ItemSummary {
            id: r.get(0)?,
            kind: r.get(1)?,
            tmdb_id: r.get(2)?,
            imdb_id: r.get(3)?,
            title: r.get(4)?,
            original_title: r.get(5)?,
            year: r.get(6)?,
            release_date: r.get(7)?,
            runtime: r.get(8)?,
            certification: r.get(9)?,
            genres: json_col(r, 10)?,
            countries: json_col(r, 11)?,
            languages: json_col(r, 12)?,
            imdb_rating: r.get(13)?,
            imdb_votes: r.get(14)?,
            tmdb_rating: r.get(15)?,
            poster: cache(r.get(16)?),
            backdrop: cache(r.get(17)?),
            matched: r.get(18)?,
            watchlist: r.get(19)?,
            added_at: r.get(20)?,
            file_count: r.get(21)?,
            total_size: r.get(22)?,
            has_subs: r.get(23)?,
            directors: json_col(r, 24)?,
            cast: json_col(r, 25)?,
            watch_count: r.get(26)?,
            last_watched: r.get(27)?,
            position: r.get(28)?,
            duration: r.get(29)?,
            playback_at: r.get(30)?,
        })
    })?;
    Ok(rows.collect::<rusqlite::Result<_>>()?)
}

#[derive(Serialize)]
#[serde(rename_all = "camelCase")]
pub struct FileInfo {
    pub id: i64,
    pub path: String,
    pub size: i64,
    pub has_subs: bool,
    pub added_at: String,
}

#[derive(Serialize)]
#[serde(rename_all = "camelCase")]
pub struct Person {
    pub id: i64,
    pub tmdb_id: Option<i64>,
    pub name: String,
    pub role: String,
    pub character: Option<String>,
    pub profile_url: Option<String>,
}

#[derive(Serialize)]
#[serde(rename_all = "camelCase")]
pub struct ItemDetail {
    pub overview: Option<String>,
    pub tagline: Option<String>,
    pub original_language: Option<String>,
    pub tmdb_votes: Option<i64>,
    pub trailer_key: Option<String>,
    pub files: Vec<FileInfo>,
    pub people: Vec<Person>,
}

pub fn item_detail(conn: &Connection, id: i64) -> Result<ItemDetail> {
    let (overview, tagline, original_language, tmdb_votes, trailer_key) = conn.query_row(
        "SELECT overview, tagline, original_language, tmdb_votes, trailer_key FROM items WHERE id = ?1",
        [id],
        |r| Ok((r.get(0)?, r.get(1)?, r.get(2)?, r.get(3)?, r.get(4)?)),
    )?;
    let mut stmt =
        conn.prepare("SELECT id, path, size, has_subs, added_at FROM files WHERE item_id = ?1 ORDER BY size DESC")?;
    let files = stmt
        .query_map([id], |r| {
            Ok(FileInfo { id: r.get(0)?, path: r.get(1)?, size: r.get(2)?, has_subs: r.get(3)?, added_at: r.get(4)? })
        })?
        .collect::<rusqlite::Result<_>>()?;
    let mut stmt = conn.prepare(
        "SELECT p.id, p.tmdb_id, p.name, ip.role, ip.character, p.profile_url
         FROM item_people ip JOIN people p ON p.id = ip.person_id
         WHERE ip.item_id = ?1 ORDER BY ip.role DESC, ip.ord",
    )?;
    let people = stmt
        .query_map([id], |r| {
            Ok(Person {
                id: r.get(0)?,
                tmdb_id: r.get(1)?,
                name: r.get(2)?,
                role: r.get(3)?,
                character: r.get(4)?,
                profile_url: r.get(5)?,
            })
        })?
        .collect::<rusqlite::Result<_>>()?;
    Ok(ItemDetail { overview, tagline, original_language, tmdb_votes, trailer_key, files, people })
}

pub fn largest_file(conn: &Connection, item_id: i64) -> Result<Option<(i64, String)>> {
    Ok(conn
        .query_row("SELECT id, path FROM files WHERE item_id = ?1 ORDER BY size DESC LIMIT 1", [item_id], |r| {
            Ok((r.get(0)?, r.get(1)?))
        })
        .optional()?)
}

// ---------------------------------------------------------------------------
// Historial y progreso

#[derive(Serialize)]
#[serde(rename_all = "camelCase")]
pub struct HistoryEntry {
    pub id: i64,
    pub item_id: i64,
    pub watched_at: String,
    pub rating: Option<f64>,
    pub rewatch: bool,
    pub source: String,
}

/// Visionados, del más reciente al más antiguo. Con `item_id`, solo los de esa película.
pub fn history(conn: &Connection, item_id: Option<i64>) -> Result<Vec<HistoryEntry>> {
    let mut stmt = conn.prepare(
        "SELECT id, item_id, watched_at, rating, rewatch, source FROM history
         WHERE ?1 IS NULL OR item_id = ?1 ORDER BY watched_at DESC, id DESC",
    )?;
    let rows = stmt.query_map([item_id], |r| {
        Ok(HistoryEntry {
            id: r.get(0)?,
            item_id: r.get(1)?,
            watched_at: r.get(2)?,
            rating: r.get(3)?,
            rewatch: r.get(4)?,
            source: r.get(5)?,
        })
    })?;
    Ok(rows.collect::<rusqlite::Result<_>>()?)
}

/// Añade un visionado. Sin `date` (`AAAA-MM-DD`) es ahora mismo, y entonces la película deja de
/// estar "a medias": se borra la posición guardada.
pub fn add_watch(conn: &Connection, item_id: i64, date: Option<&str>, source: &str) -> Result<()> {
    conn.execute(
        "INSERT INTO history (item_id, watched_at, source) VALUES (?1, COALESCE(?2, datetime('now')), ?3)",
        params![item_id, date, source],
    )?;
    if date.is_none() {
        conn.execute("DELETE FROM playback WHERE item_id = ?1", [item_id])?;
    }
    fix_rewatch(conn, item_id)
}

/// Todos los visionados de una película son "revisionado" menos el primero (por fecha).
pub fn fix_rewatch(conn: &Connection, item_id: i64) -> Result<()> {
    conn.execute(
        "UPDATE history SET rewatch = EXISTS (
             SELECT 1 FROM history h WHERE h.item_id = history.item_id
               AND (h.watched_at < history.watched_at OR (h.watched_at = history.watched_at AND h.id < history.id)))
         WHERE item_id = ?1",
        [item_id],
    )?;
    Ok(())
}

/// Pistas y retardos elegidos para un archivo en el reproductor integrado.
#[derive(Debug, Clone, Default, PartialEq)]
pub struct FilePrefs {
    pub aid: Option<String>,
    pub sid: Option<String>,
    pub audio_delay: f64,
    pub sub_delay: f64,
}

pub fn file_prefs(conn: &Connection, file_id: i64) -> Result<Option<FilePrefs>> {
    Ok(conn
        .query_row("SELECT aid, sid, audio_delay, sub_delay FROM file_prefs WHERE file_id = ?1", [file_id], |r| {
            Ok(FilePrefs { aid: r.get(0)?, sid: r.get(1)?, audio_delay: r.get(2)?, sub_delay: r.get(3)? })
        })
        .optional()?)
}

pub fn save_file_prefs(conn: &Connection, file_id: i64, p: &FilePrefs) -> Result<()> {
    conn.execute(
        "INSERT INTO file_prefs (file_id, aid, sid, audio_delay, sub_delay) VALUES (?1, ?2, ?3, ?4, ?5)
         ON CONFLICT(file_id) DO UPDATE SET aid = excluded.aid, sid = excluded.sid,
             audio_delay = excluded.audio_delay, sub_delay = excluded.sub_delay",
        params![file_id, p.aid, p.sid, p.audio_delay, p.sub_delay],
    )?;
    Ok(())
}

pub fn save_position(conn: &Connection, item_id: i64, file_id: i64, position: f64, duration: f64) -> Result<()> {
    conn.execute(
        "INSERT INTO playback (item_id, file_id, position, duration, updated_at)
         VALUES (?1, ?2, ?3, ?4, datetime('now'))
         ON CONFLICT(item_id) DO UPDATE SET file_id = excluded.file_id, position = excluded.position,
             duration = excluded.duration, updated_at = excluded.updated_at",
        params![item_id, file_id, position, duration],
    )?;
    Ok(())
}

#[cfg(test)]
mod tests {
    use super::*;

    fn conn_with_item() -> Connection {
        let conn = Connection::open_in_memory().unwrap();
        migrate(&conn).unwrap();
        conn.execute("INSERT INTO items (id, title) VALUES (1, 'Buddy')", []).unwrap();
        conn.execute("INSERT INTO files (id, item_id, path, added_at) VALUES (7, 1, 'C:/Buddy.mp4', '2026-01-01')", [])
            .unwrap();
        conn
    }

    fn rewatch_flags(conn: &Connection) -> Vec<(String, bool)> {
        history(conn, Some(1)).unwrap().into_iter().map(|e| (e.watched_at, e.rewatch)).collect()
    }

    #[test]
    fn watching_now_clears_the_saved_position() {
        let conn = conn_with_item();
        save_position(&conn, 1, 7, 1800.0, 6000.0).unwrap();
        save_position(&conn, 1, 7, 2400.0, 6000.0).unwrap();
        let pos: f64 = conn.query_row("SELECT position FROM playback WHERE item_id = 1", [], |r| r.get(0)).unwrap();
        assert_eq!(pos, 2400.0);

        add_watch(&conn, 1, None, "player").unwrap();
        let left: i64 = conn.query_row("SELECT COUNT(*) FROM playback", [], |r| r.get(0)).unwrap();
        assert_eq!(left, 0);
        assert_eq!(history(&conn, Some(1)).unwrap()[0].source, "player");
    }

    #[test]
    fn file_prefs_round_trip_and_go_with_the_file() {
        let conn = conn_with_item();
        conn.execute_batch("PRAGMA foreign_keys = ON").unwrap();
        assert_eq!(file_prefs(&conn, 7).unwrap(), None);
        let mut p = FilePrefs { aid: Some("2".into()), sid: Some("no".into()), audio_delay: 0.15, sub_delay: -0.3 };
        save_file_prefs(&conn, 7, &p).unwrap();
        assert_eq!(file_prefs(&conn, 7).unwrap(), Some(p.clone()));
        p.sid = Some("3".into());
        save_file_prefs(&conn, 7, &p).unwrap();
        assert_eq!(file_prefs(&conn, 7).unwrap().unwrap().sid.as_deref(), Some("3"));
        conn.execute("DELETE FROM files WHERE id = 7", []).unwrap();
        assert_eq!(file_prefs(&conn, 7).unwrap(), None);
    }

    #[test]
    fn a_past_date_keeps_the_position() {
        let conn = conn_with_item();
        save_position(&conn, 1, 7, 1800.0, 6000.0).unwrap();
        add_watch(&conn, 1, Some("2019-05-01"), "manual").unwrap();
        let left: i64 = conn.query_row("SELECT COUNT(*) FROM playback", [], |r| r.get(0)).unwrap();
        assert_eq!(left, 1);
    }

    #[test]
    fn only_the_earliest_viewing_is_not_a_rewatch() {
        let conn = conn_with_item();
        add_watch(&conn, 1, Some("2024-03-10"), "manual").unwrap();
        add_watch(&conn, 1, Some("2021-07-02"), "manual").unwrap();
        // La de 2021 se añadió después pero es la primera: la de 2024 pasa a ser revisionado.
        assert_eq!(
            rewatch_flags(&conn),
            vec![("2024-03-10".into(), true), ("2021-07-02".into(), false)]
        );

        let first: i64 =
            conn.query_row("SELECT id FROM history WHERE watched_at = '2021-07-02'", [], |r| r.get(0)).unwrap();
        conn.execute("DELETE FROM history WHERE id = ?1", [first]).unwrap();
        fix_rewatch(&conn, 1).unwrap();
        assert_eq!(rewatch_flags(&conn), vec![("2024-03-10".into(), false)]);
    }
}
