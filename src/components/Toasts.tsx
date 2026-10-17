import { AlertTriangle, ArrowRightLeft, Bookmark, Check, CheckCheck, Copy, Download, Eye, Loader2, Pause, Play, X } from "lucide-react";
import { AnimatePresence, motion } from "motion/react";
import { useState } from "react";
import { useTranslation } from "react-i18next";
import { errorText } from "../actions";
import { api, type NowPlaying, type ScanProgress, type TransferInfo } from "../api";
import { useStore } from "../store";
import { formatClock, formatSize } from "../util";
import { Stars } from "./Stars";

const enter = { initial: { opacity: 0, y: 16, scale: 0.98 }, animate: { opacity: 1, y: 0, scale: 1 }, exit: { opacity: 0, x: 40 } };

function ScanToast({ scan }: { scan: ScanProgress }) {
  const { t } = useTranslation();
  const pct = scan.total ? Math.round((scan.current / scan.total) * 100) : null;
  return (
    <motion.div layout key="scan" className="toast" {...enter}>
      <Loader2 size={16} className="spin" />
      <div className="toast-text">
        <strong>
          {t(`scan.${scan.phase}`)}
          {(scan.phase === "matching" || scan.phase === "refreshing" || scan.phase === "letterboxd") &&
            scan.total > 0 &&
            ` ${scan.current}/${scan.total}`}
        </strong>
        {scan.message && scan.phase !== "scanning" && <small>{scan.message}</small>}
      </div>
      {pct != null && (
        <span className="toast-progress">
          <span style={{ width: `${pct}%` }} />
        </span>
      )}
    </motion.div>
  );
}

const KIND_ICON = { import: Download, copy: Copy, move: ArrowRightLeft };

function TransferToast({ tr }: { tr: TransferInfo }) {
  const { t } = useTranslation();
  const Icon = tr.state === "done" ? Check : tr.state === "error" ? AlertTriangle : KIND_ICON[tr.kind];
  const pct = tr.total ? Math.min(100, Math.round((tr.done / tr.total) * 100)) : 0;
  return (
    <motion.div layout className={tr.state === "error" ? "toast error" : "toast"} {...enter}>
      <Icon size={16} />
      <div className="toast-text">
        <strong>
          {tr.state === "done" ? t(`transfer.done_${tr.kind}`) : t(`transfer.${tr.kind}`)}
          {tr.state === "running" && ` · ${pct}%`}
          {tr.state === "queued" && ` · ${t("transfer.queued")}`}
        </strong>
        <small title={tr.dest}>{tr.name}</small>
        {tr.state === "running" && (
          <small>
            {formatSize(tr.done)} / {formatSize(tr.total)}
          </small>
        )}
        {tr.state === "error" && <small className="toast-error">{errorText(tr.error)}</small>}
      </div>
      {(tr.state === "running" || tr.state === "queued") && (
        <button className="icon-btn small" onClick={() => void api.cancelTransfer(tr.id)} title={t("common.cancel")}>
          <X size={14} />
        </button>
      )}
      {tr.state === "error" && (
        <button className="icon-btn small" onClick={() => void api.dismissTransfers(tr.id)} title={t("common.close")}>
          <X size={14} />
        </button>
      )}
      {(tr.state === "running" || tr.state === "done") && (
        <span className="toast-progress">
          <span style={{ width: `${tr.state === "done" ? 100 : pct}%` }} />
        </span>
      )}
    </motion.div>
  );
}

/** Lo que se está viendo: por dónde va (con VLC) o solo que está abierta (otros reproductores). */
function PlayingToast({ p }: { p: NowPlaying }) {
  const { t } = useTranslation();
  const pct = p.tracked && p.position != null && p.duration ? Math.min(100, (p.position / p.duration) * 100) : null;
  const Icon = p.watched ? CheckCheck : p.paused ? Pause : Play;
  const status = !p.tracked
    ? t("playback.openIn", { player: p.player })
    : p.watched
      ? t("playback.watched")
      : t(p.paused ? "playback.paused" : "playback.playing");
  return (
    <motion.div layout className="toast clickable" {...enter} onClick={() => useStore.getState().select(p.itemId)}>
      <Icon size={16} fill={p.paused || p.watched ? "none" : "currentColor"} strokeWidth={p.paused || p.watched ? 2 : 0} />
      <div className="toast-text">
        <strong>{p.title}</strong>
        <small>
          {status}
          {pct != null && ` · ${formatClock(p.position)} / ${formatClock(p.duration)}`}
        </small>
      </div>
      {pct != null && (
        <span className="toast-progress">
          <span style={{ width: `${pct}%` }} />
        </span>
      )}
    </motion.div>
  );
}

