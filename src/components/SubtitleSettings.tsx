import { RotateCcw } from "lucide-react";
import { useMemo } from "react";
import { useTranslation } from "react-i18next";
import { fileUrl } from "../api";
import { useStore } from "../store";
import { displayName, formatNumber } from "../util";

/** Idiomas que se pueden preferir (los mismos que entiende el backend en `lang_tags`). */
export const PLAYER_LANGS = ["es", "en", "fr", "de", "it", "pt", "ca", "ja", "ko", "zh", "ru"];

const SUB_COLORS = [
  { key: "white", value: "#FFFFFF" },
  { key: "yellow", value: "#FFD83D" },
  { key: "gray", value: "#D9D9D9" },
];

/** Fuentes que trae Windows. Vacío = la de mpv (sin serifa del sistema, Arial en la práctica). */
const SUB_FONTS = ["", "Arial", "Segoe UI", "Verdana", "Tahoma", "Calibri", "Trebuchet MS", "Georgia"];

/** Ajustes del aspecto de los subtítulos y sus valores por defecto (los mismos que `SubStyle` en Rust). */
const STYLE_DEFAULTS: Record<string, string> = {
  subScale: "1",
  subHeight: "3",
  subFont: "",
  subBold: "false",
  subColor: "#FFFFFF",
  subBackground: "true",
  subBackOpacity: "65",
  subBorder: "3",
  subShadow: "0",
  subAssOverride: "false",
};

const capitalize = (s: string) => s.charAt(0).toLocaleUpperCase() + s.slice(1);

interface Style {
  scale: number;
  height: number;
  font: string;
  bold: boolean;
  color: string;
  background: boolean;
  backOpacity: number;
  border: number;
  shadow: number;
}

/**
 * Vista previa con las mismas medidas que usa mpv: todo va referido a un vídeo de 720 líneas
 * (letra de 38 y borde de 3 por defecto), aquí en unidades del alto del recuadro.
 */
function Preview({ st }: { st: Style }) {
  const { t } = useTranslation();
  const items = useStore((s) => s.items);
  const backdrop = useMemo(() => fileUrl(items.find((i) => i.backdrop)?.backdrop ?? null), [items]);
  const unit = (n: number) => `${((n / 720) * 100).toFixed(3)}cqh`;
  const border = st.border * st.scale;
  const shadow = st.shadow * st.scale;
  return (
    <div className="sub-preview" style={backdrop ? { backgroundImage: `url("${backdrop}")` } : undefined}>
      <span
        className={st.background ? "sub-preview-text boxed" : "sub-preview-text"}
        style={{
          bottom: `${st.height}cqh`,
          fontSize: unit(38 * st.scale),
          fontFamily: `${st.font ? `"${st.font}", ` : ""}Arial, sans-serif`,
          fontWeight: st.bold ? 700 : 400,
          color: st.color,
          background: st.background ? `rgba(0, 0, 0, ${st.backOpacity / 100})` : undefined,
          textShadow: !st.background && shadow ? `${unit(shadow)} ${unit(shadow)} 0 rgba(0, 0, 0, 0.75)` : undefined,
          ["--stroke" as string]: unit(border * 2),
          ["--pad" as string]: unit(border),
        }}
      >
        {t("settings.subPreview")}
      </span>
    </div>
  );
}

function Range(props: { label: string; value: number; min: number; max: number; step: number; text: string; onChange: (v: number) => void; disabled?: boolean }) {
  const fill = ((props.value - props.min) / (props.max - props.min)) * 100;
  return (
    <label className={props.disabled ? "field disabled" : "field"}>
      <span>{props.label}</span>
      <span className="field-range">
        <input
          type="range"
          min={props.min}
          max={props.max}
          step={props.step}
          value={props.value}
          disabled={props.disabled}
          onChange={(e) => props.onChange(Number(e.target.value))}
          style={{ ["--fill" as string]: `${fill}%` }}
        />
        <output>{props.text}</output>
      </span>
    </label>
  );
}

