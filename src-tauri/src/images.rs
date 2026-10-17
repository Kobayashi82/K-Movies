//! Protocolo `kimg`: sirve imágenes de TMDb (fotos del reparto, etc.) desde la caché y las
//! descarga la primera vez. En Windows la interfaz las pide como `http://kimg.localhost/w185/abc.jpg`.

use crate::{AppState, library};
use regex::Regex;
use std::sync::LazyLock;
use tauri::http::{Request, Response, header};
use tauri::{AppHandle, Manager};

static VALID: LazyLock<Regex> =
    LazyLock::new(|| Regex::new(r"^/(w\d{2,4}|h\d{2,4}|original)/([A-Za-z0-9_-]+\.(?:jpg|png))$").unwrap());

pub async fn handle(app: AppHandle, request: Request<Vec<u8>>) -> Response<Vec<u8>> {
    let not_found = || Response::builder().status(404).body(Vec::new()).unwrap();
    let path = request.uri().path().to_string();
    let Some(caps) = VALID.captures(&path) else { return not_found() };
    let (size, name) = (&caps[1], &caps[2]);
    let state = app.state::<AppState>();
    let folder = format!("tmdb/{size}");
    let Some(rel) = library::fetch_image(&state.http, &state.cache_dir, name, size, &folder).await else {
        return not_found();
    };
    match tokio::fs::read(state.cache_dir.join(rel)).await {
        Ok(bytes) => Response::builder()
            .header(header::CONTENT_TYPE, if name.ends_with(".png") { "image/png" } else { "image/jpeg" })
            .header(header::CACHE_CONTROL, "max-age=31536000, immutable")
            .body(bytes)
            .unwrap(),
        Err(_) => not_found(),
    }
}
