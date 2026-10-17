import { open } from "@tauri-apps/plugin-dialog";
import { Check, FolderPlus, KeyRound } from "lucide-react";
import { motion } from "motion/react";
import { useTranslation } from "react-i18next";
import { api } from "../api";
import { useStore } from "../store";
import { Logo } from "./Logo";

/** Primer arranque: clave de TMDb y carpeta de películas. */
export function Welcome() {
  const { t } = useTranslation();
  const hasKey = useStore((s) => !!s.settings.tmdbKey);

  const addFolder = async () => {
    const dir = await open({ directory: true, multiple: false, title: t("settings.addFolder") });
    if (typeof dir !== "string") return;
    await api.addLibrary(dir);
    await useStore.getState().refreshLibraries();
    void api.startScan();
  };

  return (
    <motion.div className="welcome" initial={{ opacity: 0, y: 12 }} animate={{ opacity: 1, y: 0 }} transition={{ duration: 0.5 }}>
      <Logo size={64} />
      <h1>{t("welcome.title")}</h1>
      <p>{t("welcome.subtitle")}</p>
      <ol className="welcome-steps">
        <li className={hasKey ? "done" : undefined}>
          <span className="step-icon">{hasKey ? <Check size={18} /> : <KeyRound size={18} />}</span>
          <div>
            <strong>{t("welcome.step1")}</strong>
            <small>{t("welcome.step1Hint")}</small>
          </div>
          {!hasKey && (
            <button className="btn" onClick={() => useStore.getState().openSettings(true)}>
              {t("welcome.addKey")}
            </button>
          )}
        </li>
        <li>
          <span className="step-icon">
            <FolderPlus size={18} />
          </span>
          <div>
            <strong>{t("welcome.step2")}</strong>
            <small>{t("welcome.step2Hint")}</small>
          </div>
          <button className="btn primary" onClick={() => void addFolder()}>
            {t("settings.addFolder")}
          </button>
        </li>
      </ol>
    </motion.div>
  );
}
