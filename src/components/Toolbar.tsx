import { ArrowDownWideNarrow, ArrowUpNarrowWide, Download, Plus, RefreshCw, Search, Settings, Shuffle, SlidersHorizontal, X } from "lucide-react";
import { motion } from "motion/react";
import { forwardRef } from "react";
import { useTranslation } from "react-i18next";
import { useShallow } from "zustand/react/shallow";
import { pickAndImport } from "../actions";
import { api } from "../api";
import { activeFilterCount, defaultDesc, SORT_KEYS, type SortKey, type View } from "../filters";
import { useViewCounts, useVisibleItems } from "../hooks";
import { GRID_MAX, GRID_MIN, useStore } from "../store";
import { heroTransition } from "../util";

const VIEWS: View[] = ["library", "pending", "watched"];

export const Toolbar = forwardRef<HTMLInputElement>(function Toolbar(_, searchRef) {
  const { t } = useTranslation();
  const s = useStore(
    useShallow((s) => ({
      view: s.view,
      query: s.query,
      sortKey: s.sortKey,
      sortDesc: s.sortDesc,
      gridSize: s.gridSize,
      filtersOpen: s.filtersOpen,
      filterCount: activeFilterCount(s.filters),
      scanning: s.scan != null,
      hasLibraries: s.libraries.length > 0,
    })),
  );
  const actions = useStore.getState();
  const counts = useViewCounts();
  const visible = useVisibleItems();

  const random = () => {
    if (!visible.length) return;
    const item = visible[Math.floor(Math.random() * visible.length)];
    const card = document.querySelector<HTMLElement>(`[data-poster-id="${item.id}"]`);
    heroTransition(card, () => actions.select(item.id));
  };

  return (
    <div className="toolbar">
      <nav className="tabs" role="tablist">
        {VIEWS.map((v) => (
          <button
            key={v}
            role="tab"
            aria-selected={s.view === v}
            className={s.view === v ? "tab active" : "tab"}
            onClick={() => actions.setView(v)}
          >
            {s.view === v && <motion.span layoutId="tab-pill" className="tab-pill" transition={{ type: "spring", stiffness: 500, damping: 38 }} />}
            <span className="tab-label">{t(`views.${v}`)}</span>
            <span className="tab-count">{counts[v]}</span>
          </button>
        ))}
      </nav>

      <label className="search">
        <Search size={16} />
        <input
          ref={searchRef}
          value={s.query}
          placeholder={t("toolbar.search")}
          onChange={(e) => actions.setQuery(e.target.value)}
          onKeyDown={(e) => {
            if (e.key === "Escape") actions.setQuery("");
            // Flecha abajo o Intro: al primer resultado, para seguir con el teclado.
            else if (e.key === "ArrowDown" || e.key === "Enter") {
              const first = document.querySelector<HTMLElement>('.grid-scroll [data-index="0"]');
              if (first) {
                e.preventDefault();
                first.focus();
              }
            }
          }}
          spellCheck={false}
        />
        {s.query && (
          <button className="search-clear" onClick={() => actions.setQuery("")} title={t("toolbar.clearSearch")}>
            <X size={14} />
          </button>
        )}
      </label>

      <div className="toolbar-spacer" />

      <div className="sort">
        <select
          value={s.sortKey}
          onChange={(e) => {
            const key = e.target.value as SortKey;
            actions.setSort(key, defaultDesc(key));
          }}
          title={t("toolbar.sortBy")}
        >
          {SORT_KEYS.map((k) => (
            <option key={k} value={k}>
              {t(`sort.${k}`)}
            </option>
          ))}
        </select>
        <button
          className="icon-btn"
          onClick={() => actions.setSort(s.sortKey, !s.sortDesc)}
          title={t(s.sortDesc ? "toolbar.descending" : "toolbar.ascending")}
        >
          {s.sortDesc ? <ArrowDownWideNarrow size={18} /> : <ArrowUpNarrowWide size={18} />}
        </button>
      </div>

      <label className="size-slider" title={t("toolbar.posterSize")}>
        <span className="size-dot small" />
        <input
          type="range"
          min={GRID_MIN}
          max={GRID_MAX}
          value={s.gridSize}
          onChange={(e) => actions.setGridSize(Number(e.target.value))}
          style={{ ["--fill" as string]: `${((s.gridSize - GRID_MIN) / (GRID_MAX - GRID_MIN)) * 100}%` }}
        />
        <span className="size-dot big" />
      </label>

      <div className="toolbar-group">
        {s.view === "library" ? (
          <button className="icon-btn" onClick={() => void pickAndImport()} disabled={!s.hasLibraries} title={t("toolbar.import")}>
            <Download size={18} />
          </button>
        ) : (
          <button
            className="icon-btn"
            onClick={() => actions.openDialog({ kind: "search", mode: s.view === "pending" ? "pending" : "watched", query: s.query })}
            title={t(`toolbar.add_${s.view}`)}
          >
            <Plus size={20} />
          </button>
        )}
        <button
          className={s.filtersOpen ? "icon-btn active" : "icon-btn"}
          onClick={actions.toggleFilters}
          title={t("toolbar.filters")}
        >
          <SlidersHorizontal size={18} />
          {s.filterCount > 0 && <span className="badge">{s.filterCount}</span>}
        </button>
        <button className="icon-btn" onClick={random} disabled={!visible.length} title={t("toolbar.random")}>
          <Shuffle size={18} />
        </button>
        <button
          className="icon-btn"
          onClick={() => void api.startScan()}
          disabled={s.scanning || !s.hasLibraries}
          title={t("toolbar.rescan")}
        >
          <RefreshCw size={18} className={s.scanning ? "spin" : undefined} />
        </button>
        <button className="icon-btn" onClick={() => actions.openSettings(true)} title={t("toolbar.settings")}>
          <Settings size={18} />
        </button>
      </div>
    </div>
  );
});
