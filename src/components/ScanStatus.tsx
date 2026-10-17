import { AlertTriangle, Loader2, X } from "lucide-react";
import { AnimatePresence, motion } from "motion/react";
import { useTranslation } from "react-i18next";
import { useStore } from "../store";

/** Aviso flotante con el progreso del escaneo o el último error. */
export function ScanStatus() {
  const { t } = useTranslation();
  const scan = useStore((s) => s.scan);
  const error = useStore((s) => s.scanError);

  let content: React.ReactNode = null;
  if (scan) {
    const pct = scan.total ? Math.round((scan.current / scan.total) * 100) : null;
    content = (
      <motion.div key="scan" className="toast" initial={{ opacity: 0, y: 16 }} animate={{ opacity: 1, y: 0 }} exit={{ opacity: 0, y: 16 }}>
        <Loader2 size={16} className="spin" />
        <div className="toast-text">
          <strong>
            {t(`scan.${scan.phase}`)}
            {scan.phase === "matching" && scan.total > 0 && ` ${scan.current}/${scan.total}`}
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
  } else if (error) {
    content = (
      <motion.div key="error" className="toast error" initial={{ opacity: 0, y: 16 }} animate={{ opacity: 1, y: 0 }} exit={{ opacity: 0, y: 16 }}>
        <AlertTriangle size={16} />
        <div className="toast-text">
          <strong>{error === "tmdb_unauthorized" ? t("scan.badKey") : t("scan.failed")}</strong>
          {error !== "tmdb_unauthorized" && <small>{error}</small>}
        </div>
        {error === "tmdb_unauthorized" && (
          <button className="btn small" onClick={() => useStore.getState().openSettings(true)}>
            {t("toolbar.settings")}
          </button>
        )}
        <button className="icon-btn small" onClick={() => useStore.setState({ scanError: null })} title={t("common.close")}>
          <X size={14} />
        </button>
      </motion.div>
    );
  }
  return <AnimatePresence>{content}</AnimatePresence>;
}
