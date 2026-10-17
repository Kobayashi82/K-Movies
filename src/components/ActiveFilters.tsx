import { X } from "lucide-react";
import { AnimatePresence, motion } from "motion/react";
import { useTranslation } from "react-i18next";
import { activeFilterCount, LIST_FACETS, type Filters, type RangeFacet } from "../filters";
import { useStore } from "../store";
import { formatNumber, formatRuntime } from "../util";
import { labelFor } from "./FilterPanel";

interface Chip {
  key: string;
  label: string;
  remove: (f: Filters) => Filters;
}

export function ActiveFilters({ count }: { count: number }) {
  const { t } = useTranslation();
  const filters = useStore((s) => s.filters);
  const query = useStore((s) => s.query);
  const setFilters = useStore((s) => s.setFilters);
  const clear = useStore((s) => s.clearFilters);
  const view = useStore((s) => s.view);
  const watchedMode = useStore((s) => s.watchedMode);
  const setWatchedMode = useStore((s) => s.setWatchedMode);

  const chips: Chip[] = [];
  for (const facet of LIST_FACETS) {
    for (const v of filters[facet]) {
      chips.push({ key: `${facet}:${v}`, label: labelFor(facet, v), remove: (f) => ({ ...f, [facet]: f[facet].filter((x) => x !== v) }) });
    }
  }
  const ranges: [RangeFacet, (v: number) => string][] = [
    ["imdb", (v) => formatNumber(v)],
    ["years", String],
    ["runtime", formatRuntime],
  ];
  for (const [facet, fmt] of ranges) {
    const r = filters[facet];
    if (r) chips.push({ key: facet, label: `${t(`filters.${facet}`)}: ${fmt(r[0])} – ${fmt(r[1])}`, remove: (f) => ({ ...f, [facet]: null }) });
  }
  if (filters.watched !== "all")
    chips.push({ key: "watched", label: t(filters.watched === "watched" ? "filters.seen" : "filters.unwatched"), remove: (f) => ({ ...f, watched: "all" }) });
  if (filters.subs !== "all")
    chips.push({ key: "subs", label: t(filters.subs === "yes" ? "filters.withSubs" : "filters.withoutSubs"), remove: (f) => ({ ...f, subs: "all" }) });
  if (filters.unmatched) chips.push({ key: "unmatched", label: t("filters.unmatched"), remove: (f) => ({ ...f, unmatched: false }) });

  const any = activeFilterCount(filters) > 0 || query.trim() !== "";

  return (
    <div className="active-filters">
      <span className="result-count">{t("grid.count", { count })}</span>
      <AnimatePresence initial={false}>
        {chips.map((c) => (
          <motion.button
            layout
            key={c.key}
            className="chip active removable"
            initial={{ opacity: 0, scale: 0.85 }}
            animate={{ opacity: 1, scale: 1 }}
            exit={{ opacity: 0, scale: 0.85 }}
            transition={{ duration: 0.16 }}
            onClick={() => setFilters(c.remove)}
          >
            {c.label}
            <X size={12} />
          </motion.button>
        ))}
      </AnimatePresence>
      {any && (
        <button className="link-btn" onClick={clear}>
          {t("filters.clearAll")}
        </button>
      )}
      {view === "watched" && (
        <div className="segmented view-mode">
          {(["grid", "history"] as const).map((m) => (
            <button key={m} className={watchedMode === m ? "active" : undefined} onClick={() => setWatchedMode(m)}>
              {t(`history.mode_${m}`)}
            </button>
          ))}
        </div>
      )}
    </div>
  );
}
