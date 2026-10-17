# libmpv (reproductor integrado)

El reproductor integrado usa `libmpv-2.dll`. Pesa unos 120 MB, así que no está en git.

- **En desarrollo**, la app la busca aquí (`src-tauri/lib/libmpv-2.dll`); instalada, junto al ejecutable. Si no la encuentra, todo funciona igual con el reproductor externo (VLC u otro).
- **El instalador** la incluye (`bundle.resources` en `tauri.conf.json`), así que para `npm run tauri build` tiene que estar aquí.

## Cómo conseguirla

```sh
npm run libmpv
```

Descarga la compilación probada de shinchiro (`mpv-dev-x86_64-20261006-git-6c092d978b.7z`, API de cliente 2.5), comprueba su SHA-256 y deja `libmpv-2.dll` en esta carpeta. `dev.bat` lo hace solo la primera vez.

Para cambiar de versión: pon otra URL y su SHA-256 en `scripts/get-libmpv.ps1`, borra la DLL de aquí, vuelve a ejecutarlo y pasa las pruebas (`cargo test`: `mpv::tests::options_exist` comprueba que existen las opciones que usa K-Movies).
