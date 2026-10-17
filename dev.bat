@echo off
rem Arranca K-Movies en modo desarrollo (la interfaz se recarga sola al guardar cambios).
cd /d "%~dp0"
set "PATH=%PATH%;%USERPROFILE%\.cargo\bin"
if not exist node_modules call npm install
call npm run tauri dev
