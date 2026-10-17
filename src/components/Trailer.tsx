import { openUrl } from "@tauri-apps/plugin-opener";
import { ExternalLink, X } from "lucide-react";
import { motion } from "motion/react";
import { useEffect } from "react";
import { useTranslation } from "react-i18next";
import { useStore } from "../store";

/** Tráiler de YouTube dentro de la app, con salida a YouTube por si el vídeo no permite incrustarse. */
export function Trailer({ videoKey }: { videoKey: string }) {
  const { t } = useTranslation();
  const close = () => useStore.getState().setTrailer(null);

  useEffect(() => {
    const onKey = (e: KeyboardEvent) => {
      if (e.key === "Escape") {
        e.stopImmediatePropagation();
        close();
      }
    };
    window.addEventListener("keydown", onKey, { capture: true });
    return () => window.removeEventListener("keydown", onKey, { capture: true });
  }, []);

  const src = `https://www.youtube-nocookie.com/embed/${videoKey}?autoplay=1&rel=0&modestbranding=1&playsinline=1`;
  return (
    <motion.div className="trailer-backdrop" onClick={close} initial={{ opacity: 0 }} animate={{ opacity: 1 }} exit={{ opacity: 0 }}>
      <motion.div
        className="trailer"
        onClick={(e) => e.stopPropagation()}
        initial={{ opacity: 0, scale: 0.94 }}
        animate={{ opacity: 1, scale: 1 }}
        exit={{ opacity: 0, scale: 0.96 }}
        transition={{ type: "spring", stiffness: 380, damping: 34 }}
      >
        <iframe
          src={src}
          title={t("detail.trailer")}
          allow="autoplay; encrypted-media; picture-in-picture; fullscreen"
          allowFullScreen
          referrerPolicy="strict-origin-when-cross-origin"
        />
        <div className="trailer-bar">
          <button className="link-btn" onClick={() => void openUrl(`https://www.youtube.com/watch?v=${videoKey}`)}>
            {t("trailer.openYoutube")} <ExternalLink size={12} />
          </button>
          <button className="icon-btn small" onClick={close} title={t("common.close")}>
            <X size={16} />
          </button>
        </div>
      </motion.div>
    </motion.div>
  );
}
