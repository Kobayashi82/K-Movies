# Descarga libmpv-2.dll (reproductor integrado) en src-tauri/lib si no está ya.
# Usa la compilación probada de shinchiro y comprueba su SHA-256 antes de descomprimirla.
#   npm run libmpv

param([string]$Lib = (Join-Path $PSScriptRoot "..\src-tauri\lib"))

$ErrorActionPreference = "Stop"

$Url = "https://github.com/shinchiro/mpv-winbuild-cmake/releases/download/20261006/mpv-dev-x86_64-20261006-git-6c092d978b.7z"
$Sha256 = "bc0e016bdf6573bada824873d4746ab58d9bfac0ddd59c046f160312f81f9548"

$Dll = Join-Path $Lib "libmpv-2.dll"
if (Test-Path $Dll) {
    Write-Host "libmpv-2.dll ya está en $Lib."
    exit 0
}

# Carpeta temporal propia y vacía: lo descargado no se mezcla con nada.
$Tmp = Join-Path ([System.IO.Path]::GetTempPath()) ("kmovies-libmpv-" + [guid]::NewGuid())
New-Item -ItemType Directory -Path $Tmp | Out-Null
try {
    $Archive = Join-Path $Tmp "mpv-dev.7z"
    Write-Host "Descargando libmpv (unos 31 MB)..."
    $ProgressPreference = "SilentlyContinue"
    Invoke-WebRequest -Uri $Url -OutFile $Archive -UseBasicParsing

    $Hash = (Get-FileHash -Algorithm SHA256 $Archive).Hash.ToLowerInvariant()
    if ($Hash -ne $Sha256) { throw "El SHA-256 del archivo descargado no coincide ($Hash)." }

    # El tar de Windows 11 (libarchive) abre los .7z.
    & "$env:SystemRoot\System32\tar.exe" -xf $Archive -C $Tmp libmpv-2.dll
    if ($LASTEXITCODE -ne 0) { throw "No se ha podido descomprimir el archivo." }

    New-Item -ItemType Directory -Force -Path $Lib | Out-Null
    Move-Item (Join-Path $Tmp "libmpv-2.dll") $Dll
    Write-Host "Listo: $Dll"
}
finally {
    Remove-Item -Recurse -Force $Tmp -ErrorAction SilentlyContinue
}
