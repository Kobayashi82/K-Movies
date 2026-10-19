// `npm run tauri …`: lanza la CLI de Tauri y, después de un `build` que sale bien, copia el
// ejecutable portable (K-Movies.exe, con todo dentro) a la carpeta `bin` del proyecto.
import { spawnSync } from "node:child_process";
import { copyFileSync, existsSync, mkdirSync } from "node:fs";
import { createRequire } from "node:module";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";

const root = join(dirname(fileURLToPath(import.meta.url)), "..");
const cli = createRequire(import.meta.url).resolve("@tauri-apps/cli/tauri.js");
const args = process.argv.slice(2);

const { status } = spawnSync(process.execPath, [cli, ...args], { stdio: "inherit", cwd: root });
if (status !== 0) process.exit(status ?? 1);

if (args[0] === "build") {
  // `--debug` deja el ejecutable en `debug` en vez de `release`.
  const profile = args.includes("--debug") || args.includes("-d") ? "debug" : "release";
  const exe = join(root, "src-tauri", "target", profile, "K-Movies.exe");
  if (existsSync(exe)) {
    const bin = join(root, "bin");
    mkdirSync(bin, { recursive: true });
    copyFileSync(exe, join(bin, "K-Movies.exe"));
    console.log(`\nCopiado a ${join(bin, "K-Movies.exe")}`);
  }
}
