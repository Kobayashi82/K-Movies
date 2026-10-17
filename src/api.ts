import { convertFileSrc, invoke } from "@tauri-apps/api/core";
import { listen, type UnlistenFn } from "@tauri-apps/api/event";

export interface Item {
  id: number;
  kind: "movie" | "series";
  tmdbId: number | null;
  imdbId: string | null;
  title: string;
  originalTitle: string | null;
  year: number | null;
  releaseDate: string | null;
  runtime: number | null;
  certification: string | null;
  genres: string[];
  countries: string[];
  languages: string[];
  imdbRating: number | null;
  imdbVotes: number | null;
  tmdbRating: number | null;
  poster: string | null;
  backdrop: string | null;
  matched: number;
  watchlist: boolean;
  addedAt: string;
  fileCount: number;
  totalSize: number;
  hasSubs: boolean;
  directors: string[];
  cast: string[];
  watchCount: number;
  lastWatched: string | null;
  position: number | null;
  duration: number | null;
}

export interface Person {
  id: number;
  tmdbId: number | null;
  name: string;
  role: "director" | "cast";
  character: string | null;
  profileUrl: string | null;
}

export interface ItemDetail {
  overview: string | null;
  tagline: string | null;
  originalLanguage: string | null;
  tmdbVotes: number | null;
  trailerKey: string | null;
  files: { id: number; path: string; size: number; hasSubs: boolean; addedAt: string }[];
  people: Person[];
}

export interface Library {
  id: number;
  path: string;
  kind: string;
  lastScan: string | null;
  fileCount: number;
}

export interface Player {
  kind: string;
  name: string;
  path: string;
}

export interface ScanProgress {
  phase: "scanning" | "matching" | "refreshing" | "ratings" | "done" | "error";
  current: number;
  total: number;
  message: string | null;
}

export interface Candidate {
  id: number;
  title: string;
  originalTitle: string;
  year: number | null;
  popularity: number;
  posterPath: string | null;
  overview: string;
}

export interface TransferInfo {
  id: number;
  kind: "import" | "copy" | "move";
  name: string;
  source: string;
  dest: string;
  total: number;
  done: number;
  state: "queued" | "running" | "done" | "error" | "cancelled";
  error: string | null;
}

export interface Notice {
  kind: "pendingAvailable" | "alreadyWatched" | "transferDone" | "transferError";
  title: string;
  itemId: number | null;
  detail: string | null;
}

/** Qué hacer si el archivo ya existe en el destino. */
export type Conflict = "ask" | "rename" | "replace";

export interface AppInfo {
  version: string;
  dataDir: string;
  scanning: boolean;
}

export const api = {
  appInfo: () => invoke<AppInfo>("app_info"),
  getSettings: () => invoke<Record<string, string>>("get_settings"),
  setSetting: (key: string, value: string) => invoke<void>("set_setting", { key, value }),
  listLibraries: () => invoke<Library[]>("list_libraries"),
  addLibrary: (path: string) => invoke<void>("add_library", { path }),
  removeLibrary: (id: number) => invoke<void>("remove_library", { id }),
  startScan: () => invoke<boolean>("start_scan"),
  refreshAll: () => invoke<boolean>("refresh_all"),
  listItems: () => invoke<Item[]>("list_items"),
  getItem: (id: number) => invoke<ItemDetail>("get_item", { id }),
  setWatchlist: (id: number, value: boolean) => invoke<void>("set_watchlist", { id, value }),
  setWatched: (id: number, value: boolean) => invoke<void>("set_watched", { id, value }),
  validateTmdbKey: (key: string) => invoke<boolean>("validate_tmdb_key", { key }),
  detectPlayers: () => invoke<Player[]>("detect_players"),
  playItem: (id: number) => invoke<void>("play_item", { id }),
  searchTmdb: (query: string, year: number | null) => invoke<Candidate[]>("search_tmdb", { query, year }),
  identifyItem: (id: number, tmdbId: number) => invoke<number>("identify_item", { id, tmdbId }),
  addItem: (tmdbId: number, list: "pending" | "watched") => invoke<number>("add_item", { tmdbId, list }),
  refreshItem: (id: number) => invoke<void>("refresh_item", { id }),
  identifyFile: (fileId: number, tmdbId: number) => invoke<number>("identify_file", { fileId, tmdbId }),
  importFiles: (paths: string[], libraryId: number, conflict?: Conflict) =>
    invoke<void>("import_files", { paths, libraryId, conflict }),
  transferFile: (fileId: number, destDir: string, mode: "copy" | "move", conflict?: Conflict) =>
    invoke<void>("transfer_file", { fileId, destDir, mode, conflict }),
  listTransfers: () => invoke<TransferInfo[]>("list_transfers"),
  cancelTransfer: (id: number) => invoke<void>("cancel_transfer", { id }),
  dismissTransfers: (id: number | null) => invoke<void>("dismiss_transfers", { id }),
  deleteFile: (fileId: number) => invoke<void>("delete_file", { fileId }),
  userLocales: () => invoke<Record<string, Record<string, unknown>>>("user_locales"),
};

export const onScanProgress = (cb: (p: ScanProgress) => void): Promise<UnlistenFn> =>
  listen<ScanProgress>("scan-progress", (e) => cb(e.payload));

export const onTransfers = (cb: (t: TransferInfo[]) => void): Promise<UnlistenFn> =>
  listen<TransferInfo[]>("transfers", (e) => cb(e.payload));

export const onNotice = (cb: (n: Notice) => void): Promise<UnlistenFn> => listen<Notice>("notice", (e) => cb(e.payload));

export const onLibraryChanged = (cb: () => void): Promise<UnlistenFn> => listen("library-changed", () => cb());

/** Ruta local de la caché → URL que puede cargar el WebView. */
export const fileUrl = (path: string | null) => (path ? convertFileSrc(path) : null);

export const tmdbImage = (path: string | null, size = "w185") =>
  path ? `https://image.tmdb.org/t/p/${size}${path}` : null;

/** Imagen de TMDb servida desde la caché local (protocolo `kimg`; la descarga la primera vez). */
export const cachedTmdbImage = (path: string | null, size = "w185") => (path ? `http://kimg.localhost/${size}${path}` : null);

/** Las fechas de SQLite (`AAAA-MM-DD HH:MM:SS`) están en UTC. */
export const parseDbDate = (s: string | null) => (s ? new Date(s.replace(" ", "T") + (s.length > 10 ? "Z" : "")) : null);
