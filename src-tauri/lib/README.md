# libmpv (reproductor integrado)

El reproductor integrado usa `libmpv-2.dll`. Pesa unos 120 MB, así que no está en git.

En desarrollo, la app la busca aquí (`src-tauri/lib/libmpv-2.dll`); instalada, junto al ejecutable. Si no la encuentra, todo funciona igual con el reproductor externo (VLC u otro).

## Cómo conseguirla

1. Descarga `mpv-dev-x86_64-<fecha>-git-<hash>.7z` de las compilaciones de shinchiro: https://github.com/shinchiro/mpv-winbuild-cmake/releases
2. Descomprime `libmpv-2.dll` en esta carpeta (el `tar` de Windows 11 abre los `.7z`: `tar -xf mpv-dev-….7z libmpv-2.dll`).

Probada con la compilación del 2026-10-06 (`mpv-dev-x86_64-20261006-git-6c092d978b.7z`, API de cliente 2.5).
