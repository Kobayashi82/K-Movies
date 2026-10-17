import { getCurrentWindow } from "@tauri-apps/api/window";
import { SearchX } from "lucide-react";
import { AnimatePresence } from "motion/react";
import { useEffect, useRef } from "react";
import { useTranslation } from "react-i18next";
import { api, onLibraryChanged, onScanProgress } from "./api";
import { ActiveFilters } from "./components/ActiveFilters";
import { Backdrop } from "./components/Backdrop";
import { Detail } from "./components/Detail";
import { FilterPanel } from "./components/FilterPanel";
import { Grid } from "./components/Grid";
import { ScanStatus } from "./components/ScanStatus";
import { Settings } from "./components/Settings";
import { TitleBar } from "./components/TitleBar";
import { Toolbar } from "./components/Toolbar";
import { Welcome } from "./components/Welcome";
import { useVisibleItems } from "./hooks";
import { useStore } from "./store";
import { useTheme } from "./theme";

export default function App() {
  const { t } = useTranslation();
  const searchRef = useRef<HTMLInputElement>(null);
  const view = useStore((s) => s.view);
  const filtersOpen = useStore((s) => s.filtersOpen);
  const settingsOpen = useStore((s) => s.settingsOpen);
  const hasLibraries = useStore((s) => s.libraries.length > 0);
  const theme = useStore((s) => s.settings.theme);
  const selected = useStore((s) => (s.selectedId == null ? null : (s.items.find((i) => i.id === s.selectedId) ?? null)));
  const visible = useVisibleItems();
  useTheme(theme);

  useEffect(() => {
    const { refreshItems, refreshLibraries, setScan } = useStore.getState();
    let timer = 0;
    const subs = [
      onScanProgress((p) => {
        setScan(p);
        if (p.phase === "done") void refreshLibraries();
      }),
      onLibraryChanged(() => {
        window.clearTimeout(timer);
        timer = window.setTimeout(() => void refreshItems(), 150);
      }),
    ];
    void getCurrentWindow().show();
    // Al abrir, se reescanea en segundo plano para recoger archivos nuevos o borrados.
    if (useStore.getState().libraries.length) void api.startScan();

    const onKey = (e: KeyboardEvent) => {
      const typing = e.target instanceof HTMLInputElement || e.target instanceof HTMLTextAreaElement;
      if ((e.ctrlKey && e.key.toLowerCase() === "f") || (e.key === "/" && !typing)) {
        e.preventDefault();
        searchRef.current?.focus();
        searchRef.current?.select();
      }
    };
    window.addEventListener("keydown", onKey);
    return () => {
      subs.forEach((s) => void s.then((f) => f()));
      window.removeEventListener("keydown", onKey);
    };
  }, []);

  const showWelcome = view === "library" && !hasLibraries;

  return (
    <div className="app">
      <Backdrop />
      <TitleBar />
      <Toolbar ref={searchRef} />
      <div className="body">
        <AnimatePresence initial={false}>{filtersOpen && !showWelcome && <FilterPanel key="filters" />}</AnimatePresence>
        <main className="content">
          {showWelcome ? (
            <Welcome />
          ) : (
            <>
              <ActiveFilters count={visible.length} />
              {visible.length > 0 ? (
                <Grid items={visible} />
              ) : (
                <div className="empty">
                  <SearchX size={40} strokeWidth={1.5} />
                  <p>{t(`grid.empty_${view}`)}</p>
                </div>
              )}
            </>
          )}
        </main>
      </div>
      {selected && <Detail item={selected} />}
      <AnimatePresence>{settingsOpen && <Settings key="settings" />}</AnimatePresence>
      <ScanStatus />
    </div>
  );
}
