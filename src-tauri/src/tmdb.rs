//! Cliente mínimo de la API v3 de TMDb.

use crate::parse::normalize;
use anyhow::{Result, anyhow, bail};
use serde_json::Value;
use std::sync::Arc;
use std::time::Duration;
use tokio::sync::Semaphore;

const API: &str = "https://api.themoviedb.org/3";
pub const IMAGE_BASE: &str = "https://image.tmdb.org/t/p";

#[derive(Clone)]
pub struct Tmdb {
    http: reqwest::Client,
    key: String,
    /// Peticiones simultáneas: TMDb tolera bastantes, pero en el primer escaneo de una
    /// biblioteca grande conviene no saturar ni a TMDb ni a la conexión.
    limit: Arc<Semaphore>,
}

#[derive(Debug, Clone)]
#[allow(dead_code)] // `poster_path` y `overview` los usará la corrección manual de coincidencias.
pub struct Candidate {
    pub id: i64,
    pub title: String,
    pub original_title: String,
    pub year: Option<i32>,
    pub popularity: f64,
    pub poster_path: Option<String>,
    pub overview: String,
}

pub struct Details {
    pub tmdb_id: i64,
    pub imdb_id: Option<String>,
    pub title: String,
    pub original_title: Option<String>,
    pub year: Option<i32>,
    pub release_date: Option<String>,
    pub runtime: Option<i64>,
    pub overview: Option<String>,
    pub tagline: Option<String>,
    pub certification: Option<String>,
    pub original_language: Option<String>,
    pub genres: Vec<String>,
    pub countries: Vec<String>,
    pub languages: Vec<String>,
    pub rating: Option<f64>,
    pub votes: Option<i64>,
    pub trailer_key: Option<String>,
    pub poster_path: Option<String>,
    pub backdrop_path: Option<String>,
    pub directors: Vec<PersonRef>,
    pub cast: Vec<PersonRef>,
}

pub struct PersonRef {
    pub tmdb_id: i64,
    pub name: String,
    pub character: Option<String>,
    pub profile_path: Option<String>,
}

fn str_of(v: &Value, k: &str) -> Option<String> {
    v.get(k).and_then(Value::as_str).filter(|s| !s.trim().is_empty()).map(str::to_string)
}

fn year_of(date: Option<&str>) -> Option<i32> {
    date.and_then(|d| d.get(..4)).and_then(|y| y.parse().ok())
}

impl Tmdb {
    pub fn new(http: reqwest::Client, key: &str) -> Self {
        Tmdb { http, key: key.trim().to_string(), limit: Arc::new(Semaphore::new(6)) }
    }

    async fn get(&self, path: &str, query: &[(&str, &str)]) -> Result<Value> {
        let _permit = self.limit.acquire().await?;
        // Admite la clave v3 (32 caracteres) y el token de lectura v4 (un JWT).
        let bearer = self.key.starts_with("eyJ");
        let mut attempt = 0;
        loop {
            attempt += 1;
            let mut req = self.http.get(format!("{API}{path}")).query(query);
            req = if bearer { req.bearer_auth(&self.key) } else { req.query(&[("api_key", &self.key)]) };
            match req.send().await {
                Ok(resp) if resp.status() == reqwest::StatusCode::TOO_MANY_REQUESTS && attempt < 6 => {
                    let wait = resp
                        .headers()
                        .get("retry-after")
                        .and_then(|v| v.to_str().ok())
                        .and_then(|v| v.parse().ok())
                        .unwrap_or(1u64);
                    tokio::time::sleep(Duration::from_secs(wait.max(1))).await;
                }
                Ok(resp) if resp.status() == reqwest::StatusCode::UNAUTHORIZED => bail!("tmdb_unauthorized"),
                Ok(resp) if resp.status().is_server_error() && attempt < 4 => {
                    tokio::time::sleep(Duration::from_millis(500 * attempt)).await;
                }
                Ok(resp) => return Ok(resp.error_for_status()?.json().await?),
                Err(e) if (e.is_timeout() || e.is_connect()) && attempt < 4 => {
                    tokio::time::sleep(Duration::from_millis(800 * attempt)).await;
                }
                Err(e) => return Err(e.into()),
            }
        }
    }

