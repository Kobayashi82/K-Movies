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
  /** Cuándo se guardó la posición (para ordenar "Seguir viendo"). */
  playbackAt: string | null;
}

/** Un visionado del historial. */
export interface HistoryEntry {
  id: number;
  itemId: number;
  /** `AAAA-MM-DD` o fecha y hora en UTC. */
  watchedAt: string;
  rating: number | null;
  rewatch: boolean;
  source: "manual" | "player" | "letterboxd" | "kmovies4";
}

/** Lo que se está reproduciendo ahora. */
export interface NowPlaying {
  itemId: number;
  title: string;
  player: string;
  /** Se sabe por dónde va (VLC); con otros reproductores solo se sabe que está abierto. */
  tracked: boolean;
  position: number | null;
  duration: number | null;
  paused: boolean;
  /** Ya se ha marcado como vista en esta sesión. */
  watched: boolean;
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
  kind:
    | "pendingAvailable"
    | "alreadyWatched"
    | "transferDone"
    | "transferError"
    | "markedWatched"
    | "askFinished"
    | "playError";
  title: string;
  itemId: number | null;
  detail: string | null;
}

/** Una pista del archivo tal como la describe mpv (`track-list`). */
export interface Track {
  id: number;
  type: "video" | "audio" | "sub";
  lang?: string;
  title?: string;
  codec?: string;
  selected?: boolean;
  default?: boolean;
  forced?: boolean;
  external?: boolean;
  "demux-channel-count"?: number;
  "external-filename"?: string;
}

/** Estado del reproductor integrado (evento `player-state`). */
export interface PlayerState {
  itemId: number;
  title: string;
  position: number | null;
  duration: number | null;
  paused: boolean;
  buffering: boolean;
  volume: number;
  mute: boolean;
  tracks: Track[] | null;
  audioDelay: number;
  subDelay: number;
  subScale: number;
  /** Distancia de los subtítulos al borde inferior, en píxeles de un vídeo de 720 de alto. */
  subMargin: number;
  subBackground: boolean;
  fullscreen: boolean;
  /** Siempre encima de las demás ventanas. */
  ontop: boolean;
  /** Alto de la ventana y bandas negras de arriba y abajo, en píxeles reales. */
  frame: { h: number; mt: number; mb: number };
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
  playItem: (id: number, opts: { fileId?: number; fromStart?: boolean } = {}) =>
    invoke<void>("play_item", { id, fileId: opts.fileId, fromStart: opts.fromStart }),
  nowPlaying: () => invoke<NowPlaying[]>("now_playing"),
  internalPlayerAvailable: () => invoke<boolean>("internal_player_available"),
  player: {
    state: () => invoke<PlayerState>("player_state"),
    command: (...args: (string | number)[]) => invoke<void>("player_command", { args: args.map(String) }),
    set: (name: string, value: string | number | boolean) =>
      invoke<void>("player_set", { name, value: typeof value === "boolean" ? (value ? "yes" : "no") : String(value) }),
    subBackground: (on: boolean) => invoke<void>("player_sub_background", { on }),
    /** Sube (+) o baja (−) los subtítulos esos puntos de %, para esta película. */
    subMargin: (percent: number) => invoke<void>("player_sub_margin", { percent }),
    fullscreen: (on: boolean) => invoke<void>("player_fullscreen", { on }),
    ontop: (on: boolean) => invoke<void>("player_ontop", { on }),
    /** Proporción forzada (`16:9`; vacío = la del vídeo). */
    aspect: (ratio: string) => invoke<void>("player_aspect", { ratio }),
    /** Recorta la imagen a esa proporción (ancho / alto; 0 = sin recorte). */
    crop: (ratio: number) => invoke<void>("player_crop", { ratio }),
    close: () => invoke<void>("player_close"),
  },
  listHistory: (itemId?: number) => invoke<HistoryEntry[]>("list_history", { itemId }),
  addHistory: (id: number, date?: string) => invoke<void>("add_history", { id, date }),
  setHistoryDate: (id: number, date: string) => invoke<void>("set_history_date", { id, date }),
  deleteHistory: (id: number) => invoke<void>("delete_history", { id }),
  clearProgress: (id: number) => invoke<void>("clear_progress", { id }),
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

export const onPlayback = (cb: (p: NowPlaying[]) => void): Promise<UnlistenFn> =>
  listen<NowPlaying[]>("playback", (e) => cb(e.payload));


export const onPlayerState = (cb: (p: PlayerState) => void): Promise<UnlistenFn> =>
  listen<PlayerState>("player-state", (e) => cb(e.payload));

export const onLibraryChanged = (cb: () => void): Promise<UnlistenFn> => listen("library-changed", () => cb());

/** Ruta local de la caché → URL que puede cargar el WebView. */
export const fileUrl = (path: string | null) => (path ? convertFileSrc(path) : null);

export const tmdbImage = (path: string | null, size = "w185") =>
  path ? `https://image.tmdb.org/t/p/${size}${path}` : null;

/** Imagen de TMDb servida desde la caché local (protocolo `kimg`; la descarga la primera vez). */
export const cachedTmdbImage = (path: string | null, size = "w185") => (path ? `http://kimg.localhost/${size}${path}` : null);

/** Las fechas y horas de SQLite (`AAAA-MM-DD HH:MM:SS`) están en UTC; las fechas sin hora son días locales. */
export const parseDbDate = (s: string | null) =>
  s ? new Date(s.length > 10 ? s.replace(" ", "T") + "Z" : `${s}T00:00`) : null;
