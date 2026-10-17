# libmpv (reproductor integrado)

El reproductor integrado usa `libmpv-2.dll`. Pesa unos 120 MB, así que no está en git.

- **En desarrollo**, la app la busca aquí (`src-tauri/lib/libmpv-2.dll`). Si no la encuentra, todo funciona igual con el reproductor externo (VLC u otro).
- **En la compilación final** (`npm run tauri build`) va **comprimida dentro del ejecutable** (`build.rs`, zstd: unos 35 MB), así que `K-Movies.exe` es un solo archivo. La primera vez que se abre la descomprime en `%LOCALAPPDATA%\kmovies\lib\libmpv-2-<huella>.dll` (en segundo plano, un segundo más o menos) y después la usa desde ahí. Una versión nueva de la DLL lleva otra huella: se descomprime aparte y la vieja se borra.
- Si hay una `libmpv-2.dll` suelta junto al ejecutable, se usa esa (sirve para probar otra versión sin recompilar).

## Cómo conseguirla

```sh
npm run libmpv
```

Descarga la compilación probada de shinchiro (`mpv-dev-x86_64-20261006-git-6c092d978b.7z`, API de cliente 2.5), comprueba su SHA-256 y deja `libmpv-2.dll` en esta carpeta. Tiene que estar aquí antes de `npm run tauri build`: si no, el ejecutable sale sin reproductor integrado.

Para cambiar de versión: pon otra URL y su SHA-256 en `scripts/get-libmpv.ps1`, borra la DLL de aquí, vuelve a ejecutarlo y pasa las pruebas (`cargo test`: `mpv::tests::options_exist` comprueba que existen las opciones que usa K-Movies).
