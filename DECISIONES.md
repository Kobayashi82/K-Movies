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
- Ya se pasan los parámetros para empezar en un punto (`--start-time`, `--start`, `/start`, `/seek`); el seguimiento del progreso está en la fase 3 (abajo).

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

## Fase 3 (2026-10-17)

### Seguimiento de VLC
- **Una instancia de VLC por reproducción, con su propia interfaz HTTP** en `127.0.0.1`, un puerto libre que elige Windows y una contraseña aleatoria de un solo uso. Todo va en la línea de comandos: **no se toca `vlcrc`**. Descartado lo del 4.0 (editar `vlcrc` para dejar la interfaz web siempre activa en el puerto 8080 con una contraseña fija y consultar cualquier VLC abierto): cambiaba la configuración del usuario, chocaba si el puerto estaba ocupado y podía confundir un VLC abierto a mano con la película.
- Parámetros (comprobados en VLC 3.0.24): `--no-one-instance` y `--no-one-instance-when-started-from-file` (si no, con VLC ya abierto el archivo iría a esa ventana y no se podría seguir), `--qt-continue=0` (tu `vlcrc` tiene `qt-continue=1`: VLC preguntaría "¿continuar?" por su cuenta; ya arrancamos donde toca con `--start-time`) y `--play-and-exit` (al acabar la película VLC se cierra y vuelves a K-Movies).
- Se lee `/requests/status.json` cada 2 s (`state`, `time`, `length` y el nombre del archivo, para no contar otra cosa que se abra en esa ventana). La lectura usa un cliente sin proxy, para que un proxy del sistema no se meta en las peticiones a `127.0.0.1`.
- **Posición**: se guarda cada 15 s y al pausar (por si algo se cierra mal), y al cerrar VLC. Por debajo de 1 minuto no se guarda: abrir la película y cerrarla no cuenta (y no pisa una posición anterior).
- **Vista**: al pasar el umbral (90 % por defecto, configurable en Ajustes entre 75 y 98 %) se marca **en ese momento**, sin esperar a cerrar (los créditos no hace falta verlos). Se añade al historial con origen "reproductor" y se borra la posición. Si ya estaba vista, cuenta como revisionado.
- La posición se guarda por película (no por archivo), con el archivo con el que se empezó: "Continuar" vuelve a ese archivo si sigue ahí.
- **Otros reproductores**: solo se sabe cuándo se cierran. Si estuvieron abiertos 20 minutos o más, se pregunta "¿La has terminado?". mpv (IPC) y MPC (interfaz web) quedan pendientes para tener seguimiento completo.

### Historial
- Cada visionado es una entrada con fecha; "revisionado" se recalcula siempre (todos menos el primero por fecha), así que añadir una fecha antigua o borrar una entrada deja las marcas bien.
- Se pueden añadir visionados con fecha, cambiar la fecha (pulsándola) y borrar entradas, en la ficha. Las fechas puestas a mano son solo el día (`AAAA-MM-DD`); las del reproductor, fecha y hora en UTC.
- Quitar "Vista" a una película con varios visionados pide confirmación, porque borra todo su historial.
- La pestaña Vistas tiene dos modos: **Pósters** (como antes) e **Historial** (cada visionado por meses, como el diario de Letterboxd). La búsqueda y los filtros también se aplican al historial.

### Seguir viendo
- Fila encima de la rejilla de Películas con las películas a medias que siguen en disco, de la última que viste a la primera. Se oculta al buscar o filtrar (ahí estorbaría).
- Va dentro del contenedor de la rejilla virtualizada (como cabecera medida), así se desplaza con ella sin romper la virtualización.
- Tarjetas apaisadas con el fondo de la película, el progreso y lo que queda; el botón de reproducir continúa directamente y la X olvida la posición.
- Mientras se ve, el aviso de la esquina, la ficha y la barra del póster muestran el progreso en directo (evento `playback`), sin recargar la biblioteca cada 2 s.

## Reproductor integrado (prueba de concepto, 2026-10-17)

Confirmado con el usuario: ver las películas dentro de la app, con todos los formatos, cambio de pistas de audio y subtítulos, estilo y posición de los subtítulos, retardo del audio y de los subtítulos, e imagen en imagen. VLC (u otro) queda como opción en Ajustes.

