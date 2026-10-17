import { getCurrentWebview } from "@tauri-apps/api/webview";
import { getCurrentWindow } from "@tauri-apps/api/window";
import { Download, Plus, SearchX } from "lucide-react";
import { AnimatePresence, motion } from "motion/react";
import { useEffect, useRef, useState } from "react";
import { useTranslation } from "react-i18next";
import { importPaths } from "./actions";
import { api, onLibraryChanged, onNotice, onScanProgress, onTransfers } from "./api";
import { ActiveFilters } from "./components/ActiveFilters";
import { Backdrop } from "./components/Backdrop";
import { ChoiceDialog } from "./components/ChoiceDialog";
import { Detail } from "./components/Detail";
import { FilterPanel } from "./components/FilterPanel";
import { Grid } from "./components/Grid";
import { ImportDialog } from "./components/ImportDialog";
import { SearchDialog } from "./components/SearchDialog";
import { Settings } from "./components/Settings";
import { TitleBar } from "./components/TitleBar";
import { Toasts } from "./components/Toasts";
import { Toolbar } from "./components/Toolbar";
import { Trailer } from "./components/Trailer";
import { Welcome } from "./components/Welcome";
import { useVisibleItems } from "./hooks";
import { useStore } from "./store";
import { useTheme } from "./theme";

export default function App() {
  const { t } = useTranslation();
  const searchRef = useRef<HTMLInputElement>(null);
  const [dragging, setDragging] = useState(false);
  const view = useStore((s) => s.view);
  const filtersOpen = useStore((s) => s.filtersOpen);
  const settingsOpen = useStore((s) => s.settingsOpen);
  const dialog = useStore((s) => s.dialog);
  const trailer = useStore((s) => s.trailer);
  const choice = useStore((s) => s.choice);
  const hasLibraries = useStore((s) => s.libraries.length > 0);
  const theme = useStore((s) => s.settings.theme);
  const query = useStore((s) => s.query);
  const selected = useStore((s) => (s.selectedId == null ? null : (s.items.find((i) => i.id === s.selectedId) ?? null)));
  const visible = useVisibleItems();
  useTheme(theme);

  useEffect(() => {
    const { refreshItems, refreshLibraries, setScan, setTransfers, pushNotice } = useStore.getState();
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
      onTransfers(setTransfers),
      onNotice(pushNotice),
      // Arrastrar vídeos a la ventana los importa, como el botón Importar.
      getCurrentWebview().onDragDropEvent((e) => {
        if (e.payload.type === "enter" || e.payload.type === "over") setDragging(true);
        else if (e.payload.type === "leave") setDragging(false);
        else if (e.payload.type === "drop") {
          setDragging(false);
          void importPaths(e.payload.paths);
        }
      }),
    ];
    void api.listTransfers().then(setTransfers);
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
  const addToList = () =>
    useStore.getState().openDialog({ kind: "search", mode: view === "watched" ? "watched" : "pending", query });

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
                  {view !== "library" && (
                    <button className="btn primary" onClick={addToList}>
                      <Plus size={18} />
                      {t(`toolbar.add_${view}`)}
                    </button>
                  )}
                </div>
              )}
            </>
          )}
        </main>
      </div>
      {selected && <Detail item={selected} />}
      <AnimatePresence>
        {settingsOpen && <Settings key="settings" />}
        {dialog?.kind === "search" && <SearchDialog key="search" {...dialog} />}
        {dialog?.kind === "import" && <ImportDialog key="import" paths={dialog.paths} />}
        {trailer && <Trailer key="trailer" videoKey={trailer} />}
        {choice && <ChoiceDialog key="choice" choice={choice} />}
      </AnimatePresence>
      <AnimatePresence>
        {dragging && hasLibraries && (
          <motion.div className="drop-overlay" initial={{ opacity: 0 }} animate={{ opacity: 1 }} exit={{ opacity: 0 }}>
            <div className="drop-box">
              <Download size={36} />
              <strong>{t("import.drop")}</strong>
            </div>
          </motion.div>
        )}
      </AnimatePresence>
      <Toasts />
    </div>
  );
}
