mod db;
mod imdb;
mod library;
mod parse;
mod players;
mod scanner;
mod tmdb;

use rusqlite::Connection;
use serde::Serialize;
use std::collections::HashMap;
use std::path::{Path, PathBuf};
use std::sync::atomic::{AtomicBool, Ordering};
use std::sync::{Mutex, MutexGuard};
use tauri::{AppHandle, Emitter, Manager, State};

pub struct AppState {
    db: Mutex<Connection>,
    data_dir: PathBuf,
    cache_dir: PathBuf,
    http: reqwest::Client,
    scanning: AtomicBool,
}

impl AppState {
    pub fn db(&self) -> MutexGuard<'_, Connection> {
        self.db.lock().unwrap_or_else(|e| e.into_inner())
    }
}

type CmdResult<T> = Result<T, String>;

fn err(e: impl std::fmt::Display) -> String {
    e.to_string()
}

#[derive(Serialize)]
#[serde(rename_all = "camelCase")]
struct AppInfo {
    version: String,
    data_dir: String,
    scanning: bool,
}

#[tauri::command]
fn app_info(app: AppHandle, state: State<AppState>) -> AppInfo {
    AppInfo {
        version: app.package_info().version.to_string(),
        data_dir: state.data_dir.to_string_lossy().into_owned(),
        scanning: state.scanning.load(Ordering::Relaxed),
    }
}

#[tauri::command]
fn get_settings(state: State<AppState>) -> CmdResult<HashMap<String, String>> {
    db::settings(&state.db()).map_err(err)
}

#[tauri::command]
fn set_setting(state: State<AppState>, key: String, value: String) -> CmdResult<()> {
    db::set_setting(&state.db(), &key, &value).map_err(err)
}

#[tauri::command]
fn list_libraries(state: State<AppState>) -> CmdResult<Vec<db::Library>> {
    db::libraries(&state.db()).map_err(err)
}

#[tauri::command]
fn add_library(state: State<AppState>, path: String) -> CmdResult<()> {
    state
        .db()
        .execute("INSERT OR IGNORE INTO libraries (path) VALUES (?1)", [path.trim_end_matches(['\\', '/'])])
        .map(|_| ())
        .map_err(err)
}

#[tauri::command]
fn remove_library(app: AppHandle, state: State<AppState>, id: i64) -> CmdResult<()> {
    let conn = state.db();
    conn.execute("DELETE FROM libraries WHERE id = ?1", [id]).map_err(err)?;
    db::prune_orphans(&conn).map_err(err)?;
    let _ = app.emit("library-changed", ());
    Ok(())
}

#[tauri::command]
fn start_scan(app: AppHandle, state: State<AppState>) -> bool {
    if state.scanning.swap(true, Ordering::SeqCst) {
        return false;
    }
    tauri::async_runtime::spawn(async move {
        if let Err(e) = library::scan(app.clone()).await {
            eprintln!("[scan] {e:#}");
            let _ = app.emit(
                "scan-progress",
                library::Progress { phase: "error", current: 0, total: 0, message: Some(e.to_string()) },
            );
        }
        app.state::<AppState>().scanning.store(false, Ordering::SeqCst);
    });
    true
}

#[tauri::command]
fn list_items(state: State<AppState>) -> CmdResult<Vec<db::ItemSummary>> {
    db::list_items(&state.db(), &state.cache_dir).map_err(err)
}

#[tauri::command]
fn get_item(state: State<AppState>, id: i64) -> CmdResult<db::ItemDetail> {
    db::item_detail(&state.db(), id).map_err(err)
}

#[tauri::command]
fn set_watchlist(app: AppHandle, state: State<AppState>, id: i64, value: bool) -> CmdResult<()> {
    state
        .db()
        .execute(
            "UPDATE items SET watchlist = ?2, watchlist_at = CASE WHEN ?2 THEN datetime('now') END WHERE id = ?1",
            rusqlite::params![id, value],
        )
        .map_err(err)?;
    let _ = app.emit("library-changed", ());
    Ok(())
}

