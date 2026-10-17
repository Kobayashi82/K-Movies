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

## Fase 2 (2026-10-17)

### Ficha y reproducción
- **Tráiler dentro de la app** con el reproductor incrustado de `youtube-nocookie.com`. Comprobado en desarrollo y en el `.exe` final (el riesgo del .md con WebView2 no se ha dado). Queda el botón "Ver en YouTube" por si un vídeo concreto no permite incrustarse.
- **Fotos del reparto en caché** con un protocolo propio, `kimg` (`http://kimg.localhost/w185/…`): la primera vez se descargan de TMDb y después se sirven desde `cache/tmdb/`. Solo acepta rutas con el formato de TMDb, para que no se pueda leer nada fuera de la caché.

### Identificación manual
- Buscador de TMDb que también acepta un id de IMDb (`tt…`) o un enlace de TMDb o IMDb.
- **Identificar la ficha entera**: si la película elegida ya existía en otra ficha, se **fusionan** (archivos, historial, progreso y Pendientes pasan a la existente). Si la ficha era antes otra película, se descartan su póster, fondo y nota antiguos.
- **Identificar un solo archivo** (botón en cada archivo cuando una ficha tiene varios): sirve para dos películas distintas que se llamaban igual. El archivo pasa a la ficha de la película elegida, o a una nueva.
- Las identificaciones manuales quedan marcadas (`matched = 2`) y los reescaneos no las tocan: solo se identifican archivos sin ficha o fichas sin coincidencia.
- Al identificar, se ignora el sufijo de copia " (2)" del nombre de archivo.

### Pendientes y Vistas sin archivo
- Botón "+" en las pestañas Pendientes y Vistas para añadir películas que no están en disco, desde TMDb.
- Si aparece en disco una película que estaba en Pendientes, **sale de Pendientes automáticamente** y se avisa (el 4.0 lo preguntaba; aquí se quita y se avisa, porque Pendientes significa "conseguirla"). Si ya la habías visto, se avisa "Ya viste…".

### Importar, copiar, mover y borrar
- Las transferencias van **de una en una** (varias copias simultáneas al mismo disco son más lentas), con progreso, cancelación y una pila de avisos en la esquina.
- Al mover dentro del mismo disco se renombra (instantáneo); si no, se copia y se borra el original.
- Los **subtítulos y `.nfo`** con el mismo nombre viajan con el vídeo y se renombran igual que él.
- Copia temporal `*.kmpart` que se renombra al terminar: una copia cortada nunca deja un vídeo a medias con el nombre bueno.
- **Nombre repetido en el destino**: se pregunta *Guardar los dos* (el nuevo pasa a "Nombre (2).ext"), *Reemplazar* (el anterior va a la papelera) o *Cancelar*. Nunca se sobrescribe sin preguntar.
- Mover o copiar un archivo de la biblioteca **conserva su película** (aunque se identificara a mano): se actualiza la base de datos en vez de reidentificar.
- "Borrar el original al importar" activado por defecto, como lo tenías en el 4.0 (`Delete_Imported = True`).
- **Borrar** manda el vídeo y sus subtítulos a la **papelera de reciclaje** (crate `trash`), nunca borrado directo.
- Arrastrar vídeos a la ventana los importa.

### Interfaz
- Confirmaciones con un **diálogo propio** con el estilo de la app, en lugar de los diálogos nativos de Windows.
- Opciones de las tarjetas: nota en el póster (IMDb, TMDb, ambas en la misma fila, o ninguna), título visible u oculto, y año visible u oculto (siempre a la derecha del título, en la misma fila).
- `KMOVIES_DATA_DIR` permite arrancar la app con otra carpeta de datos (para probar sin tocar la biblioteca real).
