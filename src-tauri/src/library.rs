//! Escaneo de la biblioteca: recorrer carpetas, identificar con TMDb, cachear imágenes y notas.
//! También las acciones sueltas sobre fichas: identificar a mano, añadir y actualizar.

use crate::tmdb::{self, Details, Tmdb};
use crate::{AppState, db, imdb, scanner};
use anyhow::{Result, anyhow};
use futures::StreamExt;
use rusqlite::{Connection, OptionalExtension, params};
use serde::Serialize;
use std::collections::HashMap;
use std::path::{Path, PathBuf};
use std::sync::atomic::{AtomicUsize, Ordering};
use std::sync::{Arc, Mutex};
use std::time::{Duration, Instant};
use tauri::{AppHandle, Emitter, Manager};

#[derive(Serialize, Clone)]
#[serde(rename_all = "camelCase")]
pub struct Progress {
    /// `scanning` | `matching` | `refreshing` | `ratings` | `done` | `error`
    pub phase: &'static str,
    pub current: usize,
    pub total: usize,
    pub message: Option<String>,
}

pub fn emit(app: &AppHandle, phase: &'static str, current: usize, total: usize, message: Option<String>) {
    let _ = app.emit("scan-progress", Progress { phase, current, total, message });
}

/// Aviso para el usuario (se muestra como notificación en la interfaz).
#[derive(Serialize, Clone)]
#[serde(rename_all = "camelCase")]
pub struct Notice {
    /// `pendingAvailable` | `alreadyWatched` | `transferDone` | `transferError` | `markedWatched` | `askFinished`
    pub kind: &'static str,
    pub title: String,
    pub item_id: Option<i64>,
    pub detail: Option<String>,
}

pub fn notice(app: &AppHandle, kind: &'static str, title: String, item_id: Option<i64>, detail: Option<String>) {
    let _ = app.emit("notice", Notice { kind, title, item_id, detail });
}

/// Avisa a la interfaz de que hay datos nuevos, como mucho una vez por segundo.
struct ChangeNotifier {
    app: AppHandle,
    last: Mutex<Instant>,
}

impl ChangeNotifier {
    fn notify(&self, force: bool) {
        let mut last = self.last.lock().unwrap();
        if force || last.elapsed() > Duration::from_millis(1000) {
            *last = Instant::now();
            let _ = self.app.emit("library-changed", ());
        }
    }
}

fn metadata_language(settings: &HashMap<String, String>) -> String {
    settings.get("metadataLanguage").cloned().unwrap_or_else(|| "es-ES".into())
}

/// Cliente de TMDb con la clave guardada, y el idioma de los datos.
pub fn client(app: &AppHandle) -> Result<(Tmdb, String)> {
    let state = app.state::<AppState>();
    let settings = db::settings(&state.db())?;
    let key = settings.get("tmdbKey").filter(|k| !k.is_empty()).ok_or_else(|| anyhow!("tmdb_no_key"))?;
    Ok((Tmdb::new(state.http.clone(), key), metadata_language(&settings)))
}

