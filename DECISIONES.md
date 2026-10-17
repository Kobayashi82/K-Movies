# Decisiones

Registro de decisiones tomadas durante el desarrollo, como pide `ESPECIFICACION.md`: qué se decidió, por qué y qué alternativa se descartó. Lo más reciente va al final de cada sección.

## Confirmadas con el usuario (2026-10-17)

| Tema | Decisión |
|---|---|
| Programa antiguo | Es **K-Movies 4.0** (VB.NET + WinForms + WebView2), del propio usuario. Referencia, no plantilla. |
| Series | Solo películas por ahora, pero el modelo de datos queda preparado (`items.kind`, `libraries.kind`). |
| Listas | Se mantiene el flujo del 4.0: **Películas** (en disco), **Pendientes** (lista de deseos, sin archivo) y **Vistas** (historial, sobrevive aunque se borre el archivo). |
| Importar / Copiar a / Mover a | Se mantienen las dos, como en el 4.0 (fase 2). |
| Idioma de la interfaz | Multiidioma: inglés y español incluidos; por defecto el del sistema; ampliable sin recompilar. |
| Tema | Claro y oscuro (y "el del sistema"). |
| Letterboxd | Importar también la lista de vistas del usuario (ya estaba en la especificación). |

## Lo que se aprendió del K-Movies 4.0

- Datos junto a cada película: `Título.jpg` (póster), `Título.txt` oculto con `imdbID|nota|año|duración|idioma|país|género|director|actores|sinopsis|NO_Auto_Watch|NO_Auto_Pending`, y `cache\Título.jpg` (miniatura).
- Ajustes en `HKCU\Software\KMovies 4.0` (carpeta `V:\Videos\Peliculas`, reproductor VLC, umbral de vista: 20 min).
- Pendientes y Vistas eran archivos `.mp4` falsos (texto) en `KMLocal\Pending` y `KMLocal\Watched` junto al ejecutable. En este PC están vacías.
- VLC se seguía con la interfaz HTTP (`/requests/status.xml`, contraseña fija) editando `vlcrc`.
- Funciones que no estaban en la especificación y se conservan: importar copiando con progreso (y borrar el original), copiar/mover a otra carpeta, película al azar, filtros por idioma y país, indicador de subtítulos, avisos de "ya la viste" / "está en Pendientes".
- La fecha de "añadido" era la de modificación del archivo (había una herramienta, *Modified Date Changer*, para ajustarla).

## Técnicas

### Stack
- **Tauri 2 + React 19 + TypeScript + Vite**, como proponía la especificación. Rust 1.99, MSVC Build Tools y WebView2 ya estaban instalados; no hizo falta nada más.
- **zustand** para el estado (mínimo y sin ceremonia). Descartado: Redux (excesivo), Context (re-renders en cascada con miles de items).
- **@tanstack/react-virtual** para la rejilla (virtualización por filas).
- **Motion** para animaciones de componentes (pestañas, paneles, modal, fundidos del fondo).
- **View Transitions API** para la transición póster → ficha. Con Motion habría que poner `layoutId` en cada tarjeta, y eso hace que al mover el deslizador de tamaño se animen cientos de tarjetas a la vez. La API nativa solo nombra el póster pulsado y no cuesta nada mientras no se usa.
- **i18next** para idiomas. Los idiomas son archivos JSON en `src/i18n/locales/` (incluidos en la app) y en `<datos>/locales/` (los añade el usuario sin recompilar). Cada archivo declara su nombre en `_name`.
- **lucide-react** para iconos.

### Datos
- **SQLite** (`rusqlite` con SQLite incluido) en `%LOCALAPPDATA%\com.kobayashi82.kmovies\kmovies.db`. Imágenes en `...\cache\posters` y `...\cache\backdrops`. Se usa *Local* y no *Roaming* porque la caché puede ocupar cientos de MB.
- Un **item** es la obra; los **archivos** cuelgan de él. Un item puede no tener archivos (pendiente o vista ya borrada). Al terminar un escaneo se borran los items sin archivos, sin historial y fuera de Pendientes.
- Géneros, países e idiomas van como JSON en la propia fila del item (conjuntos pequeños que solo se filtran). Las personas sí van en tablas propias (`people`, `item_people`) porque tendrán fotos y, en el futuro, páginas propias.
- **Filtrado y orden en el frontend**, en memoria. Con miles de películas tarda milisegundos y evita una consulta por cada pulsación. SQLite es el almacenamiento, no el motor de búsqueda.
- **Fecha de añadido**: en el primer escaneo de una carpeta se toma la fecha de modificación del archivo (como hacía el 4.0, así se conserva su orden); después, la fecha en que se detecta.
- Los ajustes (incluida la clave de TMDb) están en la tabla `settings`, fuera del repositorio. Alternativa pendiente de valorar: el almacén de credenciales de Windows para la clave.
- Si la carpeta de una biblioteca no está accesible (disco de red apagado), el escaneo la salta y no borra nada.

### Metadatos
- **TMDb** para identificar y para la ficha. Se aceptan la clave v3 y el token de lectura v4.
- Identificación, por orden: id de TMDb en un `.nfo` → id de IMDb (del `.txt` del 4.0, de un `.nfo` de Jellyfin/Kodi o del nombre del archivo) → búsqueda por título y año. La búsqueda puntúa similitud del título (Jaro-Winkler sobre texto normalizado), año exacto o ±1 y popularidad; por debajo de 0,8 de similitud no se acepta y la película queda "sin identificar" (con el título del archivo) para corregirla a mano.
- Idioma de los datos configurable (por defecto, el de la interfaz); lo que falte se completa en inglés.
- **Notas de IMDb: dataset oficial `title.ratings.tsv.gz`** (verificado: 8,7 MB, se actualiza a diario). Se descarga como mucho cada 3 días y se recorre una vez por escaneo. Descartado OMDb: límite de 1000 peticiones/día, hace falta clave, y el 4.0 llevaba claves incrustadas en el código.
- Imágenes: póster `w500` y fondo `w1280` descargados al identificar. Las fotos del reparto se cargan de TMDb al abrir la ficha (pendiente: cachearlas).
- Concurrencia: 6 peticiones simultáneas a TMDb, con reintentos ante 429 (respetando `Retry-After`), errores 5xx y cortes de red.

### Reproductor
- Detección en rutas típicas de Archivos de programa y AppData (VLC, mpv/mpv.net, MPC-HC, MPC-BE, PotPlayer). Sin reproductor configurado ni detectado se usa el predeterminado de Windows.
- Ya se pasan los parámetros para empezar en un punto (`--start-time`, `--start`, `/start`, `/seek`); el seguimiento del progreso llega en la fase 3.

### Ventana
- Sin bordes del sistema, con barra de título propia (`data-tauri-drag-region`), para que la app se vea como una sola superficie.
- La ventana arranca oculta y se muestra cuando React ha pintado, para evitar el destello blanco.

### Filtros
- Dentro de una categoría basta con uno de los valores (O); entre categorías, todas (Y). Excepción: los géneros, donde elegir varios exige todos ("Comedia" + "Romance").
- Las opciones de cada filtro se calculan sobre la pestaña actual sin filtrar, para que no desaparezcan al ir eligiendo.
