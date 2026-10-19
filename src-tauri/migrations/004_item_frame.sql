-- Fotograma del vídeo (ruta en la caché) para las películas sin póster o fondo de TMDb.
-- '' = no se pudo sacar (no se reintenta en cada escaneo).

ALTER TABLE items ADD COLUMN frame TEXT;