pub async fn scan(app: AppHandle) -> Result<()> {
    let state = app.state::<AppState>();
    let (libraries, settings) = {
        let conn = state.db();
        (db::libraries(&conn)?, db::settings(&conn)?)
    };
    let key = settings.get("tmdbKey").cloned().unwrap_or_default();
    let language = metadata_language(&settings);
    let notifier = Arc::new(ChangeNotifier { app: app.clone(), last: Mutex::new(Instant::now()) });

    // 1. Recorrer carpetas y sincronizar la tabla de archivos.
    for (i, lib) in libraries.iter().enumerate() {
        emit(&app, "scanning", i, libraries.len(), Some(lib.path.clone()));
        let root = PathBuf::from(&lib.path);
        // Disco de red desconectado: se conserva lo que hay en vez de darlo todo por borrado.
        if !root.is_dir() {
            continue;
        }
        let walk = {
            let root = root.clone();
            tauri::async_runtime::spawn_blocking(move || scanner::walk(&root)).await?
        };
        sync_files(&state.db(), lib, &root, &walk)?;
    }
    notifier.notify(true);

    // 2. Identificar archivos nuevos o sin coincidencia.
    let pending = files_to_match(&state.db(), !key.is_empty())?;
    if !pending.is_empty() {
        let total = pending.len();
        let done = Arc::new(AtomicUsize::new(0));
        let client = (!key.is_empty()).then(|| Tmdb::new(state.http.clone(), &key));
        let unauthorized = Arc::new(std::sync::atomic::AtomicBool::new(false));
        emit(&app, "matching", 0, total, None);
        futures::stream::iter(pending)
            .map(|file| {
                let app = app.clone();
                let client = client.clone();
                let language = language.clone();
                let done = done.clone();
                let notifier = notifier.clone();
                let unauthorized = unauthorized.clone();
                async move {
                    let title = file.title.clone();
                    let client = client.filter(|_| !unauthorized.load(Ordering::Relaxed));
                    if let Err(e) = match_file(&app, client.as_ref(), &language, file).await {
                        if e.to_string() == "tmdb_unauthorized" {
                            unauthorized.store(true, Ordering::Relaxed);
                        }
                        eprintln!("[scan] {title}: {e:#}");
                    }
                    let n = done.fetch_add(1, Ordering::Relaxed) + 1;
                    emit(&app, "matching", n, total, Some(title));
                    notifier.notify(false);
                }
            })
            .buffer_unordered(6)
            .collect::<Vec<()>>()
            .await;
        if unauthorized.load(Ordering::Relaxed) {
            emit(&app, "error", 0, 0, Some("tmdb_unauthorized".into()));
        }
    }

    // 3. Imágenes que falten (p. ej. si se cortó la conexión en un escaneo anterior).
    download_missing_images(&app).await;

    // 4. Notas de IMDb.
    emit(&app, "ratings", 0, 0, None);
    if let Err(e) = update_imdb_ratings(&app, None).await {
        eprintln!("[scan] notas de IMDb: {e:#}");
    }

    db::prune_orphans(&state.db())?;
    notifier.notify(true);
    emit(&app, "done", 0, 0, None);
    Ok(())
}

/// Vuelve a descargar la ficha de todas las películas identificadas (p. ej. tras cambiar el
/// idioma de los datos). Respeta las identificaciones hechas a mano.
pub async fn refresh_all(app: AppHandle) -> Result<()> {
    let (client, language) = client(&app)?;
    let items: Vec<(i64, i64, String)> = {
        let state = app.state::<AppState>();
        let conn = state.db();
        let mut stmt = conn.prepare("SELECT id, tmdb_id, title FROM items WHERE tmdb_id IS NOT NULL")?;
        stmt.query_map([], |r| Ok((r.get(0)?, r.get(1)?, r.get(2)?)))?.collect::<rusqlite::Result<_>>()?
    };
    let total = items.len();
    let done = Arc::new(AtomicUsize::new(0));
    let notifier = Arc::new(ChangeNotifier { app: app.clone(), last: Mutex::new(Instant::now()) });
    emit(&app, "refreshing", 0, total, None);
    futures::stream::iter(items)
        .map(|(id, tmdb_id, title)| {
            let (app, client, language, done, notifier) =
                (app.clone(), client.clone(), language.clone(), done.clone(), notifier.clone());
            async move {
                if let Err(e) = store_details(&app, &client, tmdb_id, &language, None, Some(id)).await {
                    eprintln!("[refresh] {title}: {e:#}");
                }
                let n = done.fetch_add(1, Ordering::Relaxed) + 1;
                emit(&app, "refreshing", n, total, Some(title));
                notifier.notify(false);
            }
        })
        .buffer_unordered(6)
        .collect::<Vec<()>>()
        .await;
    emit(&app, "ratings", 0, 0, None);
    if let Err(e) = update_imdb_ratings(&app, None).await {
        eprintln!("[refresh] notas de IMDb: {e:#}");
    }
    notifier.notify(true);
    emit(&app, "done", 0, 0, None);
    Ok(())
}

