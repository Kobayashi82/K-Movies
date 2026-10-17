import { useMemo } from "react";
import { useTranslation } from "react-i18next";
import { fileUrl } from "../api";
import { useStore } from "../store";
import { displayName } from "../util";

/** Idiomas que se pueden preferir (los mismos que entiende el backend en `lang_tags`). */
export const PLAYER_LANGS = ["es", "en", "fr", "de", "it", "pt", "ca", "ja", "ko", "zh", "ru"];

export const SUB_COLORS = [
  { key: "white", value: "#FFFFFF" },
  { key: "yellow", value: "#FFD83D" },
];

const capitalize = (s: string) => s.charAt(0).toLocaleUpperCase() + s.slice(1);

/**
 * Vista previa de los subtítulos con las mismas medidas que usa mpv: todo va referido a un vídeo
 * de 720 líneas (letra de 38, borde de 3, margen inferior de 22), aquí en unidades del alto del recuadro.
 */
function Preview({ scale, pos, background, color }: { scale: number; pos: number; background: boolean; color: string }) {
  const { t } = useTranslation();
  const items = useStore((s) => s.items);
  const backdrop = useMemo(() => fileUrl(items.find((i) => i.backdrop)?.backdrop ?? null), [items]);
  const unit = (n: number) => `${((n / 720) * 100).toFixed(3)}cqh`;
  return (
    <div className="sub-preview" style={backdrop ? { backgroundImage: `url("${backdrop}")` } : undefined}>
      <span
        className={background ? "sub-preview-text boxed" : "sub-preview-text"}
        style={{
          bottom: `calc(${100 - pos}cqh + ${unit(22)})`,
          fontSize: unit(38 * scale),
          color,
          ["--stroke" as string]: unit(3 * scale * 2),
          ["--pad" as string]: unit(3 * scale),
        }}
      >
        {t("settings.subPreview")}
      </span>
    </div>
  );
}

/** Ajustes del reproductor integrado: idiomas preferidos y aspecto de los subtítulos. */
export function SubtitleSettings() {
  const { t } = useTranslation();
  const settings = useStore((s) => s.settings);
  const setSetting = useStore((s) => s.setSetting);

  const audioLang = settings.audioLang ?? "";
  const subLang = settings.subLang ?? "none";
  const onlyIfNeeded = settings.subsOnlyIfNeeded !== "false";
  const scale = Number(settings.subScale) || 1;
  const pos = settings.subPos != null ? Number(settings.subPos) : 100;
  const background = settings.subBackground !== "false";
  const color = SUB_COLORS.some((c) => c.value === settings.subColor) ? settings.subColor : "#FFFFFF";
  const langName = (code: string) => capitalize(displayName(code, "language"));

  return (
    <>
      <label className="field">
        <span>{t("settings.audioLang")}</span>
        <select value={audioLang} onChange={(e) => setSetting("audioLang", e.target.value)}>
          <option value="">{t("settings.audioOriginal")}</option>
          {PLAYER_LANGS.map((l) => (
            <option key={l} value={l}>
              {langName(l)}
            </option>
          ))}
        </select>
      </label>
      <label className="field">
        <span>{t("settings.subLang")}</span>
        <select value={subLang} onChange={(e) => setSetting("subLang", e.target.value)}>
          <option value="none">{t("settings.subNone")}</option>
          {PLAYER_LANGS.map((l) => (
            <option key={l} value={l}>
              {langName(l)}
            </option>
          ))}
        </select>
      </label>
      <label className="check settings-check">
        <input
          type="checkbox"
          checked={onlyIfNeeded}
          disabled={subLang === "none"}
          onChange={(e) => setSetting("subsOnlyIfNeeded", String(e.target.checked))}
        />
        {t("settings.subsOnlyIfNeeded")}
      </label>
      <p className="hint">{t("settings.langHint")}</p>

      <h4 className="settings-sub">{t("settings.subStyle")}</h4>
      <Preview scale={scale} pos={pos} background={background} color={color} />
      <label className="field">
        <span>{t("settings.subSize")}</span>
        <span className="field-range">
          <input
            type="range"
            min={50}
            max={250}
            step={10}
            value={Math.round(scale * 100)}
            onChange={(e) => setSetting("subScale", String(Number(e.target.value) / 100))}
            style={{ ["--fill" as string]: `${((scale * 100 - 50) / 200) * 100}%` }}
          />
          <output>{Math.round(scale * 100)} %</output>
        </span>
      </label>
      <label className="field">
        <span>{t("settings.subPosition")}</span>
        <span className="field-range">
          <input
            type="range"
            min={60}
            max={100}
            step={1}
            value={pos}
            onChange={(e) => setSetting("subPos", e.target.value)}
            style={{ ["--fill" as string]: `${((pos - 60) / 40) * 100}%` }}
          />
          <output>{pos >= 100 ? t("player.bottom") : `${pos}`}</output>
        </span>
      </label>
      <div className="field">
        <span>{t("settings.subColor")}</span>
        <div className="segmented">
          {SUB_COLORS.map((c) => (
            <button key={c.key} className={color === c.value ? "active" : undefined} onClick={() => setSetting("subColor", c.value)}>
              <span className="color-dot" style={{ background: c.value }} />
              {t(`settings.subColor_${c.key}`)}
            </button>
          ))}
        </div>
      </div>
      <label className="check settings-check">
        <input type="checkbox" checked={background} onChange={(e) => setSetting("subBackground", String(e.target.checked))} />
        {t("player.subBackground")}
      </label>
      <label className="check settings-check">
        <input
          type="checkbox"
          checked={settings.subAssOverride === "true"}
          onChange={(e) => setSetting("subAssOverride", String(e.target.checked))}
        />
        {t("settings.subAssOverride")}
      </label>
      <p className="hint">{t("settings.subStyleHint")}</p>
    </>
  );
}
