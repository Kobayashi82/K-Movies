import { ChevronLeft, ChevronRight, Play, X } from "lucide-react";
import { useEffect, useRef, useState } from "react";
import { useTranslation } from "react-i18next";
import { movieMenu, openMenu, play } from "../actions";
import { api, fileUrl, type Item } from "../api";
import { useNowPlaying } from "../hooks";
import { useStore } from "../store";
import { formatRemaining, hueOf } from "../util";

function ContinueCard({ item }: { item: Item }) {
  const { t } = useTranslation();
  const live = useNowPlaying(item.id);
  const [loaded, setLoaded] = useState(false);
  const position = live?.position ?? item.position;
  const duration = live?.duration ?? item.duration;
  const progress = position && duration ? Math.min(1, position / duration) : 0;
  const image = fileUrl(item.backdrop ?? item.frame) ?? fileUrl(item.poster);
  return (
    <div className="continue-card" onContextMenu={(e) => openMenu(e, movieMenu(item))}>
      <button className="continue-open" onClick={() => useStore.getState().select(item.id)}>
        {image ? (
          <img src={image} alt="" loading="lazy" decoding="async" className={loaded ? "loaded" : undefined} onLoad={() => setLoaded(true)} />
        ) : (
          <div className="poster-fallback" style={{ ["--hue" as string]: hueOf(item.title) }} />
        )}
        <span className="continue-shade" />
        <span className="continue-text">
          <strong>{item.title}</strong>
          <small>{live ? t("continue.playingNow") : t("continue.remaining", { time: formatRemaining(position, duration) })}</small>
        </span>
        <span className="progress">
          <span style={{ width: `${progress * 100}%` }} />
        </span>
      </button>
      {!live && (
        <button className="continue-play" onClick={() => void play(item.id)} title={t("detail.resume")}>
          <Play size={22} fill="currentColor" strokeWidth={0} />
        </button>
      )}
      <button className="continue-remove icon-btn small" onClick={() => void api.clearProgress(item.id)} title={t("continue.remove")}>
        <X size={14} />
      </button>
    </div>
  );
}

/** Fila "Seguir viendo": las películas a medias, con su progreso; se continúa con un clic. */
export function ContinueRow({ items }: { items: Item[] }) {
  const { t } = useTranslation();
  const rowRef = useRef<HTMLDivElement>(null);
  const gridSize = useStore((s) => s.gridSize);
  const [edges, setEdges] = useState({ left: false, right: false });

  const update = () => {
    const el = rowRef.current;
    if (el) setEdges({ left: el.scrollLeft > 4, right: el.scrollLeft + el.clientWidth < el.scrollWidth - 4 });
  };
  useEffect(() => {
    update();
    const el = rowRef.current;
    if (!el) return;
    const ro = new ResizeObserver(update);
    ro.observe(el);
    return () => ro.disconnect();
  }, [items.length, gridSize]);

  const scroll = (dir: number) => {
    const el = rowRef.current;
    el?.scrollBy({ left: dir * el.clientWidth * 0.8, behavior: "smooth" });
  };

  // Las tarjetas crecen con el deslizador de tamaño, como los pósters.
  const width = Math.round(Math.max(220, Math.min(400, gridSize * 1.6)));

  return (
    <section className="continue" aria-label={t("continue.title")}>
      <h2>{t("continue.title")}</h2>
      <div className="continue-wrap">
        <div ref={rowRef} className="continue-row" onScroll={update} style={{ ["--card-w" as string]: `${width}px` }}>
          {items.map((item) => (
            <ContinueCard key={item.id} item={item} />
          ))}
        </div>
        {edges.left && (
          <button className="continue-arrow left" onClick={() => scroll(-1)} title={t("continue.previous")}>
            <ChevronLeft size={22} />
          </button>
        )}
        {edges.right && (
          <button className="continue-arrow right" onClick={() => scroll(1)} title={t("continue.next")}>
            <ChevronRight size={22} />
          </button>
        )}
      </div>
    </section>
  );
}