    /// Comprueba que la clave funciona.
    pub async fn validate(&self) -> Result<()> {
        self.get("/configuration", &[]).await.map(|_| ())
    }

    pub async fn search(&self, title: &str, year: Option<i32>, language: &str) -> Result<Vec<Candidate>> {
        let year_s = year.map(|y| y.to_string());
        let mut q = vec![("query", title), ("language", language), ("include_adult", "false")];
        if let Some(y) = &year_s {
            q.push(("year", y));
        }
        let mut res = self.get("/search/movie", &q).await?;
        // Con año a veces falla (el archivo trae el año de estreno local): reintenta sin él.
        if year.is_some() && res["results"].as_array().is_none_or(|a| a.is_empty()) {
            q.pop();
            res = self.get("/search/movie", &q).await?;
        }
        Ok(res["results"]
            .as_array()
            .map(|a| a.iter().map(candidate).collect())
            .unwrap_or_default())
    }

    pub async fn find_by_imdb(&self, imdb_id: &str) -> Result<Option<i64>> {
        let res = self.get(&format!("/find/{imdb_id}"), &[("external_source", "imdb_id")]).await?;
        Ok(res["movie_results"].as_array().and_then(|a| a.first()).and_then(|m| m["id"].as_i64()))
    }

    /// Ficha completa en `language` (p. ej. `es-ES`), completando con inglés lo que falte.
    pub async fn details(&self, id: i64, language: &str) -> Result<Details> {
        let lang = language.split('-').next().unwrap_or("en");
        let videos_lang = format!("{lang},en,null");
        let v = self
            .get(
                &format!("/movie/{id}"),
                &[
                    ("language", language),
                    ("append_to_response", "credits,videos,release_dates"),
                    ("include_video_language", &videos_lang),
                ],
            )
            .await?;
        if v.get("id").is_none() {
            return Err(anyhow!("tmdb: película {id} no encontrada"));
        }

        let mut overview = str_of(&v, "overview");
        let mut tagline = str_of(&v, "tagline");
        let mut title = str_of(&v, "title");
        let mut trailer_key = pick_trailer(&v["videos"]["results"], lang);
        if lang != "en" && (overview.is_none() || title.is_none() || trailer_key.is_none()) {
            if let Ok(en) = self
                .get(&format!("/movie/{id}"), &[("language", "en-US"), ("append_to_response", "videos")])
                .await
            {
                overview = overview.or_else(|| str_of(&en, "overview"));
                tagline = tagline.or_else(|| str_of(&en, "tagline"));
                title = title.or_else(|| str_of(&en, "title"));
                trailer_key = trailer_key.or_else(|| pick_trailer(&en["videos"]["results"], "en"));
            }
        }

        let region = language.split('-').nth(1).unwrap_or("US");
        let release_date = str_of(&v, "release_date");
        let crew = v["credits"]["crew"].as_array().cloned().unwrap_or_default();
        let cast = v["credits"]["cast"].as_array().cloned().unwrap_or_default();
        let person = |p: &Value, character: bool| PersonRef {
            tmdb_id: p["id"].as_i64().unwrap_or_default(),
            name: str_of(p, "name").unwrap_or_default(),
            character: if character { str_of(p, "character") } else { None },
            profile_path: str_of(p, "profile_path"),
        };
        let list = |key: &str, field: &str| -> Vec<String> {
            v[key].as_array().map(|a| a.iter().filter_map(|x| str_of(x, field)).collect()).unwrap_or_default()
        };

        Ok(Details {
            tmdb_id: id,
            imdb_id: str_of(&v, "imdb_id"),
            original_title: str_of(&v, "original_title"),
            title: title.or_else(|| str_of(&v, "original_title")).unwrap_or_default(),
            year: year_of(release_date.as_deref()),
            release_date,
            runtime: v["runtime"].as_i64().filter(|r| *r > 0),
            overview,
            tagline,
            certification: pick_certification(&v["release_dates"]["results"], region),
            original_language: str_of(&v, "original_language"),
            genres: list("genres", "name"),
            countries: list("production_countries", "iso_3166_1"),
            languages: list("spoken_languages", "iso_639_1"),
            rating: v["vote_average"].as_f64().filter(|r| *r > 0.0),
            votes: v["vote_count"].as_i64(),
            trailer_key,
            poster_path: str_of(&v, "poster_path"),
            backdrop_path: str_of(&v, "backdrop_path"),
            directors: crew.iter().filter(|p| p["job"] == "Director").map(|p| person(p, false)).collect(),
            cast: cast.iter().take(20).map(|p| person(p, true)).collect(),
        })
    }
}

