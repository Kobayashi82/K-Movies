# K-Movies 5.0

Catálogo de películas para Windows: escanea tus carpetas, descarga pósters, sinopsis, reparto y notas, y abre las películas en tu reproductor de siempre (VLC, mpv, MPC…).

Sucesor de K-Movies 4.0 (VB.NET). Construido con [Tauri 2](https://tauri.app), React y TypeScript.

## Estado

- **Fase 1 (base):** carpetas, escaneo, identificación con TMDb, notas de IMDb, rejilla de pósters con tamaño ajustable, orden, filtros, búsqueda, ficha, reproducir, Pendientes y Vistas, idiomas (inglés y español) y temas claro/oscuro.
- **Fase 2 (ficha y archivos):** tráiler dentro de la app, corrección manual de la identificación (por ficha o por archivo), añadir películas a Pendientes o Vistas sin tenerlas, importar / copiar a / mover a con progreso, borrar a la papelera, actualizar información y opciones de las tarjetas.

Ver [ESPECIFICACION.md](ESPECIFICACION.md) para el plan completo y [DECISIONES.md](DECISIONES.md) para las decisiones tomadas.

## Desarrollo

Requisitos: [Node.js](https://nodejs.org) 20+, [Rust](https://rustup.rs) y las *Build Tools* de Visual Studio con C++ (ver [requisitos de Tauri](https://tauri.app/start/prerequisites/)).

```sh
npm install
npm run tauri dev      # desarrollo con recarga en caliente
npm run tauri build    # instalador en src-tauri/target/release/bundle/nsis
cd src-tauri && cargo test
```

Necesitas una clave gratuita de [TMDb](https://www.themoviedb.org/settings/api); se introduce en Ajustes dentro de la app.

## Traducciones

Los textos están en [src/i18n/locales/](src/i18n/locales/). Para añadir un idioma, copia `en.json` con el código del idioma (por ejemplo `fr.json`), cambia `_name` y traduce los valores. También se puede dejar el archivo en `%LOCALAPPDATA%\com.kobayashi82.kmovies\locales\` sin recompilar.

## Créditos

Este producto usa la API de TMDB, pero TMDB no lo respalda ni lo certifica. Las notas de IMDb proceden de los [datasets públicos de IMDb](https://developer.imdb.com/non-commercial-datasets/) (uso personal y no comercial).
