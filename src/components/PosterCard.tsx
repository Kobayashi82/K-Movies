import { Check, Play } from "lucide-react";
import { memo, useState } from "react";
import { fileUrl, type Item } from "../api";
import { useStore } from "../store";
import { formatNumber, heroTransition, hueOf } from "../util";

export function PosterImage({ item, eager = false }: { item: Item; eager?: boolean }) {
  const [loaded, setLoaded] = useState(false);
  const src = fileUrl(item.poster);
  if (!src) {
    const hue = hueOf(item.title);
    return (
      <div className="poster-fallback" style={{ ["--hue" as string]: hue }}>
        <span>{item.title}</span>
        {item.year && <small>{item.year}</small>}
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
  year: "before" | "after" | "none";
}

export const PosterCard = memo(function PosterCard({ item, display }: { item: Item; display: CardDisplay }) {
  const ratings = [
    { source: "imdb", label: "IMDb", value: item.imdbRating },
    { source: "tmdb", label: "TMDb", value: item.tmdbRating },
  ].filter((r) => r.value != null && (display.rating === "both" || display.rating === r.source));
  const year = display.year !== "none" && item.year ? <span className="card-year">{item.year}</span> : null;
  const progress = item.position && item.duration ? item.position / item.duration : 0;
  const open = (e: React.MouseEvent<HTMLButtonElement>) => {
    const poster = e.currentTarget.querySelector<HTMLElement>(".poster");
    heroTransition(poster, () => useStore.getState().select(item.id));
  };
  return (
    <button
      className="card"
      onMouseEnter={() => useStore.getState().hover(item.id)}
      onFocus={() => useStore.getState().hover(item.id)}
      onClick={open}
    >
      <div className="poster" data-poster-id={item.id}>
        <PosterImage item={item} />
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
          <span className="play-hint">
            <Play size={22} fill="currentColor" strokeWidth={0} />
          </span>
        )}
        {progress > 0.01 && (
          <span className="progress">
            <span style={{ width: `${Math.min(progress, 1) * 100}%` }} />
          </span>
        )}
      </div>
      {(display.title || display.year !== "none") && (
        <div className="card-info">
          {display.year === "before" && year}
          {display.title && (
            <span className="card-title" title={item.title}>
              {item.title}
            </span>
          )}
          {display.year === "after" && year}
          {item.matched === 0 && <span className="unmatched-dot" />}
        </div>
      )}
    </button>
  );
});