- **libmpv** (`libmpv-2.dll`, compilaciones de shinchiro) y no el `<video>` de WebView2. El del navegador no lee bien MKV, no tiene AC3, DTS ni TrueHD, no cambia de pista de audio de forma fiable, solo acepta WebVTT (nada de PGS o VobSub) y no permite retrasar el audio. Convertir al vuelo queda fuera del alcance. mpv lo trae todo (FFmpeg) y cada ajuste se cambia en marcha. Descartado libVLC: el estilo de los subtítulos se cambia peor en marcha.
- **Carga dinámica** (`libloading`), sin enlazar al compilar. Si la DLL no está, la app funciona igual con el reproductor externo. Se busca junto al ejecutable y, en desarrollo, en `src-tauri/lib/` (fuera de git por su tamaño: 121 MB, unos 31 MB comprimida; ver `src-tauri/lib/README.md`). Falta incluirla en el instalador.
- **Cómo se compone**: una ventana de Tauri sin bordes y transparente. mpv dibuja en ella como ventana hija (opción `wid`) y se manda al fondo del orden Z (`SetWindowPos(HWND_BOTTOM)`). WebView2, con fondo transparente, queda encima con los controles. **Verificado que funciona** (vídeo HEVC, controles HTML encima, subtítulos de mpv visibles). Lo único que no se puede es desenfocar (`backdrop-filter`) lo que hay detrás de un panel, porque el vídeo no pasa por el WebView: los paneles son casi opacos.
- Alternativas que no han hecho falta: ventana de controles flotante aparte y los controles de mpv (`uosc`).
- Los comandos que crean o cambian la ventana del reproductor son **asíncronos**: en Windows, crear una ventana desde un comando síncrono (que corre en el hilo principal) se queda bloqueado (se reprodujo: la ventana se quedaba en `about:blank`).
- Ajustes de mpv: `vo=gpu-next`, `hwdec=auto-safe`, sin atajos ni ratón propios (los lleva la interfaz web), `sub-auto=fuzzy` (subtítulos externos con nombre parecido) y la posición de inicio con `start`.
- **Subtítulos**: tamaño (`sub-scale`), posición (`sub-pos`) y caja de fondo (`sub-border-style=background-box`, `sub-back-color` semitransparente) o contorno clásico. (Más abajo, cómo quedó: lo general en Ajustes y lo que se cambie en el reproductor, por película.) Mientras se ven los controles, los subtítulos suben (`sub-margin-y`) para no quedar tapados.
- **Retardos** del audio (pasos de 50 ms) y de los subtítulos (100 ms, también con Z/X como en mpv): por archivo, no se recuerdan (de momento).
- Los botones de "más/menos" mandan `add` a mpv en vez de calcular el valor en la interfaz: el estado llega 4 veces por segundo y con clics rápidos se sumaba sobre un valor viejo. El volumen y el estilo de los subtítulos los guarda el backend cuando mpv confirma el cambio.
- ~~**Imagen en imagen**~~: se quitó (ver "Ventana aparte y siempre encima" más abajo).
- El seguimiento (posición, vista al pasar del umbral, historial) es el mismo que con VLC: la lógica está ahora en `playback::Tracker`, común a los dos.
- "Automático" en Ajustes → Reproductor usa el integrado si está libmpv y, si no, el primer reproductor que encuentre.

## Reproductor integrado: idiomas, memoria por archivo e instalador (2026-10-17)

