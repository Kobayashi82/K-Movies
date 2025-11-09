
<div align="center">

![Desktop](https://img.shields.io/badge/Desktop-brown?style=for-the-badge)
![Windows](https://img.shields.io/badge/Windows-Application-blue?style=for-the-badge)
![Movie Manager](https://img.shields.io/badge/Movie-Manager-green?style=for-the-badge)
![VB.NET](https://img.shields.io/badge/Language-VB.NET-red?style=for-the-badge)

*Gestor de películas con integración OMDB*

</div>

# K-Movies 4.0

## 🎯 Descripción

**K-Movies 4.0** es un gestor de colección de películas desarrollado en Visual Basic .NET que permite organizar, visualizar y gestionar tu biblioteca de películas.

Diseñado como una aplicación Windows Forms, K-Movies integra la API de OMDB para obtener información detallada de las películas, incluyendo portadas, clasificaciones, duración, reparto y sinopsis. Con una interfaz tipo galería, podrás navegar fácilmente por tu colección y llevar un seguimiento de las películas vistas y pendientes.

## ✨ Características Principales

### 🎬 Gestión de Películas
- **Organización en categorías**: Movies (colección principal), Pending (pendientes) y Watched (vistas)
- **Galería visual** con portadas de películas en formato grid responsive
- **Importación automática** de películas con detección de formatos (MP4, MKV, AVI, M4V)
- **Información completa** de cada película (título, año, duración, género, director, actores, sinopsis)
- **Seguimiento automático** de películas vistas mediante integración con VLC

### 🔍 Búsqueda y Filtrado
- **Filtros avanzados** por año, género, idioma, país, director y actor
- **Ordenamiento** alfabético (A-Z / Z-A) por diferentes criterios
- **Búsqueda rápida** con atajo F3
- **Navegación por páginas** con zoom ajustable

### 🌐 Integración OMDB
- **Descarga automática** de información desde OMDB API
- **Actualización de metadatos** de películas existentes (F7)
- **Búsqueda manual** de películas en IMDB
- **Descarga de portadas** en alta media
- **Acceso directo** a la página IMDB de cada película

### 🎥 Reproducción
- **Integración con VLC Player** o reproductor predeterminado
- **Película aleatoria** (F9) para cuando no sabes qué ver
- **Reproducción en pantalla completa** automática
- **Control de tiempo** reproducido para marcar automáticamente como vista

### 🎨 Interfaz Moderna
- **Diseño FlatUI** minimalista y elegante
- **Galería responsiva** que se adapta al tamaño de la ventana
- **Tooltips informativos** con detalles al pasar el mouse
- **Vista de trailers** integrada con WebView2

## 🛠️ Componentes del Proyecto

### Formularios Principales

#### `FGallery` - Ventana Principal
- Galería de películas con navegación y filtros
- Menú contextual con todas las opciones
- Barra de herramientas con controles de vista
- Gestión de modos (Movies, Pending, Watched)

#### `FEditInfo` - Editor de Información
- Edición de metadatos de películas
- Búsqueda en IMDB para nuevas películas
- Gestión de portadas
- Validación de duplicados

#### `FAddWatched` - Diálogo de Película Vista
- Confirmación al marcar película como vista
- Opción de mover a Watched o eliminar
- Vista previa de portada

#### `FSettings` - Configuración
- Selección de carpeta de películas
- Configuración de reproductor
- Opciones de auto-watched
- Tiempo mínimo de reproducción

#### `FTrailer` - Reproductor de Trailers
- Integración con WebView2
- Reproducción de trailers desde YouTube

#### `FGallery` - Galería de Imágenes
- Vista ampliada de portadas
- Navegación entre películas

### Clases Principales

#### `Movies_CL` - Gestión de Películas
- Carga y guardado de películas
- Filtrado y ordenamiento
- Gestión de colecciones (Movies, Pending, Watched)
- Integración con IMDB Worker

#### `IMDB_Worker` - Integración OMDB
- Búsqueda de películas en OMDB API
- Descarga de información y portadas
- Procesamiento en segundo plano
- Caché de resultados

#### `ImageGalleryVB` - Control de Galería
- Generación dinámica de grid de imágenes
- Zoom y paginación
- Navegación con teclado
- Selección y highlight de películas

#### `GalleryImage` - Control de Imagen
- Renderizado de portadas con efectos
- Indicadores de estado (copiando, moviendo)
- Eventos de mouse y selección
- Animación flash

#### `FlatUI` - Controles Personalizados
- Botones planos estilizados
- ComboBox personalizado
- ContextMenu con diseño flat
- Efectos hover y animaciones

## 🎯 Funcionalidades Avanzadas

### Gestión Automática
- **Auto-Watched**: Marca automáticamente películas como vistas después de reproducirlas el tiempo configurado
- **Delete Imported**: Elimina archivos originales después de importarlos
- **Actualización masiva**: Actualiza información de todas las películas (F5)

### Operaciones con Películas
- **Importar**: Copia películas desde cualquier ubicación
- **Copiar/Mover**: Reorganiza películas entre categorías
- **Eliminar**: Con confirmación y opción de eliminar archivo físico
- **Editar**: Modifica manualmente cualquier campo
- **Añadir a**: Mueve entre Movies/Pending/Watched

### Atajos de Teclado
- **F3**: Búsqueda rápida
- **F5**: Actualizar listado de películas
- **F7**: Actualizar información IMDB
- **F9**: Reproducir película aleatoria
- **+/-**: Zoom in/out
- **Inicio/Fin**: Primera/Última página
- **A-Z**: Navegar a películas que empiecen con esa letra

## 📋 Requisitos

- **Sistema Operativo**: Windows 7 o superior
- **.NET Framework**: 4.7.2 o superior
- **Reproductor**: VLC Player recomendado (opcional)

## 🚀 Instalación y Uso

### Compilación
1. Abrir `KMovies.sln` en Visual Studio 2013 o superior
2. Restaurar paquetes NuGet (Microsoft.Web.WebView2)
3. Compilar en modo Release
4. El ejecutable se generará en `bin\Release\KMovies.exe`

### Primer Uso
1. Ejecutar `KMovies.exe`
2. Configurar carpeta de películas
3. Configurar ruta de VLC Player (opcional)
4. Configurar opciones de auto-watched
5. La aplicación cargará automáticamente las películas

### Importar Películas
1. Click derecho en galería → Import
2. Seleccionar uno o varios archivos de video
3. La aplicación buscará información en OMDB
4. La película se copiará a la carpeta configurada

## 📊 Estructura de Datos

Las películas se guardan en archivos `.txt` junto al video con el formato:
```
Title|Year|Rating|Duration|Language|Country|Genre|Director|Actor|Plot|ImdbID|Added|Subtitle
```

Las portadas se guardan como `.jpg` con el mismo nombre del archivo de video.

## 🔄 Historial de Versiones

### K-Movies 4.0 (Agosto 2022)
- Reescritura completa de la interfaz
- Integración con WebView2 para trailers
- Sistema de filtros mejorado
- Mejor rendimiento con colecciones grandes
- Sistema de auto-watched con VLC
- **193 películas pendientes** al finalizar el desarrollo

### K-Movies 3.0 (Abril 2018)
- Versión anterior del proyecto

## 📝 Notas de Desarrollo

- La aplicación almacena configuración en el Registro de Windows
- Los datos locales se guardan en `KMLocal\` (Pending y Watched)
- Soporta detección automática de cambios en VLC mediante polling

## 📄 Licencia

Este proyecto está licenciado bajo la WTFPL – [Do What the Fuck You Want to Public License](http://www.wtfpl.net/about/).

---

<div align="center">

**🖥️ Desarrollado por Kobayashi82 🖥️**

*"Tu colección de películas organizada"*

</div>
