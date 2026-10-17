mod db;
mod images;
mod imdb;
mod letterboxd;
mod library;
mod mpv;
mod parse;
mod playback;
mod player;
mod players;
mod scanner;
mod tmdb;
mod transfers;

use regex::Regex;
use rusqlite::Connection;
use serde::Serialize;
use std::collections::HashMap;
use std::path::{Path, PathBuf};
use std::sync::atomic::{AtomicBool, Ordering};
use std::sync::{LazyLock, Mutex, MutexGuard};
use tauri::{AppHandle, Emitter, Manager, State};

pub struct AppState {
    db: Mutex<Connection>,
    data_dir: PathBuf,
    cache_dir: PathBuf,
    http: reqwest::Client,
    scanning: AtomicBool,
    transfers: transfers::Transfers,
    sessions: playback::Sessions,
    player: player::Slot,
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

/// Lanza un escaneo en segundo plano si no hay otro en marcha. Devuelve `false` si ya había uno.
pub fn spawn_scan(app: &AppHandle) -> bool {
    spawn_job(app, |app| async move { library::scan(app).await })
}

fn spawn_job<F, Fut>(app: &AppHandle, job: F) -> bool
where
    F: FnOnce(AppHandle) -> Fut + Send + 'static,
    Fut: std::future::Future<Output = anyhow::Result<()>> + Send,
{
    if app.state::<AppState>().scanning.swap(true, Ordering::SeqCst) {
        return false;
    }
    let app = app.clone();
    tauri::async_runtime::spawn(async move {
        if let Err(e) = job(app.clone()).await {
            eprintln!("[job] {e:#}");
            library::emit(&app, "error", 0, 0, Some(e.to_string()));
        }
        app.state::<AppState>().scanning.store(false, Ordering::SeqCst);
    });
    true
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
fn start_scan(app: AppHandle) -> bool {
    spawn_scan(&app)
}

#[tauri::command]
fn refresh_all(app: AppHandle) -> bool {
    spawn_job(&app, |app| async move { library::refresh_all(app).await })
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
    let conn = state.db();
    conn.execute(
        "UPDATE items SET watchlist = ?2, watchlist_at = CASE WHEN ?2 THEN datetime('now') END WHERE id = ?1",
        rusqlite::params![id, value],
    )
    .map_err(err)?;
    db::prune_orphans(&conn).map_err(err)?;
    let _ = app.emit("library-changed", ());
    Ok(())
}

/// Marcar como vista añade una entrada al historial; desmarcar borra el historial de esa película.
#[tauri::command]
fn set_watched(app: AppHandle, state: State<AppState>, id: i64, value: bool) -> CmdResult<()> {
    let conn = state.db();
    if value {
        db::add_watch(&conn, id, None, "manual").map_err(err)?;
    } else {
        conn.execute("DELETE FROM history WHERE item_id = ?1", [id]).map_err(err)?;
    }
    db::prune_orphans(&conn).map_err(err)?;
    let _ = app.emit("library-changed", ());
    Ok(())
}

#[tauri::command]
fn list_history(state: State<AppState>, item_id: Option<i64>) -> CmdResult<Vec<db::HistoryEntry>> {
    db::history(&state.db(), item_id).map_err(err)
}

static DATE: LazyLock<Regex> = LazyLock::new(|| Regex::new(r"^\d{4}-\d{2}-\d{2}$").unwrap());

/// Añade un visionado en una fecha concreta (`AAAA-MM-DD`) o, sin fecha, ahora.
#[tauri::command]
fn add_history(app: AppHandle, state: State<AppState>, id: i64, date: Option<String>) -> CmdResult<()> {
    let date = date.filter(|d| !d.is_empty());
    if date.as_deref().is_some_and(|d| !DATE.is_match(d)) {
        return Err("bad_date".into());
    }
    db::add_watch(&state.db(), id, date.as_deref(), "manual").map_err(err)?;
    let _ = app.emit("library-changed", ());
    Ok(())
}

#[tauri::command]
fn set_history_date(app: AppHandle, state: State<AppState>, id: i64, date: String) -> CmdResult<()> {
    if !DATE.is_match(&date) {
        return Err("bad_date".into());
    }
    let conn = state.db();
    let item: i64 = conn.query_row("SELECT item_id FROM history WHERE id = ?1", [id], |r| r.get(0)).map_err(err)?;
    conn.execute("UPDATE history SET watched_at = ?2 WHERE id = ?1", rusqlite::params![id, date]).map_err(err)?;
    db::fix_rewatch(&conn, item).map_err(err)?;
    let _ = app.emit("library-changed", ());
    Ok(())
}

#[tauri::command]
fn delete_history(app: AppHandle, state: State<AppState>, id: i64) -> CmdResult<()> {
    let conn = state.db();
    let item: i64 = conn.query_row("SELECT item_id FROM history WHERE id = ?1", [id], |r| r.get(0)).map_err(err)?;
    conn.execute("DELETE FROM history WHERE id = ?1", [id]).map_err(err)?;
    db::fix_rewatch(&conn, item).map_err(err)?;
    db::prune_orphans(&conn).map_err(err)?;
    let _ = app.emit("library-changed", ());
    Ok(())
}

/// Tu nota de un visionado (0,5–5, en medias estrellas); `None` la quita.
#[tauri::command]
fn set_history_rating(app: AppHandle, state: State<AppState>, id: i64, rating: Option<f64>) -> CmdResult<()> {
    db::set_rating(&state.db(), id, rating).map_err(err)?;
    let _ = app.emit("library-changed", ());
    Ok(())
}

/// Escribe el CSV de importación de Letterboxd. Con `only_new`, solo lo que no se haya exportado ya.
#[tauri::command]
fn letterboxd_export(state: State<AppState>, path: String, only_new: bool) -> CmdResult<letterboxd::ExportResult> {
    letterboxd::export(&state.db(), Path::new(&path), only_new).map_err(err)
}

/// Importa la exportación de Letterboxd (el ZIP o un CSV suelto). No va a la vez que un escaneo:
/// los dos crean fichas.
#[tauri::command]
async fn letterboxd_import(app: AppHandle, path: String, watchlist: bool) -> CmdResult<letterboxd::ImportSummary> {
    if app.state::<AppState>().scanning.swap(true, Ordering::SeqCst) {
        return Err("busy".into());
    }
    let result = letterboxd::import(&app, Path::new(&path), watchlist).await;
    app.state::<AppState>().scanning.store(false, Ordering::SeqCst);
    // También si ha fallado a medias: que no se quede el aviso de progreso.
    library::emit(&app, "done", 0, 0, None);
    result.map_err(err)
}

/// Olvida por dónde ibas (la quita de "Seguir viendo").
#[tauri::command]
fn clear_progress(app: AppHandle, state: State<AppState>, id: i64) -> CmdResult<()> {
    let conn = state.db();
    conn.execute("DELETE FROM playback WHERE item_id = ?1", [id]).map_err(err)?;
    db::prune_orphans(&conn).map_err(err)?;
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

static IMDB_ID: LazyLock<Regex> = LazyLock::new(|| Regex::new(r"\btt\d{7,9}\b").unwrap());
static TMDB_URL: LazyLock<Regex> = LazyLock::new(|| Regex::new(r"themoviedb\.org/movie/(\d+)").unwrap());

/// Busca en TMDb. También acepta un id de IMDb (`tt0133093`) o un enlace de TMDb o IMDb.
#[tauri::command]
async fn search_tmdb(app: AppHandle, query: String, year: Option<i32>) -> CmdResult<Vec<tmdb::Candidate>> {
    let (client, language) = library::client(&app).map_err(err)?;
    if let Some(m) = IMDB_ID.find(&query) {
        return match client.find_by_imdb(m.as_str()).await.map_err(err)? {
            Some(id) => Ok(vec![client.candidate_by_id(id, &language).await.map_err(err)?]),
            None => Ok(vec![]),
        };
    }
    if let Some(id) = TMDB_URL.captures(&query).and_then(|c| c[1].parse().ok()) {
        return Ok(vec![client.candidate_by_id(id, &language).await.map_err(err)?]);
    }
    client.search(query.trim(), year, &language).await.map_err(err)
}

#[tauri::command]
async fn identify_item(app: AppHandle, id: i64, tmdb_id: i64) -> CmdResult<i64> {
    library::identify(&app, id, tmdb_id).await.map_err(err)
}

#[tauri::command]
async fn identify_file(app: AppHandle, file_id: i64, tmdb_id: i64) -> CmdResult<i64> {
    library::identify_file(&app, file_id, tmdb_id).await.map_err(err)
}

#[tauri::command]
async fn add_item(app: AppHandle, tmdb_id: i64, list: String) -> CmdResult<i64> {
    library::add(&app, tmdb_id, &list).await.map_err(err)
}

#[tauri::command]
async fn refresh_item(app: AppHandle, id: i64) -> CmdResult<()> {
    library::refresh(&app, id).await.map_err(err)
}

#[tauri::command]
fn detect_players() -> Vec<players::Player> {
    players::detect()
}

/// Reproduce (continuando donde se quedó salvo `from_start`) y sigue el progreso si se puede.
/// Asíncrono: en Windows, crear una ventana (el reproductor integrado) desde un comando síncrono
/// (que corre en el hilo principal) se queda bloqueado.
#[tauri::command]
async fn play_item(app: AppHandle, id: i64, file_id: Option<i64>, from_start: Option<bool>) -> CmdResult<()> {
    playback::play(&app, id, file_id, from_start.unwrap_or(false)).map_err(err)
}

#[tauri::command]
fn now_playing(app: AppHandle) -> Vec<playback::NowPlaying> {
    playback::list(&app)
}

/// ¿Se puede usar el reproductor integrado (está libmpv)?
#[tauri::command]
fn internal_player_available() -> bool {
    mpv::available()
}

#[tauri::command]
fn player_state(app: AppHandle) -> player::PlayerState {
    player::state(&app)
}

#[tauri::command]
fn player_command(app: AppHandle, args: Vec<String>) -> CmdResult<()> {
    player::command(&app, &args).map_err(err)
}

#[tauri::command]
fn player_set(app: AppHandle, name: String, value: String) -> CmdResult<()> {
    player::set(&app, &name, &value).map_err(err)
}

#[tauri::command]
fn player_sub_background(app: AppHandle, on: bool) -> CmdResult<()> {
    player::set_sub_background(&app, on).map_err(err)
}

#[tauri::command]
fn player_sub_margin(app: AppHandle, percent: f64) -> CmdResult<()> {
    player::add_sub_margin(&app, percent).map_err(err)
}

#[tauri::command]
async fn player_fullscreen(app: AppHandle, on: bool) -> CmdResult<()> {
    player::set_fullscreen(&app, on).map_err(err)
}

#[tauri::command]
async fn player_ontop(app: AppHandle, on: bool) -> CmdResult<()> {
    player::set_ontop(&app, on).map_err(err)
}

#[tauri::command]
fn player_aspect(app: AppHandle, ratio: String) -> CmdResult<()> {
    player::set_aspect(&app, &ratio).map_err(err)
}

#[tauri::command]
fn player_crop(app: AppHandle, ratio: f64) -> CmdResult<()> {
    player::set_crop(&app, ratio).map_err(err)
}

#[tauri::command]
fn player_close(app: AppHandle) {
    player::close(&app)
}

fn file_path(state: &AppState, file_id: i64) -> CmdResult<PathBuf> {
    state
        .db()
        .query_row("SELECT path FROM files WHERE id = ?1", [file_id], |r| r.get::<_, String>(0))
        .map(PathBuf::from)
        .map_err(err)
}

/// Copia (o mueve, según el ajuste) vídeos a una carpeta de la biblioteca, como el "Import" del 4.0.
#[tauri::command]
fn import_files(
    app: AppHandle,
    state: State<AppState>,
    paths: Vec<String>,
    library_id: i64,
    conflict: Option<transfers::Conflict>,
) -> CmdResult<()> {
    let (root, delete_original) = {
        let conn = state.db();
        let root: String =
            conn.query_row("SELECT path FROM libraries WHERE id = ?1", [library_id], |r| r.get(0)).map_err(err)?;
        let delete = db::setting(&conn, "importDeleteOriginal").map_err(err)?.is_none_or(|v| v == "true");
        (root, delete)
    };
    for path in paths {
        transfers::enqueue(
            &app,
            transfers::Kind::Import,
            PathBuf::from(path),
            Path::new(&root),
            None,
            delete_original,
            conflict.unwrap_or_default(),
        )
            .map_err(err)?;
    }
    Ok(())
}

#[tauri::command]
fn transfer_file(
    app: AppHandle,
    state: State<AppState>,
    file_id: i64,
    dest_dir: String,
    mode: String,
    conflict: Option<transfers::Conflict>,
) -> CmdResult<()> {
    let source = file_path(&state, file_id)?;
    let kind = if mode == "move" { transfers::Kind::Move } else { transfers::Kind::Copy };
    transfers::enqueue(&app, kind, source, Path::new(&dest_dir), Some(file_id), false, conflict.unwrap_or_default()).map_err(err)
}

#[tauri::command]
fn list_transfers(app: AppHandle) -> Vec<transfers::TransferInfo> {
    transfers::list(&app)
}

#[tauri::command]
fn cancel_transfer(app: AppHandle, id: u64) {
    transfers::cancel(&app, id)
}

#[tauri::command]
fn dismiss_transfers(app: AppHandle, id: Option<u64>) {
    transfers::dismiss(&app, id)
}

/// Manda el archivo a la papelera y lo quita de la biblioteca.
#[tauri::command]
fn delete_file(app: AppHandle, state: State<AppState>, file_id: i64) -> CmdResult<()> {
    let path = file_path(&state, file_id)?;
    if path.exists() {
        transfers::delete_to_trash(&path).map_err(err)?;
    }
    let conn = state.db();
    conn.execute("DELETE FROM files WHERE id = ?1", [file_id]).map_err(err)?;
    db::prune_orphans(&conn).map_err(err)?;
    let _ = app.emit("library-changed", ());
    Ok(())
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
        .register_asynchronous_uri_scheme_protocol("kimg", |ctx, request, responder| {
            let app = ctx.app_handle().clone();
            tauri::async_runtime::spawn(async move { responder.respond(images::handle(app, request).await) });
        })
        .setup(|app| {
            // KMOVIES_DATA_DIR permite usar otra carpeta de datos (pruebas sin tocar la biblioteca real).
            let data_dir = match std::env::var_os("KMOVIES_DATA_DIR") {
                Some(dir) => PathBuf::from(dir),
                None => app.path().app_local_data_dir()?,
            };
            let cache_dir = data_dir.join("cache");
            for dir in ["posters", "backdrops"] {
                std::fs::create_dir_all(cache_dir.join(dir))?;
            }
            app.asset_protocol_scope().allow_directory(&cache_dir, true)?;
            let conn = db::open(&data_dir.join("kmovies.db"))?;
            let http = reqwest::Client::builder()
                .user_agent(concat!("K-Movies/", env!("CARGO_PKG_VERSION")))
                .timeout(std::time::Duration::from_secs(30))
                .build()?;
            app.manage(AppState {
                db: Mutex::new(conn),
                data_dir,
                cache_dir,
                http,
                scanning: AtomicBool::new(false),
                transfers: transfers::Transfers::default(),
                sessions: playback::Sessions::default(),
                player: player::Slot::default(),
            });
            // Cerrar K-Movies cierra también la película (y guarda por dónde ibas).
            if let Some(main) = app.get_webview_window("main") {
                let handle = app.handle().clone();
                main.on_window_event(move |e| player::on_main_event(&handle, e));
            }
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
            refresh_all,
            list_items,
            get_item,
            set_watchlist,
            set_watched,
            list_history,
            add_history,
            set_history_date,
            delete_history,
            set_history_rating,
            letterboxd_export,
            letterboxd_import,
            clear_progress,
            now_playing,
            internal_player_available,
            player_state,
            player_command,
            player_set,
            player_sub_background,
            player_sub_margin,
            player_fullscreen,
            player_ontop,
            player_aspect,
            player_crop,
            player_close,
            validate_tmdb_key,
            search_tmdb,
            identify_item,
            identify_file,
            add_item,
            refresh_item,
            detect_players,
            play_item,
            import_files,
            transfer_file,
            list_transfers,
            cancel_transfer,
            dismiss_transfers,
            delete_file,
            user_locales,
        ])
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
}
