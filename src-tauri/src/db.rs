//! SQLite: apertura, migraciones y consultas.

use anyhow::Result;
use rusqlite::{Connection, OptionalExtension, params};
use serde::Serialize;
use std::collections::HashMap;
use std::path::Path;

const MIGRATIONS: &[&str] = &[include_str!("../migrations/001_init.sql")];

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
                pb.position, pb.duration
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

pub fn largest_file(conn: &Connection, item_id: i64) -> Result<Option<String>> {
    Ok(conn
        .query_row("SELECT path FROM files WHERE item_id = ?1 ORDER BY size DESC LIMIT 1", [item_id], |r| r.get(0))
        .optional()?)
}
