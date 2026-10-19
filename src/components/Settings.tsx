import { open } from "@tauri-apps/plugin-dialog";
import { openUrl } from "@tauri-apps/plugin-opener";
import { Check, Eye, EyeOff, FolderPlus, Loader2, RefreshCw, Trash2, X } from "lucide-react";
import { motion } from "motion/react";
import { useEffect, useRef, useState } from "react";
import { useTranslation } from "react-i18next";
import { rescanWhenFree } from "../actions";
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

function Section({ id, title, children }: { id: string; title: string; children: React.ReactNode }) {
  return (
    <section className="settings-section" data-section={id}>
      <h3>{title}</h3>
      {children}
    </section>
  );
}

/** Opciones de la ventana: icono de notificación, cerrar y minimizar a él, qué hacer al reproducir
 *  e iniciar con Windows. */
function WindowSettings() {
  const { t } = useTranslation();
  const settings = useStore((s) => s.settings);
  const [autostart, setAutostart] = useState<boolean | null>(null);
  useEffect(() => {
    void api.autostartEnabled().then(setAutostart);
  }, []);
  const save = (key: string, value: string) => useStore.getState().setSetting(key, value);
  const check = (key: string, label: string, disabled = false) => (
    <label className="check settings-check">
      <input type="checkbox" checked={settings[key] === "true"} disabled={disabled} onChange={(e) => void save(key, String(e.target.checked))} />
      {label}
    </label>
  );
  const toggleAutostart = async (on: boolean) => {
    setAutostart(on);
    try {
      await api.setAutostart(on);
    } catch {
      setAutostart(await api.autostartEnabled());
    }
  };
  return (
    <>
      <label className="check settings-check first">
        <input type="checkbox" checked={!!autostart} disabled={autostart == null} onChange={(e) => void toggleAutostart(e.target.checked)} />
        {t("settings.autostart")}
      </label>
      {check("startMinimized", t("settings.startMinimized"))}
      {check("closeToTray", t("settings.closeToTray"))}
      {check("minimizeToTray", t("settings.minimizeToTray"))}
      <label className="field">
        <span>{t("settings.playMinimize")}</span>
        <select value={settings.playMinimize ?? "none"} onChange={(e) => void save("playMinimize", e.target.value)}>
          {(["none", "minimize", "tray"] as const).map((v) => (
            <option key={v} value={v}>
              {t(`settings.playMinimize_${v}`)}
            </option>
          ))}
        </select>
      </label>
      <label className="check settings-check">
        <input
          type="checkbox"
          checked={settings.playRestore !== "false"}
          disabled={(settings.playMinimize ?? "none") === "none"}
          onChange={(e) => void save("playRestore", String(e.target.checked))}
        />
        {t("settings.playRestore")}
      </label>
    </>
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

  /** Comprueba la clave con TMDb y, si vale, la guarda (y se identifica la biblioteca con ella). */
  const saveKey = async (raw: string) => {
    const value = raw.trim();
    if (!value || value === settings.tmdbKey) return;
    setKeyState("checking");
    try {
      const ok = await api.validateTmdbKey(value);
      // Si mientras tanto se ha seguido escribiendo, esta respuesta ya no vale.
      if (value !== keyRef.current.trim()) return;
      setKeyState(ok ? "ok" : "bad");
      if (ok) {
        setSetting("tmdbKey", value);
        await api.setSetting("tmdbKey", value);
        if (libraries.length) void api.startScan();
      }
    } catch {
      if (value === keyRef.current.trim()) setKeyState("error");
    }
  };
  // Se guarda sola al dejar de escribir (o al pegarla), sin botón.
  const keyRef = useRef(key);
  keyRef.current = key;
  useEffect(() => {
    const id = window.setTimeout(() => void saveKey(key), 700);
    return () => window.clearTimeout(id);
  }, [key]);

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
  const metaLanguage = settings.metadataLanguage || "auto";
  const playerPath = settings.playerPath ?? "";
  const langName = (code: string) => new Intl.DisplayNames([i18n.language], { type: "language" }).of(code) ?? code;

  /** Cambia el idioma de la información y vuelve a descargar las fichas que estén en otro. */
  const changeMetaLanguage = async (value: string) => {
    setSetting("metadataLanguage", value);
    await api.setSetting("metadataLanguage", value);
    rescanWhenFree();
  };

  /** Idioma de la interfaz; en "Automático", la información pasa también al idioma nuevo. */
  const changeUiLanguage = async (value: string) => {
    setSetting("uiLanguage", value);
    await setLanguage(value);
    const auto = defaultMetadataLanguage(i18n.language);
    if (auto !== settings.metadataLanguageAuto) {
      setSetting("metadataLanguageAuto", auto);
      await api.setSetting("metadataLanguageAuto", auto);
      if (metaLanguage === "auto") rescanWhenFree();
    }
  };

  // Secciones a la izquierda: al pulsar una se va a ella; al desplazarse se marca la que se ve.
  const bodyRef = useRef<HTMLDivElement>(null);
  const sections = ["appearance", "window", "player", "libraries", "tmdb", "letterboxd", "about"];
  const [active, setActive] = useState(sections[0]);
  // Mientras se desplaza por un clic en la lista, no se va marcando cada sección por la que pasa.
  const jumping = useRef(false);
  useEffect(() => {
    const body = bodyRef.current;
    if (!body) return;
    const stop = () => (jumping.current = false);
    // Termina al acabar de desplazarse, o si el usuario toma el control (rueda, clic, teclado).
    const events = ["scrollend", "wheel", "pointerdown", "keydown"];
    events.forEach((e) => body.addEventListener(e, stop, { passive: true }));
    return () => events.forEach((e) => body.removeEventListener(e, stop));
  }, []);
  const goTo = (id: string) => {
    const body = bodyRef.current;
    const el = body?.querySelector<HTMLElement>(`[data-section="${id}"]`);
    if (!el || !body) return;
    setActive(id);
    const top = Math.min(el.offsetTop - body.offsetTop, body.scrollHeight - body.clientHeight);
    // Si ya está ahí no hay desplazamiento (ni `scrollend`).
    if (Math.abs(top - body.scrollTop) < 2) return;
    jumping.current = true;
    body.scrollTo({ top, behavior: "smooth" });
  };
  const onScroll = () => {
    const body = bodyRef.current;
    if (!body || jumping.current) return;
    // Al llegar al final, la última (puede ser tan corta que nunca llegue arriba).
    if (body.scrollTop + body.clientHeight >= body.scrollHeight - 4) return setActive(sections[sections.length - 1]);
    let current = sections[0];
    for (const el of body.querySelectorAll<HTMLElement>("[data-section]")) {
      if (el.offsetTop - body.offsetTop <= body.scrollTop + 40) current = el.dataset.section ?? current;
    }
    setActive(current);
  };

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

        <div className="settings-layout">
          <nav className="settings-nav">
            {sections.map((id) => (
              <button key={id} className={active === id ? "active" : undefined} onClick={() => goTo(id)}>
                {id === "letterboxd" ? "Letterboxd" : t(`settings.nav_${id}`)}
              </button>
            ))}
          </nav>
          <div className="modal-body" ref={bodyRef} onScroll={onScroll}>
            <Section id="appearance" title={t("settings.appearance")}>
              <label className="field">
                <span>{t("settings.language")}</span>
                <select
                  value={uiLanguage}
                  onChange={(e) => void changeUiLanguage(e.target.value)}
                >
                  <option value="system">{t("settings.system")}</option>
                  {available.map((l) => (
                    <option key={l.code} value={l.code}>
                      {l.name}
                    </option>
                  ))}
                </select>
              </label>
              <p className="hint">
                {t("settings.languageHint")}{" "}
                <button className="link-btn inline path-link" onClick={() => void api.openFolder("locales")}>
                  {`${info?.dataDir ?? ""}\\locales`}
                </button>
              </p>
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

            <Section id="window" title={t("settings.window")}>
              <WindowSettings />
            </Section>

            <Section id="player" title={t("settings.player")}>
              <label className="field">
                <span>{t("settings.playerApp")}</span>
                {/* Sin elegir (versiones anteriores: "Automático") se usa el integrado. */}
                <select
                  value={playerPath || (internal ? "internal" : (players[0]?.path ?? ""))}
                  onChange={(e) => (e.target.value === "__custom" ? void pickPlayer() : setSetting("playerPath", e.target.value))}
                >
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
              {/* Lo del reproductor de K-Movies, solo si es el elegido. */}
              {internal && (playerPath === "" || playerPath === "internal") && (
                <>
                  <h4 className="settings-sub">{t("settings.internalPlayer")}</h4>
                  <label className="check settings-check first">
                    <input
                      type="checkbox"
                      checked={settings.playerFullscreen === "true"}
                      onChange={(e) => setSetting("playerFullscreen", String(e.target.checked))}
                    />
                    {t("settings.playerFullscreen")}
                  </label>
                  <SubtitleSettings />
                </>
              )}
            </Section>

            <Section id="libraries" title={t("settings.libraries")}>
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

            <Section id="tmdb" title={t("settings.tmdb")}>
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
                    onKeyDown={(e) => e.key === "Enter" && void saveKey(key)}
                    placeholder={t("settings.tmdbPlaceholder")}
                    spellCheck={false}
                  />
                  {keyState === "checking" && <Loader2 size={16} className="spin key-status" />}
                  {keyState === "ok" && <Check size={16} className="key-status ok" />}
                  <button className="icon-btn small" onClick={() => setShowKey(!showKey)} title={t(showKey ? "settings.hide" : "settings.show")}>
                    {showKey ? <EyeOff size={16} /> : <Eye size={16} />}
                  </button>
                </div>
              </div>
              {keyState === "bad" && <p className="hint error">{t("settings.keyInvalid")}</p>}
              {keyState === "error" && <p className="hint error">{t("settings.keyNetwork")}</p>}
              {keyState === "ok" && <p className="hint ok">{t("settings.keyOk")}</p>}

              <label className="field">
                <span>{t("settings.metadataLanguage")}</span>
                <select value={metaLanguage} onChange={(e) => void changeMetaLanguage(e.target.value)}>
                  <option value="auto">{t("settings.metadataAuto", { language: langName(defaultMetadataLanguage(i18n.language)) })}</option>
                  {META_LANGUAGES.map((l) => (
                    <option key={l} value={l}>
                      {langName(l)}
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

            <Section id="letterboxd" title="Letterboxd">
              <LetterboxdSettings />
            </Section>

            <Section id="about" title={t("settings.about")}>
              <p className="hint">
                K-Movies {info?.version} · {t("settings.dataFolder")}:{" "}
                <button className="link-btn inline path-link" onClick={() => void api.openFolder("data")}>
                  {info?.dataDir}
                </button>
              </p>
              <p className="hint">{t("settings.attribution")}</p>
            </Section>
          </div>
        </div>
      </motion.div>
    </motion.div>
  );
}
