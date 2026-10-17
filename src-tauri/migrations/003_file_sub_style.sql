-- Aspecto de los subtítulos cambiado en el reproductor para un archivo concreto.
-- NULL = el de Ajustes generales (así ese archivo sigue los cambios que se hagan allí).

ALTER TABLE file_prefs ADD COLUMN sub_scale REAL;
ALTER TABLE file_prefs ADD COLUMN sub_margin REAL;            -- píxeles de un vídeo de 720 de alto
ALTER TABLE file_prefs ADD COLUMN sub_background INTEGER;