fn sync_files(conn: &Connection, lib: &db::Library, root: &Path, walk: &scanner::Walk) -> Result<()> {
    let mut known: HashMap<String, (i64, i64, i64)> = HashMap::new();
    {
        let mut stmt = conn.prepare("SELECT id, path, size, mtime FROM files WHERE library_id = ?1")?;
        let rows = stmt.query_map([lib.id], |r| Ok((r.get::<_, String>(1)?, (r.get(0)?, r.get(2)?, r.get(3)?))))?;
        for row in rows {
            let (path, v) = row?;
            known.insert(path.to_lowercase(), v);
        }
    }
    // En el primer escaneo, la fecha de añadido es la de modificación del archivo, como en el
    // K-Movies 4.0 (así se conserva el orden de "añadidas recientemente"). Después, la actual.
    let first_scan = lib.last_scan.is_none();
    let tx = conn.unchecked_transaction()?;
    for video in &walk.videos {
        let path = video.path.to_string_lossy().into_owned();
        match known.remove(&path.to_lowercase()) {
            Some((id, size, mtime)) => {
                if size != video.size as i64 || mtime != video.mtime {
                    tx.execute("UPDATE files SET size = ?2, mtime = ?3 WHERE id = ?1", params![id, video.size as i64, video.mtime])?;
                }
            }
            None => {
                let h = scanner::hints(&video.path, root, walk);
                tx.execute(
                    "INSERT INTO files (library_id, path, size, mtime, parsed_title, parsed_year, hint_imdb, hint_tmdb, has_subs, added_at)
                     VALUES (?1, ?2, ?3, ?4, ?5, ?6, ?7, ?8, ?9,
                             CASE WHEN ?10 THEN datetime(?4, 'unixepoch') ELSE datetime('now') END)",
                    params![
                        lib.id, path, video.size as i64, video.mtime, h.parsed.title, h.parsed.year,
                        h.imdb_id, h.tmdb_id, h.has_subs, first_scan && video.mtime > 0
                    ],
                )?;
            }
        }
    }
    // Lo que queda en `known` ya no está en disco.
    for (id, _, _) in known.values() {
        tx.execute("DELETE FROM files WHERE id = ?1", [id])?;
    }
    tx.execute("UPDATE libraries SET last_scan = datetime('now') WHERE id = ?1", [lib.id])?;
    tx.commit()?;
    Ok(())
}

struct PendingFile {
    id: i64,
    item_id: Option<i64>,
    title: String,
    year: Option<i32>,
    hint_imdb: Option<String>,
    hint_tmdb: Option<i64>,
}

/// Archivos sin item, y (si hay clave) archivos cuyo item aún no se ha buscado en TMDb.
fn files_to_match(conn: &Connection, with_key: bool) -> Result<Vec<PendingFile>> {
    let mut stmt = conn.prepare(
        "SELECT f.id, f.item_id, f.parsed_title, f.parsed_year, f.hint_imdb, f.hint_tmdb
         FROM files f LEFT JOIN items i ON i.id = f.item_id
         WHERE f.item_id IS NULL OR (?1 AND i.matched = 0 AND i.match_failed = 0)",
    )?;
    let rows = stmt.query_map([with_key], |r| {
        Ok(PendingFile {
            id: r.get(0)?,
            item_id: r.get(1)?,
            title: r.get::<_, Option<String>>(2)?.unwrap_or_default(),
            year: r.get(3)?,
            hint_imdb: r.get(4)?,
            hint_tmdb: r.get(5)?,
        })
    })?;
    Ok(rows.collect::<rusqlite::Result<_>>()?)
}

