import { open } from "@tauri-apps/plugin-dialog";
import { revealItemInDir } from "@tauri-apps/plugin-opener";
import { BookmarkMinus, BookmarkPlus, Check, EyeOff, FolderOpen, Info, Play, RefreshCw, RotateCcw, SearchCheck, Trash2 } from "lucide-react";
import { api, type Conflict, type Item } from "./api";
import i18n from "./i18n";
import { useStore, type Choice, type MenuEntry } from "./store";
import { formatClock, heroTransition } from "./util";

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

/** Reproduce (por defecto, continuando donde se quedó). Los errores salen como aviso. */
export async function play(id: number, opts: { fileId?: number; fromStart?: boolean } = {}) {
  try {
    await api.playItem(id, opts);
  } catch (e) {
    useStore.getState().pushNotice({ kind: "playError", title: "", itemId: id, detail: String(e) });
  }
}

/** Con reproductores que no dicen por dónde van: al cerrarlo, se pregunta si se terminó. */
export async function askFinished(id: number, title: string) {
  const answer = await askChoice({
    title: i18n.t("playback.askTitle"),
    message: i18n.t("playback.askMessage", { title }),
    options: [
      { value: "no", label: i18n.t("playback.askNo") },
      { value: "yes", label: i18n.t("playback.askYes"), kind: "primary" },
    ],
  });
  if (answer === "yes") await api.setWatched(id, true);
}

/** Quitar de Vistas borra todo su historial: si hay más de un visionado, se pregunta antes. */
export async function unmarkWatched(id: number, count: number) {
  if (count > 1) {
    const answer = await askChoice({
      title: i18n.t("history.unmarkTitle"),
      message: i18n.t("history.unmarkMessage", { count }),
      options: [
        { value: "cancel", label: i18n.t("common.cancel") },
        { value: "ok", label: i18n.t("history.unmark"), kind: "danger" },
      ],
    });
    if (answer !== "ok") return;
  }
  await api.setWatched(id, false);
}

/** Texto legible para los códigos de error del backend. */
export function errorText(code: string | null): string {
  const key = `errors.${code}`;
  return code && i18n.exists(key) ? i18n.t(key) : (code ?? "");
}

/** Borra la película: todos sus archivos y sus subtítulos externos van a la papelera. */
export async function deleteMovie(item: Item): Promise<boolean> {
  const answer = await askChoice({
    title: i18n.t("detail.deleteMovie"),
    message: i18n.t("detail.deleteMovieConfirm", { title: item.title, count: item.fileCount }),
    options: [
      { value: "cancel", label: i18n.t("common.cancel") },
      { value: "delete", label: i18n.t("detail.deleteMovie"), kind: "danger" },
    ],
  });
  if (answer !== "delete") return false;
  await api.deleteItemFiles(item.id);
  return true;
}

/** Abre la ficha (con la transición del póster si está a la vista). */
export function openDetail(id: number) {
  const card = document.querySelector<HTMLElement>(`[data-poster-id="${id}"]`);
  heroTransition(card, () => useStore.getState().select(id));
}

/** Abre la carpeta del archivo más grande de la película. */
async function showInFolder(id: number) {
  const file = (await api.getItem(id)).files[0];
  if (file) await revealItemInDir(file.path);
}

/** Lanza una acción y, si falla, lo dice en un aviso. */
const guarded = (id: number, action: () => Promise<unknown>) => () =>
  action().catch((e) => useStore.getState().pushNotice({ kind: "actionError", title: "", itemId: id, detail: String(e) }));

/** Menú del botón derecho sobre una película (póster, Seguir viendo, historial). */
export function movieMenu(item: Item): MenuEntry[] {
  const { playing, openDialog, select } = useStore.getState();
  const live = playing.find((p) => p.itemId === item.id);
  const resumable = !live && item.position != null && item.duration != null;
  const onDisk = item.fileCount > 0;
  const t = i18n.t.bind(i18n);
  const entries: (MenuEntry | false)[] = [
    onDisk && {
      label: live ? t("menu.goToPlayer") : resumable ? t("detail.resumeAt", { time: formatClock(item.position) }) : t("detail.play"),
      icon: Play,
      action: () => (live?.window ? api.focusPlayer(item.id) : play(item.id)),
    },
    onDisk && resumable && { label: t("detail.fromStart"), icon: RotateCcw, action: () => play(item.id, { fromStart: true }) },
    { label: t("menu.open"), icon: Info, action: () => select(item.id) },
    null,
    item.watchCount > 0
      ? { label: t("history.unmark"), icon: EyeOff, action: guarded(item.id, () => unmarkWatched(item.id, item.watchCount)) }
      : { label: t("detail.markWatched"), icon: Check, action: guarded(item.id, () => api.setWatched(item.id, true)) },
    item.watchlist
      ? { label: t("menu.removePending"), icon: BookmarkMinus, action: guarded(item.id, () => api.setWatchlist(item.id, false)) }
      : { label: t("detail.addPending"), icon: BookmarkPlus, action: guarded(item.id, () => api.setWatchlist(item.id, true)) },
    null,
    onDisk && { label: t("detail.showInFolder"), icon: FolderOpen, action: guarded(item.id, () => showInFolder(item.id)) },
    {
      label: t("menu.fixMatch"),
      icon: SearchCheck,
      action: () => openDialog({ kind: "search", mode: "identify", itemId: item.id, query: item.title, year: item.matched === 0 ? item.year : null }),
    },
    item.tmdbId != null && { label: t("detail.refresh"), icon: RefreshCw, action: guarded(item.id, () => api.refreshItem(item.id)) },
    onDisk && null,
    onDisk && { label: t("detail.deleteMovie"), icon: Trash2, danger: true, action: guarded(item.id, () => deleteMovie(item)) },
  ];
  return entries.filter((e): e is MenuEntry => e !== false);
}

/** Abre el menú propio de la app donde se ha pulsado el botón derecho. */
export function openMenu(e: React.MouseEvent, entries: MenuEntry[]) {
  e.preventDefault();
  e.stopPropagation();
  useStore.setState({ menu: { x: e.clientX, y: e.clientY, entries } });
}

/** Textos del menú del icono de notificación (en el idioma de la interfaz); también lo enseña o lo quita. */
export function syncTray() {
  void api.trayUpdate(i18n.t("tray.show"), i18n.t("tray.quit")).catch(console.error);
}

let rescanTimer = 0;

/** Escanea en cuanto se pueda (si hay un escaneo o una importación en marcha, lo reintenta). Así se
 *  actualizan las fichas tras cambiar el idioma de la información. */
export function rescanWhenFree(tries = 60) {
  window.clearTimeout(rescanTimer);
  if (!useStore.getState().libraries.length) return;
  void api.startScan().then((started) => {
    if (!started && tries > 1) rescanTimer = window.setTimeout(() => rescanWhenFree(tries - 1), 5000);
  });
}
