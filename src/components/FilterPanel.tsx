import { motion } from "motion/react";
import { useMemo, useState } from "react";
import { useTranslation } from "react-i18next";
import { facetOptions, fold, rangeBounds, type Filters, type ListFacet, type RangeFacet } from "../filters";
import { useViewItems } from "../hooks";
import { useStore } from "../store";
import { displayName, formatNumber, formatRuntime } from "../util";
import { RangeSlider } from "./RangeSlider";

export function labelFor(facet: ListFacet, value: string): string {
  if (facet === "countries") return displayName(value, "region");
  if (facet === "languages") return displayName(value, "language");
  return value;
}

/** Cómo se escribe cada valor de los filtros de rango (en el deslizador y en las etiquetas). */
export function rangeFormat(facet: RangeFacet): (v: number) => string {
  switch (facet) {
    case "imdb":
      return (v) => formatNumber(v);
    case "runtime":
      return formatRuntime;
    case "myRating":
      return (v) => `★ ${formatNumber(v, v % 1 ? 1 : 0)}`;
    default:
      return String;
  }
}

function Segmented<T extends string>({ value, options, onChange }: { value: T; options: { value: T; label: string }[]; onChange: (v: T) => void }) {
  return (
    <div className="segmented">
      {options.map((o) => (
        <button key={o.value} className={o.value === value ? "active" : undefined} onClick={() => onChange(o.value)}>
          {o.label}
        </button>
      ))}
    </div>
  );
}

function FacetList({ facet, searchable }: { facet: ListFacet; searchable?: boolean }) {
  const { t } = useTranslation();
  const viewItems = useViewItems();
  const selected = useStore((s) => s.filters[facet]);
  const setFilters = useStore((s) => s.setFilters);
  const [expanded, setExpanded] = useState(false);
  const [search, setSearch] = useState("");
  const options = useMemo(() => facetOptions(viewItems, facet), [viewItems, facet]);
  if (!options.length) return null;

  const q = fold(search.trim());
  const filtered = q ? options.filter((o) => fold(labelFor(facet, o.value)).includes(q)) : options;
  // Los seleccionados siempre visibles, aunque no estén entre los primeros.
  const limit = expanded || q ? filtered.length : 10;
  const shown = [...filtered.slice(0, limit), ...filtered.slice(limit).filter((o) => selected.includes(o.value))];
  const toggle = (value: string) =>
    setFilters((f: Filters) => ({
      ...f,
      [facet]: f[facet].includes(value) ? f[facet].filter((v) => v !== value) : [...f[facet], value],
    }));

  return (
    <section className="filter-section">
      <h4>{t(`filters.${facet}`)}</h4>
      {searchable && options.length > 10 && (
        <input className="facet-search" value={search} onChange={(e) => setSearch(e.target.value)} placeholder={t("filters.searchIn")} />
      )}
      <div className={facet === "genres" || facet === "certifications" ? "chips" : "facet-list"}>
        {shown.map((o) => (
          <button key={o.value} className={selected.includes(o.value) ? "chip active" : "chip"} onClick={() => toggle(o.value)}>
            <span>{labelFor(facet, o.value)}</span>
            <small>{o.count}</small>
          </button>
        ))}
      </div>
      {!q && filtered.length > 10 && (
        <button className="link-btn" onClick={() => setExpanded(!expanded)}>
          {expanded ? t("filters.showLess") : t("filters.showAll", { count: filtered.length })}
        </button>
      )}
    </section>
  );
}

function RangeFilter({ facet }: { facet: RangeFacet }) {
  const { t } = useTranslation();
  const viewItems = useViewItems();
  const value = useStore((s) => s.filters[facet]);
  const setFilters = useStore((s) => s.setFilters);
  const bounds = useMemo(() => rangeBounds(viewItems, facet), [viewItems, facet]);
  if (!bounds || bounds[0] === bounds[1]) return null;
  const format = rangeFormat(facet);
  return (
    <section className="filter-section">
      <h4>{t(`filters.${facet}`)}</h4>
      <RangeSlider
        bounds={bounds}
        value={value}
        step={facet === "imdb" ? 0.1 : facet === "runtime" ? 5 : facet === "myRating" ? 0.5 : 1}
        format={format}
        onChange={(v) => setFilters((f) => ({ ...f, [facet]: v }))}
      />
    </section>
  );
}

export function FilterPanel() {
  const { t } = useTranslation();
  const filters = useStore((s) => s.filters);
  const setFilters = useStore((s) => s.setFilters);
  const view = useStore((s) => s.view);
  const viewItems = useViewItems();
  // Las que TMDb no ha reconocido por el nombre del archivo (para corregirlas a mano).
  const unmatched = useMemo(() => viewItems.filter((i) => i.matched === 0).length, [viewItems]);

  return (
    <motion.aside
      className="filter-panel"
      initial={{ width: 0, opacity: 0 }}
      animate={{ width: 290, opacity: 1 }}
      exit={{ width: 0, opacity: 0 }}
      transition={{ type: "spring", stiffness: 420, damping: 40 }}
    >
      <div className="filter-panel-inner">
        {view !== "watched" && (
          <section className="filter-section">
            <h4>{t("filters.watched")}</h4>
            <Segmented
              value={filters.watched}
              onChange={(watched) => setFilters((f) => ({ ...f, watched }))}
              options={[
                { value: "all", label: t("filters.all") },
                { value: "unwatched", label: t("filters.unwatched") },
                { value: "watched", label: t("filters.seen") },
              ]}
            />
          </section>
        )}
        {view === "library" && (
          <section className="filter-section">
            <h4>{t("filters.subtitles")}</h4>
            <Segmented
              value={filters.subs}
              onChange={(subs) => setFilters((f) => ({ ...f, subs }))}
              options={[
                { value: "all", label: t("filters.all") },
                { value: "yes", label: t("filters.with") },
                { value: "no", label: t("filters.without") },
              ]}
            />
          </section>
        )}
        <RangeFilter facet="myRating" />
        <RangeFilter facet="imdb" />
        <RangeFilter facet="years" />
        <RangeFilter facet="runtime" />
        <FacetList facet="genres" />
        <FacetList facet="directors" searchable />
        <FacetList facet="actors" searchable />
        <FacetList facet="countries" searchable />
        <FacetList facet="languages" searchable />
        <FacetList facet="certifications" />
        {(unmatched > 0 || filters.unmatched) && (
          <section className="filter-section">
            <label className="check">
              <input type="checkbox" checked={filters.unmatched} onChange={(e) => setFilters((f) => ({ ...f, unmatched: e.target.checked }))} />
              {t("filters.unmatchedCount", { count: unmatched })}
            </label>
            <p className="filter-hint">{t("filters.unmatchedHint")}</p>
          </section>
        )}
      </div>
    </motion.aside>
  );
}
