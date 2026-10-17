import { open } from "@tauri-apps/plugin-dialog";
import { openUrl } from "@tauri-apps/plugin-opener";
import { Check, Eye, EyeOff, FolderPlus, Loader2, RefreshCw, Trash2, X } from "lucide-react";
import { motion } from "motion/react";
import { useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import { api, parseDbDate, type Player } from "../api";
import { available, setLanguage } from "../i18n";
import { useStore } from "../store";
import { formatDate } from "../util";
import { LetterboxdSettings } from "./LetterboxdSettings";
import { PLAYER_LANGS, SubtitleSettings } from "./SubtitleSettings";

/** Idiomas preferidos del reproductor la primera vez: los de la interfaz, con subtítulos solo si hacen falta. */
export function defaultPlayerLanguages(uiLanguage: string): Record<string, string> {
  const lang = PLAYER_LANGS.includes(uiLanguage) ? uiLanguage : "";
  return { audioLang: lang, subLang: lang || "none", subsOnlyIfNeeded: "true" };
}

const META_LANGUAGES = ["es-ES", "es-MX", "en-US", "en-GB", "fr-FR", "de-DE", "it-IT", "pt-PT", "pt-BR", "ja-JP", "ko-KR"];

export function defaultMetadataLanguage(uiLanguage: string): string {
  return META_LANGUAGES.find((l) => l.startsWith(uiLanguage)) ?? "en-US";
}

function Section({ title, children }: { title: string; children: React.ReactNode }) {
  return (
    <section className="settings-section">
      <h3>{title}</h3>
      {children}
    </section>
  );
}

export function Settings() {
  const { t, i18n } = useTranslation();
  const settings = useStore((s) => s.settings);
  const libraries = useStore((s) => s.libraries);
  const info = useStore((s) => s.info);
  const busy = useStore((s) => s.scan != null);
  const { setSetting, refreshLibraries, openSettings } = useStore.getState();

  const [key, setKey] = useState(settings.tmdbKey ?? "");
  const [showKey, setShowKey] = useState(false);
  const [keyState, setKeyState] = useState<"idle" | "checking" | "ok" | "bad" | "error">("idle");
  const [players, setPlayers] = useState<Player[]>([]);
  const [internal, setInternal] = useState(false);

  useEffect(() => {
    void api.detectPlayers().then(setPlayers);
    void api.internalPlayerAvailable().then(setInternal);
    // El reproductor (otra ventana) también guarda ajustes: volumen y estilo de los subtítulos.
    void api.getSettings().then((s) => useStore.setState({ settings: s }));
    const onKey = (e: KeyboardEvent) => e.key === "Escape" && openSettings(false);
    window.addEventListener("keydown", onKey);
    return () => window.removeEventListener("keydown", onKey);
  }, [openSettings]);

  const saveKey = async () => {
    const value = key.trim();
    setKeyState("checking");
    try {
      const ok = await api.validateTmdbKey(value);
      setKeyState(ok ? "ok" : "bad");
      if (ok) {
        const changed = value !== settings.tmdbKey;
        setSetting("tmdbKey", value);
        if (changed && libraries.length) void api.startScan();
      }
    } catch {
      setKeyState("error");
    }
  };

  const addFolder = async () => {
    const dir = await open({ directory: true, multiple: false, title: t("settings.addFolder") });
    if (typeof dir !== "string") return;
    await api.addLibrary(dir);
    await refreshLibraries();
    void api.startScan();
  };

  const removeFolder = async (id: number) => {
    await api.removeLibrary(id);
    await refreshLibraries();
  };

  const pickPlayer = async () => {
    const file = await open({ multiple: false, title: t("settings.choosePlayer"), filters: [{ name: "exe", extensions: ["exe"] }] });
    if (typeof file === "string") setSetting("playerPath", file);
  };

  const uiLanguage = settings.uiLanguage ?? "system";
  const theme = settings.theme ?? "system";
  const metaLanguage = settings.metadataLanguage ?? defaultMetadataLanguage(i18n.language);
  const playerPath = settings.playerPath ?? "";

  return (
    <motion.div className="modal-backdrop" onClick={() => openSettings(false)} initial={{ opacity: 0 }} animate={{ opacity: 1 }} exit={{ opacity: 0 }}>
      <motion.div
        className="modal settings"
        role="dialog"
        aria-modal="true"
        aria-label={t("settings.title")}
        onClick={(e) => e.stopPropagation()}
        initial={{ opacity: 0, y: 24, scale: 0.97 }}
        animate={{ opacity: 1, y: 0, scale: 1 }}
        exit={{ opacity: 0, y: 16, scale: 0.98 }}
        transition={{ type: "spring", stiffness: 420, damping: 34 }}
      >
        <header className="modal-header">
          <h2>{t("settings.title")}</h2>
          <button className="icon-btn" onClick={() => openSettings(false)} title={t("common.close")}>
            <X size={18} />
          </button>
        </header>

        <div className="modal-body">
          <Section title={t("settings.libraries")}>
            <ul className="library-list">
              {libraries.map((l) => (
                <li key={l.id}>
                  <div>
                    <div className="library-path">{l.path}</div>
                    <small>
                      {t("settings.filesCount", { count: l.fileCount })}
                      {l.lastScan && ` · ${t("settings.lastScan", { date: formatDate(parseDbDate(l.lastScan), { dateStyle: "short", timeStyle: "short" }) })}`}
                    </small>
                  </div>
                  <button className="icon-btn small danger" onClick={() => void removeFolder(l.id)} title={t("settings.removeFolder")}>
                    <Trash2 size={16} />
                  </button>
                </li>
              ))}
            </ul>
            <button className="btn" onClick={() => void addFolder()}>
              <FolderPlus size={18} />
              {t("settings.addFolder")}
            </button>
            <label className="check settings-check">
              <input
                type="checkbox"
                checked={(settings.importDeleteOriginal ?? "true") === "true"}
                onChange={(e) => setSetting("importDeleteOriginal", String(e.target.checked))}
              />
              {t("settings.importDeleteOriginal")}
            </label>
            <p className="hint">{t("settings.importHint")}</p>
          </Section>

          <Section title={t("settings.tmdb")}>
            <p className="hint">
              {t("settings.tmdbHint")}{" "}
              <button className="link-btn inline" onClick={() => void openUrl("https://www.themoviedb.org/settings/api")}>
                themoviedb.org/settings/api
              </button>
            </p>
            <div className="input-row">
              <div className="input-with-btn">
                <input
                  type={showKey ? "text" : "password"}
                  value={key}
                  onChange={(e) => {
                    setKey(e.target.value);
                    setKeyState("idle");
                  }}
                  onKeyDown={(e) => e.key === "Enter" && void saveKey()}
                  placeholder={t("settings.tmdbPlaceholder")}
                  spellCheck={false}
                />
                <button className="icon-btn small" onClick={() => setShowKey(!showKey)} title={t(showKey ? "settings.hide" : "settings.show")}>
                  {showKey ? <EyeOff size={16} /> : <Eye size={16} />}
                </button>
              </div>
              <button className="btn primary" onClick={() => void saveKey()} disabled={!key.trim() || keyState === "checking"}>
                {keyState === "checking" ? <Loader2 size={16} className="spin" /> : keyState === "ok" ? <Check size={16} /> : null}
                {t("settings.save")}
              </button>
            </div>
            {keyState === "bad" && <p className="hint error">{t("settings.keyInvalid")}</p>}
            {keyState === "error" && <p className="hint error">{t("settings.keyNetwork")}</p>}
            {keyState === "ok" && <p className="hint ok">{t("settings.keyOk")}</p>}

            <label className="field">
              <span>{t("settings.metadataLanguage")}</span>
              <select value={metaLanguage} onChange={(e) => setSetting("metadataLanguage", e.target.value)}>
                {META_LANGUAGES.map((l) => (
                  <option key={l} value={l}>
                    {new Intl.DisplayNames([i18n.language], { type: "language" }).of(l)}
                  </option>
                ))}
              </select>
            </label>
            <p className="hint">{t("settings.metadataLanguageHint")}</p>
            <button className="btn" onClick={() => void api.refreshAll()} disabled={busy || !settings.tmdbKey}>
              <RefreshCw size={16} className={busy ? "spin" : undefined} />
              {t("settings.refreshAll")}
            </button>
          </Section>

          <Section title={t("settings.player")}>
            <label className="field">
              <span>{t("settings.playerApp")}</span>
              <select
                value={players.some((p) => p.path === playerPath) || !playerPath || playerPath === "internal" ? playerPath : "__custom"}
                onChange={(e) => (e.target.value === "__custom" ? void pickPlayer() : setSetting("playerPath", e.target.value))}
              >
                <option value="">{t("settings.playerAuto")}</option>
                {internal && <option value="internal">{t("settings.playerInternal")}</option>}
                {players.map((p) => (
                  <option key={p.path} value={p.path}>
                    {p.name}
                  </option>
                ))}
                {playerPath && playerPath !== "internal" && !players.some((p) => p.path === playerPath) && (
                  <option value={playerPath}>{playerPath}</option>
                )}
                <option value="__custom">{t("settings.playerOther")}</option>
              </select>
            </label>
            <label className="field">
              <span>{t("settings.watchedThreshold")}</span>
              <select value={settings.watchedThreshold ?? "90"} onChange={(e) => setSetting("watchedThreshold", e.target.value)}>
                {["75", "80", "85", "90", "95", "98"].map((v) => (
                  <option key={v} value={v}>
                    {v} %
                  </option>
                ))}
              </select>
            </label>
            <p className="hint">{t("settings.trackingHint")}</p>
          </Section>

          {internal && (playerPath === "" || playerPath === "internal") && (
            <Section title={t("settings.internalPlayer")}>
              <SubtitleSettings />
            </Section>
          )}

          <Section title="Letterboxd">
            <LetterboxdSettings />
          </Section>

          <Section title={t("settings.appearance")}>
            <label className="field">
              <span>{t("settings.language")}</span>
              <select
                value={uiLanguage}
                onChange={(e) => {
                  setSetting("uiLanguage", e.target.value);
                  void setLanguage(e.target.value);
                }}
              >
                <option value="system">{t("settings.system")}</option>
                {available.map((l) => (
                  <option key={l.code} value={l.code}>
                    {l.name}
                  </option>
                ))}
              </select>
            </label>
            <p className="hint">{t("settings.languageHint", { path: `${info?.dataDir ?? ""}\\locales` })}</p>
            <div className="field">
              <span>{t("settings.theme")}</span>
              <div className="segmented">
                {(["system", "dark", "light"] as const).map((th) => (
                  <button key={th} className={theme === th ? "active" : undefined} onClick={() => setSetting("theme", th)}>
                    {t(`settings.theme_${th}`)}
                  </button>
                ))}
              </div>
            </div>
            <div className="field">
              <span>{t("settings.posterRating")}</span>
              <div className="segmented">
                {(["imdb", "tmdb", "both", "none"] as const).map((r) => (
                  <button key={r} className={(settings.posterRating ?? "imdb") === r ? "active" : undefined} onClick={() => setSetting("posterRating", r)}>
                    {t(`settings.posterRating_${r}`)}
                  </button>
                ))}
              </div>
            </div>
            <label className="check settings-check">
              <input type="checkbox" checked={settings.posterTitle !== "false"} onChange={(e) => setSetting("posterTitle", String(e.target.checked))} />
              {t("settings.posterTitle")}
            </label>
            <label className="check settings-check">
              <input
                type="checkbox"
                checked={settings.posterYear !== "false" && settings.posterYear !== "none"}
                onChange={(e) => setSetting("posterYear", String(e.target.checked))}
              />
              {t("settings.posterYear")}
            </label>
          </Section>

          <Section title={t("settings.about")}>
            <p className="hint">
              K-Movies {info?.version} · {t("settings.dataFolder")}: <code>{info?.dataDir}</code>
            </p>
            <p className="hint">{t("settings.attribution")}</p>
          </Section>
        </div>
      </motion.div>
    </motion.div>
  );
}
