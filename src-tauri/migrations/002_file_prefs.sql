-- Pistas y retardos que se eligieron para cada archivo en el reproductor integrado.
-- Van por archivo (no por película) porque los números de pista son de cada archivo.

CREATE TABLE file_prefs (
    file_id      INTEGER PRIMARY KEY REFERENCES files(id) ON DELETE CASCADE,
    aid          TEXT,                                   -- id de la pista de audio
    sid          TEXT,                                   -- id de la pista de subtítulos, o 'no'
    audio_delay  REAL NOT NULL DEFAULT 0,                -- segundos
    sub_delay    REAL NOT NULL DEFAULT 0
);
