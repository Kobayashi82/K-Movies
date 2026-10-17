# Catálogo de películas — Especificación inicial

> **Para el modelo que lea esto (Claude Code):** este documento es una **propuesta razonada, no un contrato**. Cada decisión técnica explica *por qué* se eligió y qué alternativas hay. Si ves una opción mejor (más simple, más robusta, más moderna o más adecuada a lo que encuentres en el PC del usuario), **cámbiala**. Reglas:
> - Cambios pequeños o de detalle: decide tú y anótalo en `DECISIONES.md` (qué, por qué, alternativa descartada).
> - Cambios grandes (framework, base de datos, arquitectura, quitar una función): **explica la propuesta al usuario y pide confirmación antes**.
> - Lo marcado como **[verificar]** son detalles técnicos que pueden haber cambiado o no estar confirmados: compruébalos antes de depender de ellos.
> - Lo marcado como **[obligatorio]** es lo que el usuario quiere sí o sí. El *cómo* sigue siendo flexible.

---

## 1. Contexto

El usuario tiene un programa antiguo en Windows para catalogar sus películas: añade películas, el programa busca la información en internet, y desde ahí puede abrirlas en VLC (u otro reproductor configurable), ver el trailer dentro del programa, ordenar y filtrar. Funciona, pero es viejo y visualmente pobre.

También usa Jellyfin, que le gusta, pero no le convence la reproducción (sobre todo los subtítulos: pequeños y sin fondo) y no puede abrir directamente en VLC. Este programa es la alternativa: **local, ligero, bonito y con su reproductor de siempre**.

**Objetivo:** rehacer el programa con todas sus funciones actuales, una interfaz moderna, fluida y animada, e integración real con los reproductores (continuar, progreso, historial) y con Letterboxd.

**Fuera de alcance (no-objetivos):** servidor, streaming a otros dispositivos, transcodificación, multiusuario. Es una app de escritorio para un solo PC.

---

## 2. Requisitos funcionales

### 2.1 Biblioteca **[obligatorio]**
- Añadir una o varias carpetas (locales o de red). Escanear y detectar películas.
- Reescaneo manual y, si es viable, detección automática de archivos nuevos/borrados.
- Extraer título y año del nombre de archivo/carpeta (formatos típicos: `Título (2010)`, `Titulo.2010.1080p.BluRay...`).
- Pantalla para **corregir una coincidencia errónea** (buscar a mano y elegir la película correcta).

### 2.2 Metadatos **[obligatorio]**
Póster, fondo (backdrop), sinopsis, fecha de estreno, duración, géneros, director, reparto, nota de IMDb, otras notas si están disponibles, trailer.
- Idioma preferente español, con *fallback* a inglés cuando falte información.
- Cachear imágenes y datos en local para que la app sea rápida y funcione sin conexión.

### 2.3 Navegación, orden y filtros **[obligatorio]**
- **Ordenar por:** nombre, fecha de añadido, fecha de estreno, nota IMDb, duración, última vez vista… (todo lo que tenga sentido).
- **Filtrar por:** nota IMDb (rango), duración (rango), rating/clasificación, director, actor, género, año, vista/no vista.
- Búsqueda rápida por texto.
- Combinar varios filtros a la vez.

### 2.4 Reproducción **[obligatorio]**
- Abrir la película en el reproductor elegido en ajustes.
- Ver el **trailer dentro del programa**.

### 2.5 Integración con reproductores **[obligatorio en VLC, deseable en el resto]**
- **Continuar donde se quedó**: guardar la posición y lanzar el reproductor en ese punto.
- **Seguir el progreso** mientras se reproduce.
- **Marcar como vista** automáticamente al superar un umbral (propuesta: 90 %, configurable) y añadirla al **historial** con fecha.
- Sección **"Seguir viendo"**.
- Marcar vista/no vista a mano.

### 2.6 Letterboxd **[deseable]**
- Exportar el historial en el CSV que Letterboxd acepta para importar.
- Importar la exportación de Letterboxd del usuario para marcar lo que ya ha visto (y sus notas).

### 2.7 Migración desde el programa antiguo **[pendiente de confirmar]**
No sabemos todavía qué programa es ni si exporta datos. **Pregunta al usuario** al empezar; si tiene base de datos o exportación accesible, importar al menos la lista de películas y su estado de visto.

---

## 3. Interfaz y experiencia **[obligatorio en el espíritu, libre en el detalle]**

