-- Esquema inicial de K-Movies 5.0.
-- Un "item" es una obra (película; en el futuro también serie). Puede existir sin archivos:
-- así sobreviven las vistas aunque se borre el archivo y las pendientes que aún no se tienen.

CREATE TABLE libraries (
    id          INTEGER PRIMARY KEY,
    path        TEXT NOT NULL UNIQUE COLLATE NOCASE,
    kind        TEXT NOT NULL DEFAULT 'movies',          -- 'movies' | 'series' (futuro)
    added_at    TEXT NOT NULL DEFAULT (datetime('now')),
    last_scan   TEXT
);

CREATE TABLE items (
    id                 INTEGER PRIMARY KEY,
    kind               TEXT NOT NULL DEFAULT 'movie',    -- 'movie' | 'series' (futuro)
    tmdb_id            INTEGER,
    imdb_id            TEXT,
    title              TEXT NOT NULL,
    original_title     TEXT,
    year               INTEGER,
    release_date       TEXT,
    runtime            INTEGER,                          -- minutos
    overview           TEXT,
    tagline            TEXT,
    certification      TEXT,
    original_language  TEXT,
    genres             TEXT NOT NULL DEFAULT '[]',       -- JSON: ["Drama", ...]
    countries          TEXT NOT NULL DEFAULT '[]',       -- JSON: códigos ISO 3166-1
    languages          TEXT NOT NULL DEFAULT '[]',       -- JSON: códigos ISO 639-1
    tmdb_rating        REAL,
    tmdb_votes         INTEGER,
    imdb_rating        REAL,
    imdb_votes         INTEGER,
    trailer_key        TEXT,                             -- clave de YouTube
    poster_url         TEXT,                             -- ruta remota de TMDb
    backdrop_url       TEXT,
    poster             TEXT,                             -- archivo en caché
    backdrop           TEXT,
    matched            INTEGER NOT NULL DEFAULT 0,       -- 0 sin coincidencia, 1 automática, 2 manual
    match_failed       INTEGER NOT NULL DEFAULT 0,       -- se buscó con clave y no se encontró
    meta_language      TEXT,                             -- idioma con el que se descargaron los datos
    watchlist          INTEGER NOT NULL DEFAULT 0,       -- "Pendientes"
    watchlist_at       TEXT,
    added_at           TEXT NOT NULL DEFAULT (datetime('now')),
    updated_at         TEXT
);
CREATE UNIQUE INDEX items_tmdb ON items(kind, tmdb_id) WHERE tmdb_id IS NOT NULL;
CREATE INDEX items_imdb ON items(imdb_id);

CREATE TABLE files (
    id            INTEGER PRIMARY KEY,
    item_id       INTEGER REFERENCES items(id) ON DELETE SET NULL,
    library_id    INTEGER REFERENCES libraries(id) ON DELETE CASCADE,
    path          TEXT NOT NULL UNIQUE COLLATE NOCASE,
    size          INTEGER NOT NULL DEFAULT 0,
    mtime         INTEGER NOT NULL DEFAULT 0,            -- segundos Unix
    parsed_title  TEXT,
    parsed_year   INTEGER,
    hint_imdb     TEXT,                                  -- de .txt del 4.0, .nfo o nombre
    hint_tmdb     INTEGER,
    has_subs      INTEGER NOT NULL DEFAULT 0,
    added_at      TEXT NOT NULL
);
CREATE INDEX files_item ON files(item_id);

CREATE TABLE people (
    id           INTEGER PRIMARY KEY,
    tmdb_id      INTEGER UNIQUE,
    name         TEXT NOT NULL,
    profile_url  TEXT
);

CREATE TABLE item_people (
    item_id    INTEGER NOT NULL REFERENCES items(id) ON DELETE CASCADE,
    person_id  INTEGER NOT NULL REFERENCES people(id),
    role       TEXT NOT NULL,                            -- 'director' | 'cast'
    character  TEXT,
    ord        INTEGER NOT NULL DEFAULT 0,
    PRIMARY KEY (item_id, person_id, role)
);
CREATE INDEX item_people_item ON item_people(item_id, role, ord);

CREATE TABLE playback (
    item_id     INTEGER PRIMARY KEY REFERENCES items(id) ON DELETE CASCADE,
    file_id     INTEGER REFERENCES files(id) ON DELETE SET NULL,
    position    REAL NOT NULL,                           -- segundos
    duration    REAL,
    updated_at  TEXT NOT NULL
);

CREATE TABLE history (
    id           INTEGER PRIMARY KEY,
    item_id      INTEGER NOT NULL REFERENCES items(id) ON DELETE CASCADE,
    watched_at   TEXT NOT NULL,                          -- AAAA-MM-DD o fecha y hora
    rating       REAL,                                   -- 0.5–5 (Letterboxd)
    rewatch      INTEGER NOT NULL DEFAULT 0,
    source       TEXT NOT NULL DEFAULT 'manual',         -- 'manual' | 'player' | 'letterboxd' | 'kmovies4'
    exported_at  TEXT
);
CREATE INDEX history_item ON history(item_id);

CREATE TABLE settings (
    key    TEXT PRIMARY KEY,
    value  TEXT NOT NULL
);
