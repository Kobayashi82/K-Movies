import { Check, Clapperboard, Play } from "lucide-react";
import { memo, useState } from "react";
import { useTranslation } from "react-i18next";
import { movieMenu, openMenu, play } from "../actions";
import { fileUrl, type Item } from "../api";
import { useNowPlaying } from "../hooks";
import { useStore } from "../store";
import { formatNumber, formatRuntime, heroTransition, hueOf } from "../util";

/** `showTitle`: el título dentro del póster cuando no hay imagen (solo si no se ve ya debajo o al lado). */
export function PosterImage({ item, eager = false, showTitle = false }: { item: Item; eager?: boolean; showTitle?: boolean }) {
  const [loaded, setLoaded] = useState(false);
  const src = fileUrl(item.poster);
  if (!src) {
    // Sin póster: un fotograma de la propia película (si se ha podido sacar) o un cartel oscuro
    // con una claqueta, con un tono propio de cada película.
    const frame = fileUrl(item.frame);
    return (
      <div className={frame ? "poster-fallback has-frame" : "poster-fallback"} style={{ ["--hue" as string]: hueOf(item.title) }}>
        {frame ? (
          <img src={frame} alt="" draggable={false} loading={eager ? "eager" : "lazy"} decoding="async" className="loaded" />
        ) : (
          <Clapperboard className="poster-fallback-icon" strokeWidth={1.2} />
        )}
        {showTitle && (
          <div className="poster-fallback-text">
            <span>{item.title}</span>
            {item.year && <small>{item.year}</small>}
          </div>
        )}
      </div>
    );
  }
  return (
    <img
      src={src}
      alt=""
      draggable={false}
      loading={eager ? "eager" : "lazy"}
      decoding="async"
      className={loaded ? "loaded" : undefined}
      onLoad={() => setLoaded(true)}
    />
  );
}


/** Qué se ve en cada tarjeta (Ajustes → Apariencia). */
export interface CardDisplay {
  rating: "imdb" | "tmdb" | "both" | "none";
  title: boolean;
  year: boolean;
  runtime: boolean;
}

export const PosterCard = memo(function PosterCard({ item, display, index }: { item: Item; display: CardDisplay; index: number }) {
  const { t } = useTranslation();
  const ratings = [
    { source: "imdb", label: "IMDb", value: item.imdbRating },
    { source: "tmdb", label: "TMDb", value: item.tmdbRating },
  ].filter((r) => r.value != null && (display.rating === "both" || display.rating === r.source));
  const year = display.year && item.year ? <span className="card-year">{item.year}</span> : null;
  // La duración de TMDb y, si no hay, la del archivo.
  const minutes = item.runtime ?? (item.duration ? Math.round(item.duration / 60) : null);
  const runtime =
    display.runtime && minutes ? (
      <span className="card-year">
        {year && "· "}
        {formatRuntime(minutes)}
      </span>
    ) : null;
  // Mientras se ve, la barra avanza en directo.
  const live = useNowPlaying(item.id);
  const position = live?.tracked ? live.position : item.position;
  const duration = live?.tracked ? live.duration : item.duration;
  const progress = position && duration ? position / duration : 0;
  const open = (e: React.MouseEvent<HTMLButtonElement>) => {
    const poster = e.currentTarget.querySelector<HTMLElement>(".poster");
    heroTransition(poster, () => useStore.getState().select(item.id));
  };
  return (
    <button
      className="card"
      data-index={index}
      onMouseEnter={() => useStore.getState().hover(item.id)}
      onFocus={() => useStore.getState().hover(item.id)}
      onClick={open}
      onContextMenu={(e) => openMenu(e, movieMenu(item))}
    >
      <div className="poster" data-poster-id={item.id}>
        <PosterImage item={item} showTitle={!display.title} />
        <div className="poster-shade" />
        {ratings.length > 0 && (
          <span className="rating-badges">
            {ratings.map((r) => (
              <span key={r.source} className={`rating-badge ${r.source}`}>
                <b>{r.label}</b>
                {formatNumber(r.value)}
              </span>
            ))}
          </span>
        )}
        {item.watchCount > 0 && (
          <span className="watched-badge">
            <Check size={13} strokeWidth={3} />
          </span>
        )}
        {item.fileCount > 0 && (
          // El círculo del centro reproduce (o trae su reproductor al frente); el resto del póster abre la ficha.
          <span
            className="play-hint"
            title={live ? t("menu.goToPlayer") : t("detail.play")}
            onClick={(e) => {
              e.stopPropagation();
              void play(item.id);
            }}
          >
            <Play size={22} fill="currentColor" strokeWidth={0} />
          </span>
        )}
        {progress > 0.01 && !live?.watched && (
          <span className="progress">
            <span style={{ width: `${Math.min(progress, 1) * 100}%` }} />
          </span>
        )}
      </div>
      {(display.title || display.year || display.runtime) && (
        <div className="card-info">
          {display.title && (
            <span className="card-title" title={item.title}>
              {item.title}
            </span>
          )}
          {year}
          {runtime}
          {item.matched === 0 && <span className="unmatched-dot" />}
        </div>
      )}
    </button>
  );
});
