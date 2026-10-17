import { Film, Loader2, Search, X } from "lucide-react";
import { motion } from "motion/react";
import { useEffect, useMemo, useRef, useState } from "react";
import { useTranslation } from "react-i18next";
import { errorText } from "../actions";
import { api, tmdbImage, type Candidate } from "../api";
import { useStore, type Dialog } from "../store";

type SearchDialogProps = Extract<Dialog, { kind: "search" }>;

/**
 * Búsqueda en TMDb. En modo `identify` corrige la película de un item; en `pending` y
 * `watched` añade una película que no está en disco a esa lista.
 */
export function SearchDialog({ mode, itemId, fileId, query: initialQuery, year: initialYear }: SearchDialogProps) {
  const { t } = useTranslation();
  const [query, setQuery] = useState(initialQuery);
  const [year, setYear] = useState(initialYear ? String(initialYear) : "");
  const [results, setResults] = useState<Candidate[] | null>(null);
  const [loading, setLoading] = useState(false);
  const [busyId, setBusyId] = useState<number | null>(null);
  const [error, setError] = useState<string | null>(null);
  const inputRef = useRef<HTMLInputElement>(null);
  const items = useStore((s) => s.items);
  const close = () => useStore.getState().openDialog(null);

  // Qué tenemos ya de cada película de TMDb, para marcarlo en los resultados.
  const known = useMemo(() => {
    const map = new Map<number, { library: boolean; pending: boolean; watched: boolean }>();
    for (const i of items) if (i.tmdbId) map.set(i.tmdbId, { library: i.fileCount > 0, pending: i.watchlist, watched: i.watchCount > 0 });
    return map;
  }, [items]);

  useEffect(() => {
    inputRef.current?.select();
    const onKey = (e: KeyboardEvent) => {
      if (e.key === "Escape") {
        e.stopImmediatePropagation();
        close();
      }
    };
    window.addEventListener("keydown", onKey, { capture: true });
    return () => window.removeEventListener("keydown", onKey, { capture: true });
  }, []);

  // Busca al dejar de escribir.
  useEffect(() => {
    const q = query.trim();
    if (q.length < 2) {
      setResults(null);
      return;
    }
    let alive = true;
    const timer = window.setTimeout(async () => {
      setLoading(true);
      setError(null);
      try {
        const y = Number(year);
        const r = await api.searchTmdb(q, year.length === 4 && y > 1870 ? y : null);
        if (alive) setResults(r);
      } catch (e) {
        if (alive) setError(errorText(String(e)));
      } finally {
        if (alive) setLoading(false);
      }
    }, 350);
    return () => {
      alive = false;
      window.clearTimeout(timer);
    };
  }, [query, year]);

  const choose = async (c: Candidate) => {
    setBusyId(c.id);
    setError(null);
    try {
      const id =
        mode === "identify" && fileId != null
          ? await api.identifyFile(fileId, c.id)
          : mode === "identify" && itemId != null
            ? await api.identifyItem(itemId, c.id)
            : await api.addItem(c.id, mode === "watched" ? "watched" : "pending");
      await useStore.getState().refreshItems();
      close();
      useStore.getState().select(id);
    } catch (e) {
      setError(errorText(String(e)));
      setBusyId(null);
    }
  };

  return (
    <motion.div className="modal-backdrop" onClick={close} initial={{ opacity: 0 }} animate={{ opacity: 1 }} exit={{ opacity: 0 }}>
      <motion.div
        className="modal search-dialog"
        role="dialog"
        aria-modal="true"
        onClick={(e) => e.stopPropagation()}
        initial={{ opacity: 0, y: 24, scale: 0.97 }}
        animate={{ opacity: 1, y: 0, scale: 1 }}
        exit={{ opacity: 0, y: 16, scale: 0.98 }}
        transition={{ type: "spring", stiffness: 420, damping: 34 }}
      >
        <header className="modal-header">
          <h2>{t(`search.title_${mode}`)}</h2>
          <button className="icon-btn" onClick={close} title={t("common.close")}>
            <X size={18} />
          </button>
        </header>
        <div className="search-fields">
          <label className="search grow">
            <Search size={16} />
            <input ref={inputRef} value={query} onChange={(e) => setQuery(e.target.value)} placeholder={t("search.placeholder")} spellCheck={false} />
            {loading && <Loader2 size={16} className="spin" />}
          </label>
          <input
            className="year-input"
            value={year}
            onChange={(e) => setYear(e.target.value.replace(/\D/g, "").slice(0, 4))}
            placeholder={t("search.year")}
            inputMode="numeric"
          />
        </div>
        <p className="hint search-hint">{t("search.hint")}</p>
        {error && <p className="hint error search-hint">{error}</p>}

        <div className="search-results">
          {results?.length === 0 && !loading && <p className="empty-small">{t("search.noResults")}</p>}
          {results?.map((c) => {
            const k = known.get(c.id);
            const poster = tmdbImage(c.posterPath, "w154");
            return (
              <button key={c.id} className="result" onClick={() => void choose(c)} disabled={busyId != null}>
                <div className="result-poster">{poster ? <img src={poster} alt="" loading="lazy" /> : <Film size={22} />}</div>
                <div className="result-info">
                  <div className="result-title">
                    <strong>{c.title}</strong>
                    {c.year && <span>{c.year}</span>}
                  </div>
                  {c.originalTitle && c.originalTitle !== c.title && <small className="result-original">{c.originalTitle}</small>}
                  {c.overview && <p>{c.overview}</p>}
                  {k && (
                    <div className="result-tags">
                      {k.library && <span>{t("views.library")}</span>}
                      {k.pending && <span>{t("views.pending")}</span>}
                      {k.watched && <span>{t("views.watched")}</span>}
                    </div>
                  )}
                </div>
                {busyId === c.id && <Loader2 size={18} className="spin result-busy" />}
              </button>
            );
          })}
        </div>
      </motion.div>
    </motion.div>
  );
}
