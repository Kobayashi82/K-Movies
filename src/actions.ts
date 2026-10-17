import { open } from "@tauri-apps/plugin-dialog";
import { api, type Conflict } from "./api";
import i18n from "./i18n";
import { useStore, type Choice } from "./store";

const VIDEO_EXT = ["mp4", "mkv", "avi", "m4v", "mov", "wmv", "mpg", "mpeg", "ts", "m2ts", "webm", "flv", "ogm", "divx"];

const isVideo = (path: string) => VIDEO_EXT.includes(path.split(".").pop()?.toLowerCase() ?? "");
const fileName = (path: string) => path.split(/[\\/]/).pop() ?? path;

/** Pregunta con el diálogo propio de la app. Devuelve el valor elegido o `null` si se cancela. */
export function askChoice(choice: Omit<Choice, "resolve">): Promise<string | null> {
  return new Promise((resolve) => useStore.setState({ choice: { ...choice, resolve } }));
}

/** Si ya hay un archivo con ese nombre: renombrar, reemplazar o cancelar. */
async function askConflict(path: string): Promise<Conflict | null> {
  const name = fileName(path);
  const dot = name.lastIndexOf(".");
  const renamed = dot > 0 ? `${name.slice(0, dot)} (2)${name.slice(dot)}` : `${name} (2)`;
  const answer = await askChoice({
    title: i18n.t("conflict.title"),
    message: i18n.t("conflict.message", { name, renamed }),
    options: [
      { value: "cancel", label: i18n.t("common.cancel") },
      { value: "replace", label: i18n.t("conflict.replace"), kind: "danger" },
      { value: "rename", label: i18n.t("conflict.rename"), kind: "primary" },
    ],
  });
  return answer === "rename" || answer === "replace" ? answer : null;
}

/** Lanza una transferencia; si el nombre ya existe, pregunta y reintenta con la respuesta. */
async function withConflict(path: string, start: (conflict?: Conflict) => Promise<void>) {
  try {
    await start();
  } catch (e) {
    if (String(e) !== "exists") throw e;
    const conflict = await askConflict(path);
    if (conflict) await start(conflict);
  }
}

function reportError(e: unknown) {
  useStore.getState().pushNotice({ kind: "transferError", title: "", itemId: null, detail: String(e) });
}

/** Importa vídeos a la biblioteca. Con varias carpetas, pregunta a cuál. */
export async function importPaths(paths: string[], libraryId?: number) {
  const videos = paths.filter(isVideo);
  if (!videos.length) return;
  const { libraries, openDialog, openSettings } = useStore.getState();
  if (!libraries.length) return openSettings(true);
  if (libraryId == null && libraries.length > 1) return openDialog({ kind: "import", paths: videos });
  const target = libraryId ?? libraries[0].id;
  // De uno en uno, para poder preguntar por cada nombre repetido.
  for (const path of videos) {
    try {
      await withConflict(path, (conflict) => api.importFiles([path], target, conflict));
    } catch (e) {
      reportError(e);
    }
  }
}

export async function pickAndImport() {
  const files = await open({
    multiple: true,
    title: i18n.t("import.title"),
    filters: [{ name: i18n.t("import.videos"), extensions: VIDEO_EXT }],
  });
  if (Array.isArray(files)) await importPaths(files);
  else if (typeof files === "string") await importPaths([files]);
}

export async function transferFile(fileId: number, path: string, mode: "copy" | "move") {
  const dir = await open({ directory: true, title: i18n.t(mode === "copy" ? "detail.copyTo" : "detail.moveTo") });
  if (typeof dir !== "string") return;
  try {
    await withConflict(path, (conflict) => api.transferFile(fileId, dir, mode, conflict));
  } catch (e) {
    reportError(e);
  }
}

export async function deleteFile(fileId: number, path: string) {
  const answer = await askChoice({
    title: i18n.t("detail.delete"),
    message: i18n.t("detail.deleteConfirm", { name: fileName(path) }),
    options: [
      { value: "cancel", label: i18n.t("common.cancel") },
      { value: "delete", label: i18n.t("detail.delete"), kind: "danger" },
    ],
  });
  if (answer === "delete") await api.deleteFile(fileId);
}

/** Texto legible para los códigos de error del backend. */
export function errorText(code: string | null): string {
  const key = `errors.${code}`;
  return code && i18n.exists(key) ? i18n.t(key) : (code ?? "");
}