/** Nota para lo que se acaba de ver, desde el propio aviso (`historyId`: el visionado nuevo). */
function RateWatched({ historyId }: { historyId: number }) {
  const { t } = useTranslation();
  const [value, setValue] = useState<number | null>(null);
  const change = (v: number | null) => {
    setValue(v);
    api.setHistoryRating(historyId, v).catch(() => setValue(null));
  };
  return (
    <span className="toast-rate">
      <small>{t("rating.rateIt")}</small>
      <Stars value={value} onChange={change} size={17} />
    </span>
  );
}

const NOTICE_ICON = { pendingAvailable: Bookmark, alreadyWatched: Eye, markedWatched: CheckCheck };
const isError = (kind: string) => kind === "transferError" || kind === "playError";

/** Pila de avisos de la esquina: escaneo, reproducción, transferencias, errores y notificaciones. */
export function Toasts() {
  const { t } = useTranslation();
  const transfers = useStore((s) => s.transfers);
  const playing = useStore((s) => s.playing);
  const notices = useStore((s) => s.notices);
  const error = useStore((s) => s.scanError);
  const scan = useStore((s) => s.scan);
  const { dropNotice, select, openSettings } = useStore.getState();

  return (
    <div className="toasts">
      <AnimatePresence initial={false}>
        {notices
          .filter((n) => n.kind !== "transferDone" && !(n.kind === "transferError" && n.title))
          .map((n) => {
            const Icon = NOTICE_ICON[n.kind as keyof typeof NOTICE_ICON] ?? AlertTriangle;
            const error = isError(n.kind);
            return (
              <motion.div layout key={n.id} className={error ? "toast error" : "toast clickable"} {...enter} onClick={() => n.itemId && select(n.itemId)}>
                <Icon size={16} />
                <div className="toast-text">
                  <strong>
                    {n.kind === "transferError"
                      ? t("transfer.failed")
                      : n.kind === "playError"
                        ? t("playback.failed")
                        : t(`notice.${n.kind}`, { title: n.title })}
                  </strong>
                  {error && <small className="toast-error">{errorText(n.detail)}</small>}
                  {n.kind === "markedWatched" && n.detail && <RateWatched historyId={Number(n.detail)} />}
                </div>
                <button
                  className="icon-btn small"
                  onClick={(e) => {
                    e.stopPropagation();
                    dropNotice(n.id);
                  }}
                  title={t("common.close")}
                >
                  <X size={14} />
                </button>
              </motion.div>
            );
          })}
        {playing.map((p) => (
          <PlayingToast key={`p${p.itemId}`} p={p} />
        ))}
        {transfers.filter((tr) => tr.state !== "cancelled").map((tr) => (
          <TransferToast key={`t${tr.id}`} tr={tr} />
        ))}
        {error && (
          <motion.div layout key="error" className="toast error" {...enter}>
            <AlertTriangle size={16} />
            <div className="toast-text">
              <strong>{error === "tmdb_unauthorized" ? t("scan.badKey") : t("scan.failed")}</strong>
              {error !== "tmdb_unauthorized" && <small>{errorText(error)}</small>}
            </div>
            {error === "tmdb_unauthorized" && (
              <button className="btn small" onClick={() => openSettings(true)}>
                {t("toolbar.settings")}
              </button>
            )}
            <button className="icon-btn small" onClick={() => useStore.setState({ scanError: null })} title={t("common.close")}>
              <X size={14} />
            </button>
          </motion.div>
        )}
        {scan && <ScanToast key="scan" scan={scan} />}
      </AnimatePresence>
    </div>
  );
}