async fn match_file(app: &AppHandle, client: Option<&Tmdb>, language: &str, file: PendingFile) -> Result<()> {
    let state = app.state::<AppState>();
    let Some(client) = client else {
        // Sin clave de TMDb: la película aparece igualmente, con el título del archivo.
        if file.item_id.is_none() {
            let conn = state.db();
            let item = insert_placeholder(&conn, &file.title, file.year, file.hint_imdb.as_deref())?;
            conn.execute("UPDATE files SET item_id = ?2 WHERE id = ?1", params![file.id, item])?;
        }
        return Ok(());
    };

    let mut tmdb_id = file.hint_tmdb;
    if tmdb_id.is_none() {
        if let Some(imdb) = &file.hint_imdb {
            tmdb_id = client.find_by_imdb(imdb).await?;
        }
    }
    if tmdb_id.is_none() && !file.title.is_empty() {
        let candidates = client.search(&file.title, file.year, language).await?;
        tmdb_id = tmdb::best_match(&candidates, &file.title, file.year);
    }

    let Some(tmdb_id) = tmdb_id else {
        let conn = state.db();
        match file.item_id {
            Some(item) => conn.execute("UPDATE items SET match_failed = 1 WHERE id = ?1", [item])?,
            None => {
                let item = insert_placeholder(&conn, &file.title, file.year, file.hint_imdb.as_deref())?;
                conn.execute("UPDATE items SET match_failed = 1 WHERE id = ?1", [item])?;
                conn.execute("UPDATE files SET item_id = ?2 WHERE id = ?1", params![file.id, item])?
            }
        };
        return Ok(());
    };

    // ¿Ya la conocíamos? (otra copia, una pendiente o una vista cuyo archivo se borró)
    let existing: Option<(i64, String, bool, i64)> = state
        .db()
        .query_row(
            "SELECT id, title, watchlist, (SELECT COUNT(*) FROM history h WHERE h.item_id = items.id)
             FROM items WHERE kind = 'movie' AND tmdb_id = ?1 AND matched > 0",
            [tmdb_id],
            |r| Ok((r.get(0)?, r.get(1)?, r.get(2)?, r.get(3)?)),
        )
        .optional()?;
    let item = match existing {
        Some((item, title, watchlist, watched)) => {
            if watchlist {
                // Era una pendiente y ya la tienes: sale de Pendientes, como ofrecía el 4.0.
                state.db().execute("UPDATE items SET watchlist = 0, watchlist_at = NULL WHERE id = ?1", [item])?;
                notice(app, "pendingAvailable", title.clone(), Some(item), None);
            }
            if watched > 0 {
                notice(app, "alreadyWatched", title, Some(item), None);
            }
            item
        }
        None => store_details(app, client, tmdb_id, language, file.item_id, None).await?,
    };
    state.db().execute("UPDATE files SET item_id = ?2 WHERE id = ?1", params![file.id, item])?;
    Ok(())
}

fn insert_placeholder(conn: &Connection, title: &str, year: Option<i32>, imdb: Option<&str>) -> Result<i64> {
    conn.execute("INSERT INTO items (title, year, imdb_id) VALUES (?1, ?2, ?3)", params![title, year, imdb])?;
    Ok(conn.last_insert_rowid())
}

/// Descarga la ficha de TMDb, la guarda y baja sus imágenes. Devuelve el id del item.
/// `reuse`: item provisional que pasa a ser esta película. `keep_match`: actualización de un
/// item existente (no cambia su nivel de coincidencia).
async fn store_details(
    app: &AppHandle,
    client: &Tmdb,
    tmdb_id: i64,
    language: &str,
    reuse: Option<i64>,
    keep_match: Option<i64>,
) -> Result<i64> {
    let details = client.details(tmdb_id, language).await?;
    let state = app.state::<AppState>();
    let item = upsert_details(&state.db(), &details, language, 1, keep_match.or(reuse))?;
    download_images(app, item, &details).await;
    Ok(item)
}

