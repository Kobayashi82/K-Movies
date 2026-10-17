import { openUrl } from "@tauri-apps/plugin-opener";
import { Check, ExternalLink, Filter, X } from "lucide-react";
import { motion } from "motion/react";
import { useEffect, useMemo, useState } from "react";
import { useTranslation } from "react-i18next";
import { api, cachedTmdbImage, type Item, type PersonPage } from "../api";
import { useStore } from "../store";
import { PosterImage } from "./PosterCard";

/** Actor o director: su foto y sus películas que tienes (en disco, Pendientes o Vistas). */
export function PersonDialog({ personId }: { personId: number }) {
  const { t } = useTranslation();
  const items = useStore((s) => s.items);
  const [person, setPerson] = useState<PersonPage | null>(null);
  const close = () => useStore.getState().openDialog(null);

  useEffect(() => {
    let alive = true;
    api.getPerson(personId).then((p) => alive && setPerson(p), console.error);
    return () => {
      alive = false;
    };
  }, [personId]);

  useEffect(() => {
    const onKey = (e: KeyboardEvent) => {
      if (e.key !== "Escape") return;
      e.stopImmediatePropagation();
      close();
    };
    window.addEventListener("keydown", onKey, { capture: true });
    return () => window.removeEventListener("keydown", onKey, { capture: true });
  }, []);

  // Por papel; dentro de cada uno, de la más reciente a la más antigua.
  const groups = useMemo(() => {
    const byId = new Map(items.map((i) => [i.id, i]));
    const out: { role: "director" | "cast"; rows: { item: Item; character: string | null }[] }[] = [];
    for (const role of ["director", "cast"] as const) {
      const rows = (person?.credits ?? [])
        .filter((c) => c.role === role && byId.has(c.itemId))
        .map((c) => ({ item: byId.get(c.itemId)!, character: c.character }))
        .sort((a, b) => (b.item.year ?? 0) - (a.item.year ?? 0));
      if (rows.length) out.push({ role, rows });
    }
    return out;
  }, [items, person]);

  const open = (id: number) => {
    close();
    useStore.getState().select(id);
  };

  /** Deja la biblioteca filtrada por esta persona (como director o en el reparto). */
  const filter = () => {
    if (!person) return;
    const { setFilters, select } = useStore.getState();
    const directs = groups.some((g) => g.role === "director");
    const acts = groups.some((g) => g.role === "cast");
    setFilters((f) => ({
      ...f,
      directors: directs && !acts ? [person.name] : f.directors,
      actors: acts || !directs ? [person.name] : f.actors,
    }));
    close();
    select(null);
  };

  const photo = cachedTmdbImage(person?.profileUrl ?? null, "w185");
  const total = new Set(groups.flatMap((g) => g.rows.map((r) => r.item.id))).size;

  return (
    <motion.div className="modal-backdrop" onClick={close} initial={{ opacity: 0 }} animate={{ opacity: 1 }} exit={{ opacity: 0 }}>
      <motion.div
        className="modal person-dialog"
        role="dialog"
        aria-modal="true"
        aria-label={person?.name}
        onClick={(e) => e.stopPropagation()}
        initial={{ opacity: 0, y: 20 }}
        animate={{ opacity: 1, y: 0 }}
        exit={{ opacity: 0, y: 12 }}
      >
        <header className="person-header">
          <div className="cast-photo person-photo">{photo ? <img src={photo} alt="" /> : <span>{person?.name[0]}</span>}</div>
          <div className="person-info">
            <h2>{person?.name}</h2>
            {person && <p className="hint">{t("person.count", { count: total })}</p>}
            <div className="person-actions">
              <button className="btn small" onClick={filter} disabled={!total}>
                <Filter size={15} />
                {t("person.filter")}
              </button>
              {person?.tmdbId && (
                <button className="link-btn" onClick={() => void openUrl(`https://www.themoviedb.org/person/${person.tmdbId}`)}>
                  TMDb <ExternalLink size={12} />
                </button>
              )}
            </div>
          </div>
          <button className="icon-btn" onClick={close} title={t("common.close")}>
            <X size={18} />
          </button>
        </header>
        <div className="modal-body">
          {groups.map((g) => (
            <section key={g.role} className="person-section">
              <h3>{t(`person.${g.role}`)}</h3>
              <div className="person-grid">
                {g.rows.map(({ item, character }) => (
                  <button key={item.id} className="person-movie" onClick={() => open(item.id)}>
                    <span className="person-poster">
                      <PosterImage item={item} />
                      {item.watchCount > 0 && (
                        <span className="watched-badge">
                          <Check size={12} strokeWidth={3} />
                        </span>
                      )}
                    </span>
                    <strong>{item.title}</strong>
                    <small>{[item.year, character].filter(Boolean).join(" · ")}</small>
                  </button>
                ))}
              </div>
            </section>
          ))}
        </div>
      </motion.div>
    </motion.div>
  );
}