/// Marcar como vista añade una entrada al historial; desmarcar borra el historial de esa película.
#[tauri::command]
fn set_watched(app: AppHandle, state: State<AppState>, id: i64, value: bool) -> CmdResult<()> {
    let conn = state.db();
    if value {
        conn.execute(
            "INSERT INTO history (item_id, watched_at, rewatch, source)
             VALUES (?1, datetime('now'), EXISTS (SELECT 1 FROM history WHERE item_id = ?1), 'manual')",
            [id],
        )
    } else {
        conn.execute("DELETE FROM history WHERE item_id = ?1", [id])
    }
    .map_err(err)?;
    let _ = app.emit("library-changed", ());
    Ok(())
}

#[tauri::command]
async fn validate_tmdb_key(state: State<'_, AppState>, key: String) -> CmdResult<bool> {
    let client = tmdb::Tmdb::new(state.http.clone(), &key);
    match client.validate().await {
        Ok(()) => Ok(true),
        Err(e) if e.to_string() == "tmdb_unauthorized" => Ok(false),
        Err(e) => Err(err(e)),
    }
}

#[tauri::command]
fn detect_players() -> Vec<players::Player> {
    players::detect()
}

#[tauri::command]
fn play_item(state: State<AppState>, id: i64) -> CmdResult<()> {
    let (file, player, position) = {
        let conn = state.db();
        let file = db::largest_file(&conn, id).map_err(err)?.ok_or("file_missing")?;
        let player = db::setting(&conn, "playerPath").map_err(err)?.filter(|p| !p.is_empty());
        let position: Option<f64> = conn
            .query_row("SELECT position FROM playback WHERE item_id = ?1", [id], |r| r.get(0))
            .ok();
        (file, player, position)
    };
    let player = player
        .map(PathBuf::from)
        .or_else(|| players::detect().into_iter().next().map(|p| PathBuf::from(p.path)));
    match player {
        Some(player) => players::launch(&player, Path::new(&file), position).map_err(err),
        // Sin reproductor configurado ni detectado: el predeterminado de Windows.
        None => tauri_plugin_opener::open_path(&file, None::<&str>).map_err(err),
    }
}

/// Idiomas extra que el usuario deja en `<datos>/locales/*.json` (sin recompilar la app).
#[tauri::command]
fn user_locales(state: State<AppState>) -> HashMap<String, serde_json::Value> {
    let mut out = HashMap::new();
    let Ok(entries) = std::fs::read_dir(state.data_dir.join("locales")) else { return out };
    for entry in entries.flatten() {
        let path = entry.path();
        if path.extension().is_some_and(|e| e.eq_ignore_ascii_case("json")) {
            let code = path.file_stem().unwrap_or_default().to_string_lossy().into_owned();
            if let Some(json) = std::fs::read_to_string(&path).ok().and_then(|s| serde_json::from_str(&s).ok()) {
                out.insert(code, json);
            }
        }
    }
    out
}

#[cfg_attr(mobile, tauri::mobile_entry_point)]
pub fn run() {
    tauri::Builder::default()
        .plugin(tauri_plugin_opener::init())
        .plugin(tauri_plugin_dialog::init())
        .setup(|app| {
            let data_dir = app.path().app_local_data_dir()?;
            let cache_dir = data_dir.join("cache");
            for dir in ["posters", "backdrops"] {
                std::fs::create_dir_all(cache_dir.join(dir))?;
            }
            let conn = db::open(&data_dir.join("kmovies.db"))?;
            let http = reqwest::Client::builder()
                .user_agent(concat!("K-Movies/", env!("CARGO_PKG_VERSION")))
                .timeout(std::time::Duration::from_secs(30))
                .build()?;
            app.manage(AppState { db: Mutex::new(conn), data_dir, cache_dir, http, scanning: AtomicBool::new(false) });
            Ok(())
        })
        .invoke_handler(tauri::generate_handler![
            app_info,
            get_settings,
            set_setting,
            list_libraries,
            add_library,
            remove_library,
            start_scan,
            list_items,
            get_item,
            set_watchlist,
            set_watched,
            validate_tmdb_key,
            detect_players,
            play_item,
            user_locales,
        ])
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
}
