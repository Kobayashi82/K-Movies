import { create } from "zustand";
import { api, type AppInfo, type Item, type Library, type Notice, type ScanProgress, type TransferInfo } from "./api";
import { emptyFilters, type Filters, type SortKey, type View, SORT_KEYS } from "./filters";

export type ThemeSetting = "system" | "dark" | "light";

/** Diálogos que se abren encima de todo. */
export type Dialog =
  | {
      kind: "search";
      mode: "identify" | "pending" | "watched";
      itemId?: number;
      /** Identificar solo este archivo (no toda la ficha). */
      fileId?: number;
      query: string;
      year?: number | null;
    }
  | { kind: "import"; paths: string[] };

export type ToastNotice = Notice & { id: number };

/** Pregunta con el estilo de la app (sustituye a los diálogos nativos de Windows). */
export interface Choice {
  title: string;
  message: string;
  options: { value: string; label: string; kind?: "primary" | "danger" }[];
  resolve: (value: string | null) => void;
}

interface State {
  ready: boolean;
  info: AppInfo | null;
  items: Item[];
  libraries: Library[];
  settings: Record<string, string>;

  view: View;
  query: string;
  sortKey: SortKey;
  sortDesc: boolean;
  filters: Filters;
  gridSize: number;
  filtersOpen: boolean;

  hoveredId: number | null;
  selectedId: number | null;
  settingsOpen: boolean;
  scan: ScanProgress | null;
  scanError: string | null;
  dialog: Dialog | null;
  trailer: string | null;
  choice: Choice | null;
  transfers: TransferInfo[];
  notices: ToastNotice[];

  init: () => Promise<void>;
  refreshItems: () => Promise<void>;
  refreshLibraries: () => Promise<void>;
  setSetting: (key: string, value: string) => void;
  setView: (view: View) => void;
  setQuery: (query: string) => void;
  setSort: (key: SortKey, desc: boolean) => void;
  setFilters: (update: (f: Filters) => Filters) => void;
  clearFilters: () => void;
  setGridSize: (size: number) => void;
  toggleFilters: () => void;
  hover: (id: number | null) => void;
  select: (id: number | null) => void;
  openSettings: (open: boolean) => void;
  setScan: (p: ScanProgress | null) => void;
  openDialog: (dialog: Dialog | null) => void;
  setTrailer: (key: string | null) => void;
  setTransfers: (t: TransferInfo[]) => void;
  pushNotice: (n: Notice) => void;
  dropNotice: (id: number) => void;
}

let noticeId = 0;

export const GRID_MIN = 110;
export const GRID_MAX = 340;

const timers: Record<string, number> = {};

/** Guarda un ajuste en la base de datos, agrupando cambios rápidos (p. ej. el deslizador). */
function persist(key: string, value: string, delay = 0) {
  window.clearTimeout(timers[key]);
  timers[key] = window.setTimeout(() => void api.setSetting(key, value), delay);
}

export const useStore = create<State>((set, get) => ({
  ready: false,
  info: null,
  items: [],
  libraries: [],
  settings: {},

  view: "library",
  query: "",
  sortKey: "added",
  sortDesc: true,
  filters: emptyFilters(),
  gridSize: 180,
  filtersOpen: false,

  hoveredId: null,
  selectedId: null,
  settingsOpen: false,
  scan: null,
  scanError: null,
  dialog: null,
  trailer: null,
  choice: null,
  transfers: [],
  notices: [],

  init: async () => {
    const [info, settings, items, libraries] = await Promise.all([
      api.appInfo(),
      api.getSettings(),
      api.listItems(),
      api.listLibraries(),
    ]);
    const sortKey = SORT_KEYS.includes(settings.sortKey as SortKey) ? (settings.sortKey as SortKey) : "added";
    const view = (["library", "pending", "watched"] as View[]).includes(settings.view as View)
      ? (settings.view as View)
      : "library";
    set({
      ready: true,
      info,
      settings,
      items,
      libraries,
      view,
      sortKey,
      sortDesc: settings.sortDesc ? settings.sortDesc === "true" : true,
      gridSize: Number(settings.gridSize) || 180,
      filtersOpen: settings.filtersOpen === "true",
    });
  },

  refreshItems: async () => set({ items: await api.listItems() }),
  refreshLibraries: async () => set({ libraries: await api.listLibraries() }),

  setSetting: (key, value) => {
    set({ settings: { ...get().settings, [key]: value } });
    persist(key, value);
  },
  setView: (view) => {
    set({ view, selectedId: null });
    persist("view", view);
  },
  setQuery: (query) => set({ query }),
  setSort: (sortKey, sortDesc) => {
    set({ sortKey, sortDesc });
    persist("sortKey", sortKey);
    persist("sortDesc", String(sortDesc));
  },
  setFilters: (update) => set({ filters: update(get().filters) }),
  clearFilters: () => set({ filters: emptyFilters(), query: "" }),
  setGridSize: (gridSize) => {
    set({ gridSize });
    persist("gridSize", String(gridSize), 400);
  },
  toggleFilters: () => {
    const filtersOpen = !get().filtersOpen;
    set({ filtersOpen });
    persist("filtersOpen", String(filtersOpen));
  },
  hover: (hoveredId) => set({ hoveredId }),
  select: (selectedId) => set({ selectedId }),
  openSettings: (settingsOpen) => set({ settingsOpen }),
  setScan: (p) => {
    if (!p || p.phase === "done") return set({ scan: null });
    if (p.phase === "error") return set({ scan: null, scanError: p.message });
    // Un escaneo nuevo borra el error del anterior.
    const fresh = p.phase === "scanning" && p.current === 0;
    set({ scan: p, scanError: fresh ? null : get().scanError });
  },
  openDialog: (dialog) => set({ dialog }),
  setTrailer: (trailer) => set({ trailer }),
  setTransfers: (transfers) => set({ transfers }),
  pushNotice: (n) => {
    const id = ++noticeId;
    set({ notices: [...get().notices.slice(-3), { ...n, id }] });
    window.setTimeout(() => get().dropNotice(id), 8000);
  },
  dropNotice: (id) => set({ notices: get().notices.filter((n) => n.id !== id) }),
}));