El usuario quiere algo **moderno, dinámico, fluido y animado**. Referencia mental: Jellyfin / Plex / Netflix, pero más ágil.

Ideas propuestas (cámbialas si se te ocurre algo mejor):
- **Rejilla de pósters grandes** con un **deslizador para cambiar el tamaño** en tiempo real **[obligatorio]**.
- Al pasar el ratón: leve escalado, sombra, y el fondo de la app cambia suavemente al backdrop de esa película.
- Ficha de película con transición animada desde el póster (*shared element transition*): backdrop grande, datos, reparto con fotos, botones Reproducir / Continuar / Trailer / Marcar vista.
- Panel lateral de filtros plegable; chips para los filtros activos.
- Fila superior "Seguir viendo" con barra de progreso sobre el póster.
- Tema oscuro por defecto.
- Navegación con teclado.
- **Rendimiento:** la rejilla debe ir fluida con miles de películas → virtualización y carga diferida de imágenes. Las animaciones nunca deben costar fluidez.

---

## 4. Stack propuesto (y por qué)

| Pieza | Propuesta | Por qué | Alternativas razonables |
|---|---|---|---|
| App de escritorio | **Tauri 2** | Ejecutable pequeño, poco consumo de RAM, usa WebView2 (ya viene en Windows 11). Backend en Rust con acceso al sistema (lanzar procesos, sockets, named pipes). | **Electron**: más fácil y ecosistema enorme, pero pesa mucho más. **WinUI 3 / .NET** o **Avalonia**: nativo, pero las animaciones ricas cuestan más. **Flutter**: buena animación, ecosistema de escritorio menor. |
| Interfaz | **React + TypeScript** (o **Svelte**) | Interfaces web dan la mayor libertad visual y de animación. | Svelte/SolidJS si se prefiere menos peso. Elige lo que mejor domines. |
| Animaciones | **Motion (antes Framer Motion)** o View Transitions API | Transiciones de layout y elementos compartidos con poco código. | GSAP, CSS puro. |
| Rejilla | Virtualizada (p. ej. TanStack Virtual) | Miles de pósters sin tirones. | Cualquier solución equivalente. |
| Base de datos | **SQLite** | Un único archivo local, rápido, consultas complejas para filtros y orden. | JSON plano (no escala bien con filtros); DuckDB (excesivo). |
| Metadatos | **TMDb API** | Gratuita con clave, imágenes de calidad, reparto, director, trailers (claves de YouTube), en español. | Leer **NFO** existentes (ver nota abajo). |
| Nota IMDb | **OMDb API** | Da la nota de IMDb a partir del `imdb_id` que proporciona TMDb. | Datasets públicos de IMDb (`title.ratings.tsv.gz`), descargables y sin límite de peticiones — puede ser mejor opción **[verificar]**. |

**Nota NFO:** el usuario tiene Jellyfin. Si sus carpetas tienen archivos `.nfo` y pósters generados por Jellyfin, se podrían reutilizar como primera fuente (sin llamadas a la red). Evalúa si merece la pena.

**Claves de API:** el usuario las introduce en Ajustes. Guardarlas fuera del repositorio (almacén de credenciales de Windows o archivo de configuración del usuario). **Nunca** en el código ni en git.

---

## 5. Integración con reproductores (detalle técnico)

Diseñar una **interfaz común** (p. ej. `PlayerAdapter`: `launch(file, startSeconds)`, `getStatus() → {position, duration, paused, ended}`) con un adaptador por reproductor. Detectar automáticamente qué reproductores están instalados (registro de Windows / rutas típicas) y permitir ruta manual.

| Reproductor | Arrancar en un punto | Leer progreso | Nivel |
|---|---|---|---|
| **VLC** (prioritario) | `--start-time=<seg>` | Interfaz HTTP: lanzar con `--extraintf=http --http-host=127.0.0.1 --http-port=<p> --http-password=<pw>` y consultar `/requests/status.json` (auth básica, usuario vacío) **[verificar]** | Completo |
| **mpv / mpv.net** | `--start=<seg>` | IPC JSON: `--input-ipc-server=\\.\pipe\<nombre>`, propiedades `time-pos` y `duration` | Completo |
| **MPC-HC / MPC-BE** | `/start <ms>` **[verificar]** | Interfaz web (puerto por defecto 13579, `/variables.html`); el usuario debe activarla — avisarle desde la app | Completo |
| **PotPlayer** | `/seek=hh:mm:ss` **[verificar]** | Sin API cómoda | Básico |
| **Cualquier otro** | Si no hay forma, solo abrir | Medir tiempo con el proceso abierto; al cerrar, preguntar "¿La terminaste?" | Básico |

