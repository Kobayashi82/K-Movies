import { openUrl } from "@tauri-apps/plugin-opener";
import { BookmarkPlus, Check, Clapperboard, ExternalLink, Filter, Loader2, X } from "lucide-react";
import { motion } from "motion/react";
import { useEffect, useMemo, useState } from "react";
import { useTranslation } from "react-i18next";
import { movieMenu, openMenu } from "../actions";
import { api, cachedTmdbImage, type Item, type PersonCredit, type PersonPage } from "../api";
import { useStore } from "../store";
import { hueOf } from "../util";
import { PosterImage } from "./PosterCard";

type Role = "director" | "cast";

/** Una película de la persona: la tuya (`item`) o solo de TMDb. */
interface Row {
  key: string;
  item: Item | null;
  tmdbId: number | null;
  title: string;
  year: number | null;
  posterPath: string | null;
  character: string | null;
}

const FILTERS = ["all", "disk", "pending", "watched", "unwatched"] as const;
type PersonFilter = (typeof FILTERS)[number];

const passes = (row: Row, f: PersonFilter) => {
  const i = row.item;
  switch (f) {
    case "all":
      return true;
    case "disk":
      return !!i && i.fileCount > 0;
    case "pending":
      return !!i?.watchlist;
    case "watched":
      return !!i && i.watchCount > 0;
    case "unwatched":
      return !i || i.watchCount === 0;
  }
};

/**
 * Actor o director: su foto y todas sus películas (las tuyas, en disco, Pendientes o Vistas, y el
 * resto de su filmografía en TMDb), con filtros. Pulsar una tuya abre su ficha; una que no tienes,
 * un menú para añadirla a Pendientes o a Vistas.
 */