/// Guarda la ficha de TMDb. Si ya hay un item con ese `tmdb_id` lo actualiza; si no, reutiliza
/// `reuse` (el item provisional del archivo) o crea uno nuevo.
pub fn upsert_details(conn: &Connection, d: &Details, language: &str, matched: i64, reuse: Option<i64>) -> Result<i64> {
    let tx = conn.unchecked_transaction()?;
    let by_tmdb: Option<i64> = tx
        .query_row("SELECT id FROM items WHERE kind = 'movie' AND tmdb_id = ?1", [d.tmdb_id], |r| r.get(0))
        .optional()?;
    let id = match by_tmdb.or(reuse) {
        Some(id) => id,
        None => {
            tx.execute("INSERT INTO items (title) VALUES (?1)", [&d.title])?;
            tx.last_insert_rowid()
        }
    };
    let json = |v: &Vec<String>| serde_json::to_string(v).unwrap_or_else(|_| "[]".into());
    // Si el item era otra película (identificación corregida), se descartan su póster, fondo y
    // nota antiguos. En SQLite todas las expresiones ven los valores previos de la fila.
    tx.execute(
        "UPDATE items SET
            imdb_id = CASE WHEN tmdb_id = ?2 OR tmdb_id IS NULL THEN COALESCE(?3, imdb_id) ELSE ?3 END,
            imdb_rating = CASE WHEN tmdb_id = ?2 OR tmdb_id IS NULL THEN imdb_rating END,
            imdb_votes = CASE WHEN tmdb_id = ?2 OR tmdb_id IS NULL THEN imdb_votes END,
            poster = CASE WHEN tmdb_id = ?2 THEN poster END,
            backdrop = CASE WHEN tmdb_id = ?2 THEN backdrop END,
            tmdb_id = ?2, title = ?4, original_title = ?5, year = ?6,
            release_date = ?7, runtime = ?8, overview = ?9, tagline = ?10, certification = ?11,
            original_language = ?12, genres = ?13, countries = ?14, languages = ?15, tmdb_rating = ?16,
            tmdb_votes = ?17, trailer_key = ?18, poster_url = ?19, backdrop_url = ?20,
            matched = MAX(matched, ?21), match_failed = 0, meta_language = ?22, updated_at = datetime('now')
         WHERE id = ?1",
        params![
            id, d.tmdb_id, d.imdb_id, d.title, d.original_title, d.year, d.release_date, d.runtime, d.overview,
            d.tagline, d.certification, d.original_language, json(&d.genres), json(&d.countries),
            json(&d.languages), d.rating, d.votes, d.trailer_key, d.poster_path, d.backdrop_path, matched, language
        ],
    )?;
    tx.execute("DELETE FROM item_people WHERE item_id = ?1", [id])?;
    for (role, people) in [("director", &d.directors), ("cast", &d.cast)] {
        for (ord, p) in people.iter().enumerate() {
            let person: i64 = tx.query_row(
                "INSERT INTO people (tmdb_id, name, profile_url) VALUES (?1, ?2, ?3)
                 ON CONFLICT(tmdb_id) DO UPDATE SET name = excluded.name, profile_url = excluded.profile_url
                 RETURNING id",
                params![p.tmdb_id, p.name, p.profile_path],
                |r| r.get(0),
            )?;
            tx.execute(
                "INSERT OR IGNORE INTO item_people (item_id, person_id, role, character, ord) VALUES (?1, ?2, ?3, ?4, ?5)",
                params![id, person, role, p.character, ord as i64],
            )?;
        }
    }
    tx.commit()?;
    Ok(id)
}

/// Pasa archivos, historial, progreso y "Pendiente" de `from` a `into` y borra `from`.
fn merge_items(conn: &Connection, from: i64, into: i64) -> Result<()> {
    let tx = conn.unchecked_transaction()?;
    tx.execute("UPDATE files SET item_id = ?2 WHERE item_id = ?1", [from, into])?;
    tx.execute("UPDATE history SET item_id = ?2 WHERE item_id = ?1", [from, into])?;
    tx.execute(
        "INSERT OR IGNORE INTO playback (item_id, file_id, position, duration, updated_at)
         SELECT ?2, file_id, position, duration, updated_at FROM playback WHERE item_id = ?1",
        [from, into],
    )?;
    tx.execute(
        "UPDATE items SET watchlist = MAX(watchlist, (SELECT watchlist FROM items WHERE id = ?1)),
                          watchlist_at = COALESCE(watchlist_at, (SELECT watchlist_at FROM items WHERE id = ?1))
         WHERE id = ?2",
        [from, into],
    )?;
    tx.execute("DELETE FROM items WHERE id = ?1", [from])?;
    tx.commit()?;
    Ok(())
}

/// Corrige a mano la película de un item. Si esa película ya existía en otro item, se fusionan.
/// Devuelve el id con el que queda la película.
pub async fn identify(app: &AppHandle, item_id: i64, tmdb_id: i64) -> Result<i64> {
    let (client, language) = client(app)?;
    let details = client.details(tmdb_id, &language).await?;
    let state = app.state::<AppState>();
    let target = {
        let conn = state.db();
        let other: Option<i64> = conn
            .query_row(
                "SELECT id FROM items WHERE kind = 'movie' AND tmdb_id = ?1 AND id != ?2",
                params![tmdb_id, item_id],
                |r| r.get(0),
            )
            .optional()?;
        let target = upsert_details(&conn, &details, &language, 2, Some(other.unwrap_or(item_id)))?;
        if target != item_id {
            merge_items(&conn, item_id, target)?;
        }
        conn.execute("UPDATE items SET matched = 2 WHERE id = ?1", [target])?;
        target
    };
    download_images(app, target, &details).await;
    if let Some(imdb) = details.imdb_id {
        let _ = update_imdb_ratings(app, Some(vec![imdb])).await;
    }
    let _ = app.emit("library-changed", ());
    Ok(target)
}

