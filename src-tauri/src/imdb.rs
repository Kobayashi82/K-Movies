//! Notas de IMDb a partir del dataset público `title.ratings.tsv.gz` (uso personal, se
//! actualiza a diario, ~9 MB). Evita depender de OMDb y de sus límites de peticiones.

use anyhow::Result;
use flate2::read::GzDecoder;
use std::collections::HashMap;
use std::io::{BufRead, BufReader};
use std::path::Path;
use std::time::{Duration, SystemTime};

const URL: &str = "https://datasets.imdbws.com/title.ratings.tsv.gz";
const MAX_AGE: Duration = Duration::from_secs(3 * 24 * 3600);

/// Descarga el dataset si no existe o tiene más de tres días. Si falla la descarga y hay
/// una copia antigua, se sigue usando la antigua.
pub async fn ensure_dataset(http: &reqwest::Client, path: &Path) -> Result<()> {
    let fresh = std::fs::metadata(path)
        .and_then(|m| m.modified())
        .ok()
        .and_then(|t| SystemTime::now().duration_since(t).ok())
        .is_some_and(|age| age < MAX_AGE);
    if fresh {
        return Ok(());
    }
    let download = async {
        let bytes = http.get(URL).send().await?.error_for_status()?.bytes().await?;
        let tmp = path.with_extension("part");
        tokio::fs::write(&tmp, &bytes).await?;
        tokio::fs::rename(&tmp, path).await?;
        anyhow::Ok(())
    };
    match download.await {
        Err(_) if path.exists() => Ok(()),
        other => other,
    }
}

/// Busca la nota y los votos de los ids pedidos recorriendo el archivo una vez.
pub fn lookup(path: &Path, wanted: &[String]) -> Result<HashMap<String, (f64, i64)>> {
    let mut pending: HashMap<&str, ()> = wanted.iter().map(|s| (s.as_str(), ())).collect();
    let mut found = HashMap::new();
    let reader = BufReader::new(GzDecoder::new(std::fs::File::open(path)?));
    for line in reader.lines().skip(1) {
        let line = line?;
        let mut parts = line.split('\t');
        let (Some(id), Some(rating), Some(votes)) = (parts.next(), parts.next(), parts.next()) else { continue };
        if pending.remove(id).is_some() {
            if let (Ok(r), Ok(v)) = (rating.parse(), votes.parse()) {
                found.insert(id.to_string(), (r, v));
            }
            if pending.is_empty() {
                break;
            }
        }
    }
    Ok(found)
}
