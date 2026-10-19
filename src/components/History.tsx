import { CalendarPlus, Hand, History as HistoryIcon, Play, RotateCcw, Trash2 } from "lucide-react";
import { useEffect, useMemo, useState } from "react";
import { useTranslation } from "react-i18next";
import { errorText, movieMenu, openMenu } from "../actions";
import { api, parseDbDate, type HistoryEntry, type Item } from "../api";
import { useStore } from "../store";
import { formatDate } from "../util";
import { PosterImage } from "./PosterCard";
import { Stars } from "./Stars";

/** Fecha local en el formato de `<input type="date">`. */
function inputDate(d: Date | null = new Date()): string {
  if (!d) return "";
  const pad = (n: number) => String(n).padStart(2, "0");
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}`;
}

const SOURCE_ICON = { player: Play, manual: Hand, letterboxd: HistoryIcon, kmovies4: HistoryIcon };

function SourceTag({ entry }: { entry: HistoryEntry }) {
  const { t } = useTranslation();
  const Icon = SOURCE_ICON[entry.source] ?? Hand;
  return (
    <span className="tag subtle" title={t(`history.source_${entry.source}`)}>
      <Icon size={12} />
      {t(`history.source_${entry.source}`)}
    </span>
  );
}

/** Fecha del visionado; al pulsarla se puede cambiar. */
function EntryDate({ entry, children }: { entry: HistoryEntry; children: React.ReactNode }) {
  const { t } = useTranslation();
  const [editing, setEditing] = useState(false);
  const [value, setValue] = useState("");
  const [error, setError] = useState<string | null>(null);
  const start = () => {
    setValue(inputDate(parseDbDate(entry.watchedAt)));
    setError(null);
    setEditing(true);
  };
  const commit = async () => {
    setEditing(false);
    if (!value || value === inputDate(parseDbDate(entry.watchedAt))) return;
    try {
      await api.setHistoryDate(entry.id, value);
    } catch (e) {
      setError(errorText(String(e)));
    }
  };
  if (editing) {
    return (
      <input
        type="date"
        className="date-input"
        value={value}
        max={inputDate()}
        autoFocus
        onChange={(e) => setValue(e.target.value)}
        onBlur={() => void commit()}
        onKeyDown={(e) => {
          if (e.key === "Enter") e.currentTarget.blur();
          if (e.key === "Escape") {
            e.stopPropagation();
            setEditing(false);
          }
        }}
      />
    );
  }
  return (
    <button className="entry-date" onClick={start} title={error ?? t("history.editDate")}>
      {children}
    </button>
  );
}

/** Nota del visionado; se ve al momento, sin esperar a que se recargue el historial. */
function EntryRating({ entry, size }: { entry: HistoryEntry; size?: number }) {
  const [value, setValue] = useState(entry.rating);
  useEffect(() => setValue(entry.rating), [entry.rating]);
  const change = (v: number | null) => {
    setValue(v);
    api.setHistoryRating(entry.id, v).catch(() => setValue(entry.rating));
  };
  return <Stars value={value} onChange={change} size={size} />;
}

function DeleteEntry({ entry }: { entry: HistoryEntry }) {
  const { t } = useTranslation();
  return (
    <button className="icon-btn small danger" onClick={() => void api.deleteHistory(entry.id)} title={t("history.delete")}>
      <Trash2 size={15} />
    </button>
  );
}

/** ¿Tiene hora? Las fechas puestas a mano son solo el día. */
const hasTime = (e: HistoryEntry) => e.watchedAt.length > 10;

/** Historial de una película, en su ficha. */
export function ItemHistory({ item }: { item: Item }) {
  const { t } = useTranslation();
  const [entries, setEntries] = useState<HistoryEntry[]>([]);
  const [adding, setAdding] = useState(false);
  const [date, setDate] = useState(inputDate);
  const [error, setError] = useState<string | null>(null);

  // `item` cambia cada vez que se recarga la biblioteca (p. ej. tras marcarla como vista).
  useEffect(() => {
    let alive = true;
    api.listHistory(item.id).then((e) => alive && setEntries(e), console.error);
    return () => {
      alive = false;
    };
  }, [item]);

  if (!entries.length) return null;

  const add = async () => {
    setError(null);
    try {
      await api.addHistory(item.id, date);
      setAdding(false);
    } catch (e) {
      setError(errorText(String(e)));
    }
  };

  return (
    <section className="detail-section">
      <h3>{t("history.title")}</h3>
      <ul className="history-entries">
        {entries.map((e) => {
          const d = parseDbDate(e.watchedAt);
          return (
            <li key={e.id}>
              <EntryDate entry={e}>
                {formatDate(d, hasTime(e) ? { dateStyle: "full", timeStyle: "short" } : { dateStyle: "full" })}
              </EntryDate>
              <EntryRating entry={e} />
              <span className="history-tags">
                {e.rewatch && (
                  <span className="tag">
                    <RotateCcw size={12} />
                    {t("history.rewatch")}
                  </span>
                )}
                <SourceTag entry={e} />
              </span>
              <DeleteEntry entry={e} />
            </li>
          );
        })}
      </ul>
      {adding ? (
        <div className="history-add">
          <input type="date" className="date-input" value={date} max={inputDate()} onChange={(e) => setDate(e.target.value)} />
          <button className="btn small primary" onClick={() => void add()} disabled={!date}>
            {t("history.addConfirm")}
          </button>
          <button className="btn small" onClick={() => setAdding(false)}>
            {t("common.cancel")}
          </button>
        </div>
      ) : (
        <button
          className="btn small"
          onClick={() => {
            setDate(inputDate());
            setAdding(true);
          }}
        >
          <CalendarPlus size={15} />
          {t("history.add")}
        </button>
      )}
      {error && <div className="detail-error">{error}</div>}
    </section>
  );
}

interface Month {
  key: string;
  date: Date;
  rows: { entry: HistoryEntry; item: Item; date: Date }[];
}

/** Pestaña Vistas en modo historial: cada visionado, por meses, del más reciente al más antiguo. */
export function HistoryList({ items }: { items: Item[] }) {
  const { t } = useTranslation();
  const all = useStore((s) => s.items);
  const select = useStore((s) => s.select);
  const [entries, setEntries] = useState<HistoryEntry[] | null>(null);

  useEffect(() => {
    let alive = true;
    api.listHistory().then((e) => alive && setEntries(e), console.error);
    return () => {
      alive = false;
    };
  }, [all]);

  // Solo los visionados de las películas que pasan la búsqueda y los filtros.
  const months = useMemo(() => {
    const byId = new Map(items.map((i) => [i.id, i]));
    const out: Month[] = [];
    for (const entry of entries ?? []) {
      const item = byId.get(entry.itemId);
      const date = parseDbDate(entry.watchedAt);
      if (!item || !date) continue;
      const key = `${date.getFullYear()}-${date.getMonth()}`;
      let month = out[out.length - 1];
      if (month?.key !== key) {
        month = { key, date, rows: [] };
        out.push(month);
      }
      month.rows.push({ entry, item, date });
    }
    return out;
  }, [entries, items]);

  if (entries && !months.length) {
    return (
      <div className="empty">
        <HistoryIcon size={40} strokeWidth={1.5} />
        <p>{t("history.empty")}</p>
      </div>
    );
  }

  return (
    <div className="grid-scroll" onMouseLeave={() => useStore.getState().hover(null)}>
      <div className="history">
        {months.map((m) => (
          <section key={m.key} className="history-month">
            <h3>
              {formatDate(m.date, { month: "long", year: "numeric" })}
              <small>{t("history.count", { count: m.rows.length })}</small>
            </h3>
            <ul>
              {m.rows.map(({ entry, item, date }) => (
                <li
                  key={entry.id}
                  className="history-row"
                  onMouseEnter={() => useStore.getState().hover(item.id)}
                  onContextMenu={(e) => openMenu(e, movieMenu(item))}
                >
                  <EntryDate entry={entry}>
                    <span className="history-day">
                      <strong>{date.getDate()}</strong>
                      <small>{formatDate(date, { weekday: "short" })}</small>
                    </span>
                  </EntryDate>
                  <button className="history-open" onClick={() => select(item.id)}>
                    <span className="history-thumb">
                      <PosterImage item={item} />
                    </span>
                    <span className="history-title">
                      <strong>{item.title}</strong>
                      <small>
                        {[item.year, item.directors[0], hasTime(entry) ? formatDate(date, { timeStyle: "short" }) : null]
                          .filter(Boolean)
                          .join(" · ")}
                      </small>
                    </span>
                  </button>
                  <EntryRating entry={entry} size={14} />
                  <span className="history-tags">
                    {entry.rewatch && (
                      <span className="tag">
                        <RotateCcw size={12} />
                        {t("history.rewatch")}
                      </span>
                    )}
                    <SourceTag entry={entry} />
                  </span>
                  <DeleteEntry entry={entry} />
                </li>
              ))}
            </ul>
          </section>
        ))}
      </div>
    </div>
  );
}