fn candidate(m: &Value) -> Candidate {
    Candidate {
        id: m["id"].as_i64().unwrap_or_default(),
        title: str_of(m, "title").unwrap_or_default(),
        original_title: str_of(m, "original_title").unwrap_or_default(),
        year: year_of(m["release_date"].as_str()),
        popularity: m["popularity"].as_f64().unwrap_or_default(),
        poster_path: str_of(m, "poster_path"),
        overview: str_of(m, "overview").unwrap_or_default(),
    }
}

/// Tráiler de YouTube: preferimos oficial, de tipo "Trailer" y en el idioma pedido.
fn pick_trailer(videos: &Value, lang: &str) -> Option<String> {
    let videos = videos.as_array()?;
    videos
        .iter()
        .filter(|v| v["site"] == "YouTube" && v["key"].is_string())
        .max_by_key(|v| {
            let mut score = 0;
            if v["type"] == "Trailer" {
                score += 8;
            } else if v["type"] == "Teaser" {
                score += 4;
            }
            if v["iso_639_1"] == lang {
                score += 4;
            } else if v["iso_639_1"] == "en" {
                score += 2;
            }
            if v["official"] == true {
                score += 1;
            }
            score
        })
        .and_then(|v| str_of(v, "key"))
}

/// Clasificación por edades del país pedido; si no hay, la de EE. UU.
fn pick_certification(results: &Value, region: &str) -> Option<String> {
    let results = results.as_array()?;
    let of = |country: &str| {
        results
            .iter()
            .find(|r| r["iso_3166_1"] == country)
            .and_then(|r| r["release_dates"].as_array())
            .and_then(|dates| dates.iter().find_map(|d| str_of(d, "certification")))
    };
    of(region).or_else(|| of("US"))
}

/// Elige el mejor candidato para un título y año. `None` si ninguno es suficientemente parecido.
pub fn best_match(candidates: &[Candidate], title: &str, year: Option<i32>) -> Option<i64> {
    let wanted = normalize(title);
    if wanted.is_empty() {
        return None;
    }
    let max_pop = candidates.iter().map(|c| c.popularity).fold(1.0, f64::max);
    candidates
        .iter()
        .map(|c| {
            let sim = strsim::jaro_winkler(&wanted, &normalize(&c.title))
                .max(strsim::jaro_winkler(&wanted, &normalize(&c.original_title)));
            let year_score = match (year, c.year) {
                (Some(a), Some(b)) if a == b => 0.25,
                (Some(a), Some(b)) if (a - b).abs() == 1 => 0.15,
                (Some(_), _) => -0.15,
                (None, _) => 0.0,
            };
            let score = sim * 0.75 + year_score + 0.1 * (c.popularity / max_pop);
            (c.id, sim, score)
        })
        .filter(|(_, sim, _)| *sim >= 0.8)
        .max_by(|a, b| a.2.total_cmp(&b.2))
        .map(|(id, _, _)| id)
}

#[cfg(test)]
mod tests {
    use super::*;

    fn c(id: i64, title: &str, year: i32, popularity: f64) -> Candidate {
        Candidate {
            id,
            title: title.into(),
            original_title: title.into(),
            year: Some(year),
            popularity,
            poster_path: None,
            overview: String::new(),
        }
    }

    #[test]
    fn prefers_year_then_popularity() {
        let cs = [c(1, "Dune", 1984, 40.0), c(2, "Dune", 2021, 200.0), c(3, "Dune Drifter", 2020, 5.0)];
        assert_eq!(best_match(&cs, "Dune", Some(1984)), Some(1));
        assert_eq!(best_match(&cs, "Dune", None), Some(2));
        assert_eq!(best_match(&cs, "Something else", None), None);
    }
}