/// Corrige la película de un solo archivo (p. ej. dos películas distintas que se llamaban igual
/// y acabaron en la misma ficha). Si es el único archivo de su ficha, equivale a `identify`.
pub async fn identify_file(app: &AppHandle, file_id: i64, tmdb_id: i64) -> Result<i64> {
    let (item_id, siblings): (i64, i64) = app.state::<AppState>().db().query_row(
        "SELECT item_id, (SELECT COUNT(*) FROM files g WHERE g.item_id = f.item_id) FROM files f WHERE id = ?1",
        [file_id],
        |r| Ok((r.get(0)?, r.get(1)?)),
    )?;
    if siblings <= 1 {
        return identify(app, item_id, tmdb_id).await;
    }
    let (client, language) = client(app)?;
    let details = client.details(tmdb_id, &language).await?;
    let state = app.state::<AppState>();
    let target = {
        let conn = state.db();
        // Sin `reuse`: si la película no existía se crea una ficha nueva para este archivo.
        let target = upsert_details(&conn, &details, &language, 2, None)?;
        conn.execute("UPDATE files SET item_id = ?2 WHERE id = ?1", [file_id, target])?;
        conn.execute("UPDATE items SET matched = 2 WHERE id = ?1", [target])?;
        target
    };
    download_images(app, target, &details).await;
    if let Some(imdb) = details.imdb_id {
        let _ = update_imdb_ratings(app, Some(vec![imdb])).await;
    }
    let _ = app.emit("library-changed", ());
    Ok(target)
}

/// Añade una película que no está en disco, a Pendientes (`pending`) o a Vistas (`watched`).
pub async fn add(app: &AppHandle, tmdb_id: i64, list: &str) -> Result<i64> {
    let (client, language) = client(app)?;
    let state = app.state::<AppState>();
    let existing: Option<i64> = state
        .db()
        .query_row("SELECT id FROM items WHERE kind = 'movie' AND tmdb_id = ?1", [tmdb_id], |r| r.get(0))
        .optional()?;
    let item = match existing {
        Some(id) => id,
        None => store_details(app, &client, tmdb_id, &language, None, None).await?,
    };
    {
        let conn = state.db();
        if list == "watched" {
            db::add_watch(&conn, item, None, "manual")?;
        } else {
            conn.execute("UPDATE items SET watchlist = 1, watchlist_at = datetime('now') WHERE id = ?1", [item])?;
        }
    }
    if existing.is_none() {
        let imdb: Option<String> = state.db().query_row("SELECT imdb_id FROM items WHERE id = ?1", [item], |r| r.get(0))?;
        if let Some(imdb) = imdb {
            let _ = update_imdb_ratings(app, Some(vec![imdb])).await;
        }
    }
    let _ = app.emit("library-changed", ());
    Ok(item)
}

/// Vuelve a descargar la ficha de un item ya identificado.
pub async fn refresh(app: &AppHandle, item_id: i64) -> Result<()> {
    let (client, language) = client(app)?;
    let tmdb_id: Option<i64> = app
        .state::<AppState>()
        .db()
        .query_row("SELECT tmdb_id FROM items WHERE id = ?1", [item_id], |r| r.get(0))?;
    let tmdb_id = tmdb_id.ok_or_else(|| anyhow!("not_identified"))?;
    store_details(app, &client, tmdb_id, &language, None, Some(item_id)).await?;
    let imdb: Option<String> =
        app.state::<AppState>().db().query_row("SELECT imdb_id FROM items WHERE id = ?1", [item_id], |r| r.get(0))?;
    if let Some(imdb) = imdb {
        let _ = update_imdb_ratings(app, Some(vec![imdb])).await;
    }
    let _ = app.emit("library-changed", ());
    Ok(())
}