export function PersonDialog({ personId }: { personId: number }) {
  const { t } = useTranslation();
  const items = useStore((s) => s.items);
  const saved = useStore((s) => s.settings.personFilter);
  const [person, setPerson] = useState<PersonPage | null>(null);
  const [remote, setRemote] = useState<PersonCredit[] | null>(null);
  const [loading, setLoading] = useState(false);
  const filter: PersonFilter = FILTERS.includes(saved as PersonFilter) ? (saved as PersonFilter) : "all";
  const close = () => useStore.getState().openDialog(null);

  useEffect(() => {
    let alive = true;
    api.getPerson(personId).then((p) => {
      if (!alive) return;
      setPerson(p);
      // Su filmografía completa (sin clave de TMDb o sin conexión, solo las tuyas).
      if (p.tmdbId == null) return;
      setLoading(true);
      api
        .personCredits(p.tmdbId)
        .then((c) => alive && setRemote(c), console.error)
        .finally(() => alive && setLoading(false));
    }, console.error);
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

  // Por papel; dentro de cada uno, de la más reciente a la más antigua (las sin fecha, al final).
  const groups = useMemo(() => {
    const byId = new Map(items.map((i) => [i.id, i]));
    const byTmdb = new Map(items.filter((i) => i.tmdbId != null).map((i) => [i.tmdbId!, i]));
    const out: { role: Role; rows: Row[] }[] = [];
    for (const role of ["director", "cast"] as const) {
      const rows = new Map<string, Row>();
      for (const c of person?.credits ?? []) {
        const item = c.role === role ? byId.get(c.itemId) : undefined;
        if (!item) continue;
        const key = item.tmdbId != null ? `t${item.tmdbId}` : `i${item.id}`;
        rows.set(key, { key, item, tmdbId: item.tmdbId, title: item.title, year: item.year, posterPath: null, character: c.character });
      }
      for (const c of remote ?? []) {
        if (c.role !== role || rows.has(`t${c.tmdbId}`)) continue;
        const item = byTmdb.get(c.tmdbId) ?? null;
        rows.set(`t${c.tmdbId}`, {
          key: `t${c.tmdbId}`,
          item,
          tmdbId: c.tmdbId,
          title: item?.title ?? c.title,
          year: item?.year ?? c.year,
          posterPath: c.posterPath,
          character: c.character,
        });
      }
      const list = [...rows.values()].sort((a, b) => (b.year ?? -1) - (a.year ?? -1));
      if (list.length) out.push({ role, rows: list });
    }
    return out;
  }, [items, person, remote]);

  const shown = groups.map((g) => ({ ...g, rows: g.rows.filter((r) => passes(r, filter)) })).filter((g) => g.rows.length);
  const owned = new Set(groups.flatMap((g) => g.rows.filter((r) => r.item).map((r) => r.item!.id))).size;
  const total = new Set(groups.flatMap((g) => g.rows.map((r) => r.key))).size;

  const open = (id: number) => {
    close();
    useStore.getState().select(id);
  };

  /** Una que no tienes: añadirla a Pendientes o a Vistas, o verla en TMDb. */
  const notOwnedMenu = (e: React.MouseEvent, row: Row) => {
    const id = row.tmdbId;
    if (id == null) return;
    const add = (list: "pending" | "watched") => () =>
      api.addItem(id, list).catch((err) => useStore.getState().pushNotice({ kind: "actionError", title: "", itemId: null, detail: String(err) }));
    openMenu(e, [
      { label: t("detail.addPending"), icon: BookmarkPlus, action: add("pending") },
      { label: t("detail.markWatched"), icon: Check, action: add("watched") },
      null,
      { label: t("person.openTmdb"), icon: ExternalLink, action: () => openUrl(`https://www.themoviedb.org/movie/${id}`) },
    ]);
  };

  /** Deja la biblioteca filtrada por esta persona (como director o en el reparto). */
  const filterLibrary = () => {
    if (!person) return;
    const { setFilters, select } = useStore.getState();
    const has = (role: Role) => groups.some((g) => g.role === role && g.rows.some((r) => r.item));
    const directs = has("director");
    const acts = has("cast");
    setFilters((f) => ({
      ...f,
      directors: directs && !acts ? [person.name] : f.directors,
      actors: acts || !directs ? [person.name] : f.actors,
    }));
    close();
    select(null);
  };

  const photo = cachedTmdbImage(person?.profileUrl ?? null, "w185");

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
            {person && (
              <p className="hint">
                {t("person.count", { count: owned })}
                {remote && ` · ${t("person.total", { count: total })}`}
              </p>
            )}
            <div className="person-actions">
              <button className="btn small" onClick={filterLibrary} disabled={!owned}>
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
        <div className="person-filters">
          <div className="segmented">
            {FILTERS.map((f) => (
              <button key={f} className={filter === f ? "active" : undefined} onClick={() => useStore.getState().setSetting("personFilter", f)}>
                {t(`person.filter_${f}`)}
              </button>
            ))}
          </div>
          {loading && <Loader2 size={16} className="spin" aria-label={t("person.loading")} />}
        </div>
        <div className="modal-body">
          {shown.map((g) => (
            <section key={g.role} className="person-section">
              <h3>{t(`person.${g.role}`)}</h3>
              <div className="person-grid">
                {g.rows.map((row) => {
                  const item = row.item;
                  const poster = row.posterPath ? cachedTmdbImage(row.posterPath, "w185") : null;
                  return (
                    <button
                      key={row.key}
                      className={item ? "person-movie" : "person-movie not-owned"}
                      onClick={(e) => (item ? open(item.id) : notOwnedMenu(e, row))}
                      onContextMenu={(e) => (item ? openMenu(e, movieMenu(item)) : notOwnedMenu(e, row))}
                    >
                      <span className="person-poster">
                        {item ? (
                          <PosterImage item={item} />
                        ) : poster ? (
                          <img src={poster} alt="" loading="lazy" decoding="async" />
                        ) : (
                          <span className="poster-fallback" style={{ ["--hue" as string]: hueOf(row.title) }}>
                            <Clapperboard className="poster-fallback-icon" strokeWidth={1.2} />
                          </span>
                        )}
                        {item && item.watchCount > 0 && (
                          <span className="watched-badge">
                            <Check size={12} strokeWidth={3} />
                          </span>
                        )}
                      </span>
                      <strong>{row.title}</strong>
                      <small>{[row.year, row.character].filter(Boolean).join(" · ")}</small>
                    </button>
                  );
                })}
              </div>
            </section>
          ))}
          {!shown.length && !loading && person && <p className="hint person-empty">{t("person.none")}</p>}
        </div>
      </motion.div>
    </motion.div>
  );
}