- **Idiomas preferidos** (Ajustes → Reproductor integrado): audio preferido (o versión original) y subtítulos (o ninguno), con "solo si el audio no está en ese idioma". Se traducen a `alang`, `slang` y `subs-with-matching-audio=forced`: si el audio ya es el tuyo, solo se ponen los subtítulos forzados (carteles). Cada idioma se busca por sus etiquetas habituales (`es,spa,esp,cas`, `en,eng`…). Por defecto, el idioma de la interfaz para los dos y subtítulos solo si hacen falta: "castellano y, si no hay, original con subtítulos".
- **Memoria por archivo** (tabla `file_prefs`, migración 002): pistas de audio y subtítulos y retardos. Se guardan solo si se cambiaron a mano durante la película, para que la elección automática por idioma siga funcionando en lo demás. Van por archivo y no por película porque los números de pista son de cada archivo. Al abrir se recuperan solo si esas pistas siguen existiendo. Detalle: al cerrar, mpv descarga el archivo y la lista de pistas llega vacía, así que se guarda la última lista no vacía.
- **Estilo de los subtítulos en Ajustes**, con vista previa sobre el fondo de una de tus películas: tamaño, altura, color (blanco o amarillo) y caja de fondo. La vista previa usa las medidas de mpv (vídeo de referencia de 720 líneas, letra de 38, borde de 3, margen de 22) en unidades del alto del recuadro (`cqh`), y se ha comparado con lo que dibuja mpv de verdad.
- **ASS**: por defecto se respeta su estilo propio (`sub-ass-override=scale`: solo se escala). Una casilla permite imponer el nuestro (`force`). Los ASS de anime suelen llevar carteles y karaoke que se estropean al forzarlo.
- Las opciones de estilo e idioma se aplican después de inicializar mpv y sin abortar si alguna no existe, por si se usa otra versión de la DLL. Una prueba (`mpv::tests::options_exist`) comprueba que existen todas en la DLL de `src-tauri/lib`.
- **Instalador**: `libmpv-2.dll` va como recurso de Tauri y se instala junto al ejecutable (unos 31 MB más en el instalador). Para no subir 121 MB a git, `npm run libmpv` (`scripts/get-libmpv.ps1`) descarga la compilación probada y comprueba su SHA-256. `dev.bat` lo hace solo la primera vez.
- Ajustes vuelve a leer los ajustes de la base de datos al abrirse, porque la ventana del reproductor también guarda (volumen y estilo de los subtítulos).

## Reproductor: ventana aparte, siempre encima y subtítulos por película (2026-10-17)

- **Ventana aparte**, que se abre al pulsar Reproducir, con un botón de **siempre encima** junto a la X (tecla T). Se quita la imagen en imagen: la ventana ya se puede hacer pequeña y poner encima de todo, que es lo mismo. Cuando la ventana es pequeña (menos de 560×320), los controles se reducen solos: reproducir/pausa en el centro, los botones de la ventana y la barra de progreso, y se puede arrastrar desde cualquier punto.
- **Descartado: el reproductor dentro de la ficha.** Se probó, y técnicamente funciona: la ventana del vídeo pasa a ser hija de la principal (`SetParent` + `WS_CHILD`), se coloca en un hueco 16:9 arriba de la ficha (la ficha le dice dónde con su `getBoundingClientRect`) y se compensa el borde de redimensionar que Tauri sigue reservando. Al usuario no le convenció; preferencia: ventana aparte. Si se recupera, los detalles a tener en cuenta eran: ocultar el vídeo cuando un diálogo de la ventana principal se le pone encima (el vídeo nativo tapa el HTML), la zona del vídeo fija para que no vaya a saltos al desplazar, y la pantalla completa sacándolo un momento de la ventana principal.
- Sin menú del navegador con el botón derecho en el reproductor.
- **Aspecto de los subtítulos: lo general en Ajustes, y lo que se cambie en el reproductor, solo para esa película.** Primero se guardaba lo del reproductor para todas; el usuario prefirió que el reproductor no cambie lo general y que se recuerde por película (tabla `file_prefs`, migración 003). Del aspecto solo se guarda lo que difiere de Ajustes, para que esa película siga los cambios generales en lo demás. El volumen sí se recuerda para todas.
- Pasos en el reproductor: tamaño de 5 en 5 % y altura de 1 en 1 %. Si se mantiene pulsado el botón, se repite. La altura la suma el backend sobre su valor real, igual que `add` en mpv, para que los clics rápidos no se pisen.
- **La altura pasa a ser la distancia al borde inferior** (0–40 % del alto, 3 % por defecto = los 22 de 720 de mpv) con `sub-margin-y`, en vez de `sub-pos`. Así se puede bajar más que antes (hasta pegado al borde) y el número se entiende. Con los controles a la vista, los subtítulos suben lo que haga falta para no quedar tapados (el mayor de los dos márgenes).
- **Más opciones en Ajustes**: letra (fuentes de Windows o la de mpv), negrita, color (blanco, amarillo, gris claro), caja de fondo y su opacidad, contorno (con caja: margen de la caja), sombra y el estilo propio de los ASS. Botón para restablecer. La vista previa refleja todo.
- Limitación conocida: los subtítulos ASS traen sus márgenes y no suben con los controles (los SRT sí).
