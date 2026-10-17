@echo off
rem Arranca K-Movies en modo desarrollo (la interfaz se recarga sola al guardar cambios).
cd /d "%~dp0"
set "PATH=%PATH%;%USERPROFILE%\.cargo\bin"
if not exist node_modules call npm install
rem libmpv (reproductor integrado): se descarga la primera vez; tambien la necesita el instalador.
if not exist src-tauri\lib\libmpv-2.dll call npm run libmpv
call npm run tauri dev
