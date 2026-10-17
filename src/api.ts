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
  phase: "scanning" | "matching" | "ratings" | "done" | "error";
  current: number;
  total: number;
  message: string | null;
}

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
  listItems: () => invoke<Item[]>("list_items"),
  getItem: (id: number) => invoke<ItemDetail>("get_item", { id }),
  setWatchlist: (id: number, value: boolean) => invoke<void>("set_watchlist", { id, value }),
  setWatched: (id: number, value: boolean) => invoke<void>("set_watched", { id, value }),
  validateTmdbKey: (key: string) => invoke<boolean>("validate_tmdb_key", { key }),
  detectPlayers: () => invoke<Player[]>("detect_players"),
  playItem: (id: number) => invoke<void>("play_item", { id }),
  userLocales: () => invoke<Record<string, Record<string, unknown>>>("user_locales"),
};

export const onScanProgress = (cb: (p: ScanProgress) => void): Promise<UnlistenFn> =>
  listen<ScanProgress>("scan-progress", (e) => cb(e.payload));

export const onLibraryChanged = (cb: () => void): Promise<UnlistenFn> => listen("library-changed", () => cb());

/** Ruta local de la caché → URL que puede cargar el WebView. */
export const fileUrl = (path: string | null) => (path ? convertFileSrc(path) : null);

export const tmdbImage = (path: string | null, size = "w185") =>
  path ? `https://image.tmdb.org/t/p/${size}${path}` : null;

/** Las fechas de SQLite (`AAAA-MM-DD HH:MM:SS`) están en UTC. */
export const parseDbDate = (s: string | null) => (s ? new Date(s.replace(" ", "T") + (s.length > 10 ? "Z" : "")) : null);