/// Descarga una imagen de TMDb a la caché. Devuelve la ruta relativa a la caché.
pub async fn fetch_image(http: &reqwest::Client, cache_dir: &Path, remote: &str, size: &str, folder: &str) -> Option<String> {
    let name = remote.trim_start_matches('/');
    let rel = format!("{folder}/{name}");
    let dest = cache_dir.join(&rel);
    if dest.exists() {
        return Some(rel);
    }
    let resp = http.get(format!("{}/{size}/{name}", tmdb::IMAGE_BASE)).send().await.ok()?.error_for_status().ok()?;
    let bytes = resp.bytes().await.ok()?;
    if let Some(dir) = dest.parent() {
        tokio::fs::create_dir_all(dir).await.ok()?;
    }
    let tmp = dest.with_extension("part");
    tokio::fs::write(&tmp, &bytes).await.ok()?;
    tokio::fs::rename(&tmp, &dest).await.ok()?;
    Some(rel)
}

async fn download_images(app: &AppHandle, item: i64, d: &Details) {
    let state = app.state::<AppState>();
    let (http, cache) = (state.http.clone(), state.cache_dir.clone());
    let poster = match &d.poster_path {
        Some(p) => fetch_image(&http, &cache, p, "w500", "posters").await,
        None => None,
    };
    let backdrop = match &d.backdrop_path {
        Some(p) => fetch_image(&http, &cache, p, "w1280", "backdrops").await,
        None => None,
    };
    let _ = state.db().execute(
        "UPDATE items SET poster = COALESCE(?2, poster), backdrop = COALESCE(?3, backdrop) WHERE id = ?1",
        params![item, poster, backdrop],
    );
}

async fn download_missing_images(app: &AppHandle) {
    let state = app.state::<AppState>();
    let missing: Vec<(i64, Option<String>, Option<String>)> = {
        let conn = state.db();
        let Ok(mut stmt) = conn.prepare(
            "SELECT id, CASE WHEN poster IS NULL THEN poster_url END, CASE WHEN backdrop IS NULL THEN backdrop_url END
             FROM items WHERE (poster IS NULL AND poster_url IS NOT NULL) OR (backdrop IS NULL AND backdrop_url IS NOT NULL)",
        ) else {
            return;
        };
        stmt.query_map([], |r| Ok((r.get(0)?, r.get(1)?, r.get(2)?)))
            .map(|rows| rows.filter_map(Result::ok).collect())
            .unwrap_or_default()
    };
    let (http, cache) = (state.http.clone(), state.cache_dir.clone());
    futures::stream::iter(missing)
        .map(|(id, poster_url, backdrop_url)| {
            let (http, cache, app) = (http.clone(), cache.clone(), app.clone());
            async move {
                let poster = match poster_url {
                    Some(p) => fetch_image(&http, &cache, &p, "w500", "posters").await,
                    None => None,
                };
                let backdrop = match backdrop_url {
                    Some(p) => fetch_image(&http, &cache, &p, "w1280", "backdrops").await,
                    None => None,
                };
                let _ = app.state::<AppState>().db().execute(
                    "UPDATE items SET poster = COALESCE(?2, poster), backdrop = COALESCE(?3, backdrop) WHERE id = ?1",
                    params![id, poster, backdrop],
                );
            }
        })
        .buffer_unordered(6)
        .collect::<Vec<()>>()
        .await;
}

/// Actualiza las notas de IMDb de `only` (o de todas las películas si es `None`).
pub async fn update_imdb_ratings(app: &AppHandle, only: Option<Vec<String>>) -> Result<()> {
    let state = app.state::<AppState>();
    let ids: Vec<String> = match only {
        Some(ids) => ids,
        None => {
            let conn = state.db();
            let mut stmt = conn.prepare("SELECT DISTINCT imdb_id FROM items WHERE imdb_id IS NOT NULL")?;
            stmt.query_map([], |r| r.get(0))?.collect::<rusqlite::Result<_>>()?
        }
    };
    if ids.is_empty() {
        return Ok(());
    }
    let path = state.cache_dir.join("title.ratings.tsv.gz");
    imdb::ensure_dataset(&state.http, &path).await?;
    let ratings = tauri::async_runtime::spawn_blocking(move || imdb::lookup(&path, &ids)).await??;
    let conn = state.db();
    let tx = conn.unchecked_transaction()?;
    for (id, (rating, votes)) in ratings {
        tx.execute("UPDATE items SET imdb_rating = ?2, imdb_votes = ?3 WHERE imdb_id = ?1", params![id, rating, votes])?;
    }
    tx.commit()?;
    Ok(())
}
