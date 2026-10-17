import { openUrl, revealItemInDir } from "@tauri-apps/plugin-opener";
import { Bookmark, BookmarkCheck, Check, Clapperboard, ExternalLink, FolderOpen, Play, Star, X } from "lucide-react";
import { useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import { api, fileUrl, parseDbDate, tmdbImage, type Item, type ItemDetail } from "../api";
import { useStore } from "../store";
import { displayName, formatDate, formatNumber, formatRuntime, formatSize, heroTransition } from "../util";
import { PosterImage } from "./PosterCard";

export function Detail({ item }: { item: Item }) {
  const { t } = useTranslation();
  const [detail, setDetail] = useState<ItemDetail | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let alive = true;
    setDetail(null);
    api.getItem(item.id).then((d) => alive && setDetail(d), console.error);
    return () => {
      alive = false;
    };
  }, [item.id]);

  const close = () => {
    const card = document.querySelector<HTMLElement>(`[data-poster-id="${item.id}"]`);
    heroTransition(card, () => useStore.getState().select(null), true);
  };

  useEffect(() => {
    const onKey = (e: KeyboardEvent) => e.key === "Escape" && close();
    window.addEventListener("keydown", onKey);
    return () => window.removeEventListener("keydown", onKey);
  });

  const play = async () => {
    setError(null);
    try {
      await api.playItem(item.id);
    } catch (e) {
      setError(String(e) === "file_missing" ? t("detail.fileMissing") : String(e));
    }
  };

  const directors = detail?.people.filter((p) => p.role === "director") ?? [];
  const cast = detail?.people.filter((p) => p.role === "cast") ?? [];
  const backdrop = fileUrl(item.backdrop);
  const released = parseDbDate(item.releaseDate);
  const watched = item.watchCount > 0;

  return (
    <div className="detail" role="dialog" aria-modal="true" aria-label={item.title}>
      <div className="detail-backdrop">
        {backdrop && <img src={backdrop} alt="" />}
        <div className="detail-shade" />
      </div>
      <button className="detail-close icon-btn" onClick={close} title={t("common.close")}>
        <X size={20} />
      </button>

      <div className="detail-scroll">
        <div className="detail-main">
          <div className="detail-poster">
            <PosterImage item={item} eager />
          </div>

          <div className="detail-info">
            <h1>{item.title}</h1>
            {item.originalTitle && item.originalTitle !== item.title && <div className="detail-original">{item.originalTitle}</div>}

            <div className="detail-meta">
              {item.year && <span>{item.year}</span>}
              {item.runtime && <span>{formatRuntime(item.runtime)}</span>}
              {item.certification && <span className="cert">{item.certification}</span>}
              {item.genres.length > 0 && <span>{item.genres.join(", ")}</span>}
            </div>

            <div className="detail-ratings">
              {item.imdbRating != null && (
                <button
                  className="rating imdb"
                  onClick={() => item.imdbId && void openUrl(`https://www.imdb.com/title/${item.imdbId}/`)}
                  title="IMDb"
                >
                  <span className="rating-source">IMDb</span>
                  <Star size={14} fill="currentColor" strokeWidth={0} />
                  <strong>{formatNumber(item.imdbRating)}</strong>
                  {item.imdbVotes != null && <small>{new Intl.NumberFormat(undefined, { notation: "compact" }).format(item.imdbVotes)}</small>}
                </button>
              )}
              {item.tmdbRating != null && (
                <button className="rating tmdb" onClick={() => item.tmdbId && void openUrl(`https://www.themoviedb.org/movie/${item.tmdbId}`)} title="TMDb">
                  <span className="rating-source">TMDb</span>
                  <strong>{formatNumber(item.tmdbRating)}</strong>
                </button>
              )}
            </div>

            {detail?.tagline && <p className="detail-tagline">{detail.tagline}</p>}
            {detail?.overview && <p className="detail-overview">{detail.overview}</p>}

            <div className="detail-actions">
              {item.fileCount > 0 && (
                <button className="btn primary" onClick={() => void play()}>
                  <Play size={18} fill="currentColor" strokeWidth={0} />
                  {t("detail.play")}
                </button>
              )}
              {detail?.trailerKey && (
                <button className="btn" onClick={() => void openUrl(`https://www.youtube.com/watch?v=${detail.trailerKey}`)}>
                  <Clapperboard size={18} />
                  {t("detail.trailer")}
                </button>
              )}
              <button className={watched ? "btn toggled" : "btn"} onClick={() => void api.setWatched(item.id, !watched)}>
                <Check size={18} />
                {watched ? t("detail.watched") : t("detail.markWatched")}
              </button>
              <button className={item.watchlist ? "btn toggled" : "btn"} onClick={() => void api.setWatchlist(item.id, !item.watchlist)}>
                {item.watchlist ? <BookmarkCheck size={18} /> : <Bookmark size={18} />}
                {item.watchlist ? t("detail.inPending") : t("detail.addPending")}
              </button>
            </div>
            {error && <div className="detail-error">{error}</div>}

            <dl className="detail-facts">
              {directors.length > 0 && (
                <>
                  <dt>{t("detail.director", { count: directors.length })}</dt>
                  <dd>{directors.map((d) => d.name).join(", ")}</dd>
                </>
              )}
              {released && (
                <>
                  <dt>{t("detail.released")}</dt>
                  <dd>{formatDate(released, { dateStyle: "long" })}</dd>
                </>
              )}
              {item.countries.length > 0 && (
                <>
                  <dt>{t("detail.countries")}</dt>
                  <dd>{item.countries.map((c) => displayName(c, "region")).join(", ")}</dd>
                </>
              )}
              {item.languages.length > 0 && (
                <>
                  <dt>{t("detail.languages")}</dt>
                  <dd>{item.languages.map((c) => displayName(c, "language")).join(", ")}</dd>
                </>
              )}
              {item.lastWatched && (
                <>
                  <dt>{t("detail.lastWatched")}</dt>
                  <dd>
                    {formatDate(parseDbDate(item.lastWatched))}
                    {item.watchCount > 1 && ` · ${t("detail.timesWatched", { count: item.watchCount })}`}
                  </dd>
                </>
              )}
            </dl>
          </div>
        </div>

        {cast.length > 0 && (
          <section className="detail-section">
            <h3>{t("detail.cast")}</h3>
            <div className="cast-row">
              {cast.map((p) => (
                <div key={p.id} className="cast-card">
                  <div className="cast-photo">
                    {p.profileUrl ? <img src={tmdbImage(p.profileUrl)!} alt="" loading="lazy" /> : <span>{p.name[0]}</span>}
                  </div>
                  <strong>{p.name}</strong>
                  {p.character && <small>{p.character}</small>}
                </div>
              ))}
            </div>
          </section>
        )}

        {detail && detail.files.length > 0 && (
          <section className="detail-section">
            <h3>{t("detail.files")}</h3>
            <ul className="file-list">
              {detail.files.map((f) => (
                <li key={f.id}>
                  <button className="file-path" onClick={() => void revealItemInDir(f.path)} title={t("detail.showInFolder")}>
                    {f.path}
                  </button>
                  <span className="file-meta">
                    {formatSize(f.size)}
                    {f.hasSubs && ` · ${t("detail.subtitles")}`}
                  </span>
                  <button className="icon-btn small" onClick={() => void revealItemInDir(f.path)} title={t("detail.showInFolder")}>
                    <FolderOpen size={16} />
                  </button>
                </li>
              ))}
            </ul>
          </section>
        )}

        {item.tmdbId && (
          <div className="detail-footer">
            <button className="link-btn" onClick={() => void openUrl(`https://www.themoviedb.org/movie/${item.tmdbId}`)}>
              TMDb <ExternalLink size={12} />
            </button>
            {item.imdbId && (
              <button className="link-btn" onClick={() => void openUrl(`https://www.imdb.com/title/${item.imdbId}/`)}>
                IMDb <ExternalLink size={12} />
              </button>
            )}
          </div>
        )}
      </div>
    </div>
  );
}