Comportamiento:
- Consultar el estado cada pocos segundos mientras el proceso esté vivo; guardar posición periódicamente (por si se cierra mal).
- Al cerrar: guardar última posición; si supera el umbral → vista + entrada en historial.
- Si una película vista se vuelve a ver entera → nueva entrada en historial (*rewatch*).
- Evitar conflicto con la función propia de VLC de "continuar reproducción" (puede preguntar por su cuenta) **[verificar]** y desactivarla para esa ejecución si es posible.

---

## 6. Letterboxd

**No hay API pública abierta** (el acceso se concede a pocos proyectos) **[verificar si ha cambiado]**. Por eso la propuesta es vía CSV:

- **Exportar** el historial en el formato de importación de Letterboxd. Columnas aceptadas (comprobar la documentación actual de su importador) **[verificar]**: `Title`, `Year`, `imdbID`, `tmdbID`, `WatchedDate` (AAAA-MM-DD), `Rating` (0,5–5), `Rewatch`, `Tags`, `Review`. Incluir `tmdbID`/`imdbID` hace que la coincidencia sea exacta.
- Opción de exportar **solo lo nuevo** desde la última exportación, para no duplicar entradas del diario.
- **Importar** el ZIP de exportación de Letterboxd (`watched.csv`, `diary.csv`, `ratings.csv`) para marcar vistas y notas.
- Permitir que el usuario ponga nota (estrellas) al terminar una película, para que viaje en el CSV.

Si en el futuro el usuario quiere sincronización automática, **Trakt** tiene API abierta y es una alternativa a valorar.

---

## 7. Modelo de datos (orientativo)

- `movies`: id, tmdb_id, imdb_id, título, título original, año, fecha de estreno, duración, sinopsis, nota IMDb, clasificación, rutas de póster/backdrop en caché, trailer, fecha de añadido.
- `files`: id, movie_id, ruta, tamaño, fecha de modificación (una película puede tener varias versiones).
- `people` + `movie_people` (rol: director / actor, orden en el reparto).
- `genres` + `movie_genres`.
- `playback`: movie_id, última posición, duración, actualizado.
- `history`: movie_id, fecha vista, nota del usuario, rewatch, exportado_a_letterboxd.
- `settings`.

Ajusta libremente; es solo un punto de partida.

---

## 8. Fases sugeridas

Cada fase debe dejar la app **usable**. Puedes reordenar si tiene sentido.

1. **Base**: proyecto Tauri, ajustes (carpetas, clave TMDb), escaneo, coincidencia con TMDb, caché, rejilla de pósters con tamaño ajustable, orden, filtros y búsqueda.
   *Hecho cuando:* añade una carpeta y ve su biblioteca con pósters, puede ordenar y filtrar con fluidez.
2. **Ficha y reproducción**: ficha con transiciones, trailer integrado, abrir en reproductor configurable, corrección manual de coincidencias.
3. **Integración de reproductores**: adaptadores (VLC primero, luego mpv, MPC, básico), continuar, progreso, vista automática, historial, "Seguir viendo".
4. **Letterboxd**: exportar/importar CSV, notas propias.
5. **Pulido**: animaciones, teclado, rendimiento con bibliotecas grandes, instalador.

---

## 9. Riesgos y cosas a comprobar pronto

- **Trailer de YouTube dentro de WebView2**: los embeds a veces fallan en webviews (cabeceras/referrer, restricciones del vídeo) **[verificar]**. Alternativas: ajustar el origen del embed, abrir en ventana aparte, o descargar con yt-dlp y reproducir en local.
- **Límites de las APIs** (TMDb, OMDb) en el primer escaneo de una biblioteca grande: cola con límite de velocidad y reintentos.
- **Coincidencias erróneas** por nombres de archivo raros: la corrección manual es imprescindible.
- **Rutas de red**: tiempos de espera y biblioteca disponible aunque el disco no esté conectado.

---

## 10. Preguntas abiertas para el usuario

- ¿Qué programa usa ahora y se pueden exportar sus datos?
- ¿Solo películas, o también series en el futuro?
- ¿Dónde están las películas (carpetas locales, disco de red, varias)?
- ¿Algún detalle del programa actual que le encante y no se nos deba escapar?
