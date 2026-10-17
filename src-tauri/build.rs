use std::hash::{DefaultHasher, Hash, Hasher};
use std::path::{Path, PathBuf};
use std::{env, fs};

fn main() {
    // Al decir qué archivos vigilar, Cargo deja de repetir este script por cualquier cambio: el
    // icono del ejecutable (`icons/icon.ico`) y la configuración tienen que estar en la lista.
    println!("cargo:rerun-if-changed=icons");
    println!("cargo:rerun-if-changed=tauri.conf.json");
    embed_libmpv();
    tauri_build::build()
}

/// En la compilación final, `lib/libmpv-2.dll` va comprimida (zstd) dentro del ejecutable: así
/// K-Movies es un solo `.exe`. Al arrancar se descomprime una vez en la carpeta de datos (ver
/// `mpv.rs`). En desarrollo, o sin la DLL, va vacío y se usa la de `lib/` o ninguna.
fn embed_libmpv() {
    let out = PathBuf::from(env::var("OUT_DIR").unwrap()).join("libmpv-2.dll.zst");
    let dll = Path::new("lib/libmpv-2.dll");
    println!("cargo:rerun-if-changed=lib/libmpv-2.dll");
    let release = env::var("PROFILE").as_deref() == Ok("release");
    let mut name = String::new();
    if release && dll.is_file() {
        let bytes = fs::read(dll).expect("leer libmpv-2.dll");
        let mut hasher = DefaultHasher::new();
        bytes.hash(&mut hasher);
        // El nombre lleva la huella: otra versión de la DLL se descomprime aparte.
        name = format!("libmpv-2-{:016x}.dll", hasher.finish());
        let mut encoder = zstd::Encoder::new(Vec::new(), 19).expect("zstd");
        let threads = std::thread::available_parallelism().map_or(1, |n| n.get() as u32);
        encoder.multithread(threads).expect("zstd multihilo");
        encoder.long_distance_matching(true).expect("zstd");
        encoder.window_log(27).expect("zstd");
        std::io::copy(&mut bytes.as_slice(), &mut encoder).expect("comprimir libmpv");
        fs::write(&out, encoder.finish().expect("zstd")).expect("escribir libmpv comprimida");
    } else {
        fs::write(&out, []).expect("escribir vacío");
    }
    println!("cargo:rustc-env=KMOVIES_LIBMPV_NAME={name}");
}
