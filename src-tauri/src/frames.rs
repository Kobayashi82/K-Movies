//! Fotograma de la película (con libmpv) para las que no tienen póster ni fondo de TMDb: hace de
//! póster (con el título encima) y de fondo.

use crate::mpv::{Event, Mpv};
use crate::{AppState, library};
use anyhow::{Result, bail};
use rusqlite::params;
use std::path::{Path, PathBuf};
use std::time::{Duration, Instant};
use tauri::{AppHandle, Emitter, Manager};

/// Carpeta de los fotogramas dentro de la caché.
const FOLDER: &str = "frames";
/// Si un archivo no da imagen en este tiempo (disco de red lento, archivo roto), se deja.
const TIMEOUT: Duration = Duration::from_secs(25);

/// Guarda en `dest` (JPG, 780 px de ancho) un fotograma de `video` a la cuarta parte de la
/// película: lejos de los logotipos del principio y de los créditos.
pub fn extract(video: &Path, dest: &Path) -> Result<()> {
    let dir = dest.with_extension("tmp");
    let _ = std::fs::remove_dir_all(&dir);
    std::fs::create_dir_all(&dir)?;
    let result = (|| {
        let dir_s = dir.to_string_lossy().into_owned();
        let mpv = Mpv::new(&[
            ("vo", "image"),
            ("vo-image-format", "jpg"),
            ("vo-image-jpeg-quality", "85"),
            ("vo-image-outdir", &dir_s),
            ("vf", "scale=w=780:h=-2"),
            ("ao", "null"),
            ("aid", "no"),
            ("sid", "no"),
            ("hwdec", "no"),
            ("start", "25%"),
            ("frames", "1"),
            ("keep-open", "no"),
        ])?;
        mpv.command(&["loadfile", &video.to_string_lossy()])?;
        let started = Instant::now();
        loop {
            match mpv.wait_event(1.0) {
                Some(Event::EndFile { .. } | Event::Shutdown) => break,
                _ if started.elapsed() > TIMEOUT => bail!("frame_timeout"),
                _ => {}
            }
        }
        drop(mpv);
        let frame = std::fs::read_dir(&dir)?
            .flatten()
            .map(|e| e.path())
            .find(|p| p.extension().is_some_and(|e| e.eq_ignore_ascii_case("jpg")));
        match frame {
            Some(f) => Ok(std::fs::rename(f, dest)?),
            None => bail!("no_frame"),
        }
    })();
    let _ = std::fs::remove_dir_all(&dir);
    result
}

/// Saca el fotograma de las películas en disco que no tienen póster o fondo de TMDb y aún no lo
/// tienen. Si falla se apunta (`frame = ''`) para no reintentarlo en cada escaneo.
pub async fn missing(app: &AppHandle) -> Result<()> {
    if !crate::mpv::available() {
        return Ok(());
    }
    let state = app.state::<AppState>();
    let pending: Vec<(i64, String, String)> = {
        let conn = state.db();
        let mut stmt = conn.prepare(
            "SELECT i.id, i.title, (SELECT f.path FROM files f WHERE f.item_id = i.id ORDER BY f.size DESC LIMIT 1)
             FROM items i
             WHERE i.frame IS NULL AND (i.poster_url IS NULL OR i.backdrop_url IS NULL)
               AND EXISTS (SELECT 1 FROM files f WHERE f.item_id = i.id)",
        )?;
        stmt.query_map([], |r| Ok((r.get(0)?, r.get(1)?, r.get(2)?)))?.collect::<rusqlite::Result<_>>()?
    };
    let total = pending.len();
    let dir = state.cache_dir.join(FOLDER);
    std::fs::create_dir_all(&dir)?;
    for (n, (id, title, path)) in pending.into_iter().enumerate() {
        library::emit(app, "frames", n, total, Some(title.clone()));
        let rel = format!("{FOLDER}/{id}.jpg");
        let dest: PathBuf = state.cache_dir.join(&rel);
        let video = PathBuf::from(path);
        let ok = tauri::async_runtime::spawn_blocking(move || extract(&video, &dest)).await?;
        if let Err(e) = &ok {
            eprintln!("[frames] {title}: {e:#}");
        }
        let value = if ok.is_ok() { rel.as_str() } else { "" };
        state.db().execute("UPDATE items SET frame = ?2 WHERE id = ?1", params![id, value])?;
        if ok.is_ok() {
            let _ = app.emit("library-changed", ());
        }
    }
    Ok(())
}

#[cfg(test)]
mod tests {
    use super::*;

    /// Saca un fotograma de un vídeo de prueba generado por FFmpeg (se salta si no está libmpv).
    #[test]
    fn extracts_a_frame() {
        if !crate::mpv::available() {
            eprintln!("libmpv no está: prueba saltada");
            return;
        }
        let dir = std::env::temp_dir().join(format!("kmovies-frame-{}", std::process::id()));
        std::fs::create_dir_all(&dir).unwrap();
        let dest = dir.join("1.jpg");
        extract(Path::new("av://lavfi:testsrc=duration=8:size=1280x720:rate=25"), &dest).unwrap();
        let bytes = std::fs::read(&dest).unwrap();
        // JPG de verdad (empieza por FF D8).
        assert!(bytes.len() > 1000 && bytes[..2] == [0xFF, 0xD8]);
        std::fs::remove_dir_all(&dir).unwrap();
    }
}