/** Ajustes del reproductor integrado: idiomas preferidos y aspecto de los subtítulos. */
export function SubtitleSettings() {
  const { t } = useTranslation();
  const settings = useStore((s) => s.settings);
  const setSetting = useStore((s) => s.setSetting);
  const get = (k: string) => settings[k] ?? STYLE_DEFAULTS[k];
  const num = (k: string) => {
    const v = Number(get(k));
    return Number.isFinite(v) ? v : Number(STYLE_DEFAULTS[k]);
  };

  const audioLang = settings.audioLang ?? "";
  const subLang = settings.subLang ?? "none";
  const onlyIfNeeded = settings.subsOnlyIfNeeded !== "false";
  const st: Style = {
    scale: num("subScale"),
    height: num("subHeight"),
    font: get("subFont"),
    bold: get("subBold") === "true",
    color: SUB_COLORS.some((c) => c.value === get("subColor")) ? get("subColor") : "#FFFFFF",
    background: get("subBackground") !== "false",
    backOpacity: num("subBackOpacity"),
    border: num("subBorder"),
    shadow: num("subShadow"),
  };
  const langName = (code: string) => capitalize(displayName(code, "language"));
  const isDefault = Object.entries(STYLE_DEFAULTS).every(([k, v]) => (settings[k] ?? v) === v || Number(settings[k]) === Number(v));
  const reset = () => Object.entries(STYLE_DEFAULTS).forEach(([k, v]) => setSetting(k, v));

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

      <div className="settings-sub-row">
        <h4 className="settings-sub">{t("settings.subStyle")}</h4>
        <button className="link-btn" onClick={reset} disabled={isDefault}>
          <RotateCcw size={13} />
          {t("settings.subReset")}
        </button>
      </div>
      <Preview st={st} />

      <Range
        label={t("settings.subSize")}
        value={Math.round(st.scale * 100)}
        min={30}
        max={300}
        step={5}
        text={`${Math.round(st.scale * 100)} %`}
        onChange={(v) => setSetting("subScale", String(v / 100))}
      />
      <Range
        label={t("settings.subHeight")}
        value={st.height}
        min={0}
        max={40}
        step={1}
        text={`${st.height} %`}
        onChange={(v) => setSetting("subHeight", String(v))}
      />
      <label className="field">
        <span>{t("settings.subFont")}</span>
        <select value={st.font} onChange={(e) => setSetting("subFont", e.target.value)}>
          {SUB_FONTS.map((f) => (
            <option key={f} value={f}>
              {f || t("settings.subFontDefault")}
            </option>
          ))}
        </select>
      </label>
      <div className="field">
        <span>{t("settings.subColor")}</span>
        <div className="segmented">
          {SUB_COLORS.map((c) => (
            <button key={c.key} className={st.color === c.value ? "active" : undefined} onClick={() => setSetting("subColor", c.value)}>
              <span className="color-dot" style={{ background: c.value }} />
              {t(`settings.subColor_${c.key}`)}
            </button>
          ))}
        </div>
      </div>
      <label className="check settings-check">
        <input type="checkbox" checked={st.bold} onChange={(e) => setSetting("subBold", String(e.target.checked))} />
        {t("settings.subBold")}
      </label>
      <label className="check settings-check">
        <input type="checkbox" checked={st.background} onChange={(e) => setSetting("subBackground", String(e.target.checked))} />
        {t("player.subBackground")}
      </label>
      <Range
        label={t("settings.subBackOpacity")}
        value={st.backOpacity}
        min={0}
        max={100}
        step={5}
        text={`${st.backOpacity} %`}
        disabled={!st.background}
        onChange={(v) => setSetting("subBackOpacity", String(v))}
      />
      <Range
        label={t(st.background ? "settings.subPadding" : "settings.subBorder")}
        value={st.border}
        min={0}
        max={8}
        step={0.5}
        text={formatNumber(st.border)}
        onChange={(v) => setSetting("subBorder", String(v))}
      />
      <Range
        label={t("settings.subShadow")}
        value={st.shadow}
        min={0}
        max={6}
        step={0.5}
        text={formatNumber(st.shadow)}
        disabled={st.background}
        onChange={(v) => setSetting("subShadow", String(v))}
      />
      <label className="check settings-check">
        <input
          type="checkbox"
          checked={get("subAssOverride") === "true"}
          onChange={(e) => setSetting("subAssOverride", String(e.target.checked))}
        />
        {t("settings.subAssOverride")}
      </label>
      <p className="hint">{t("settings.subStyleHint")}</p>
    </>
  );
}
