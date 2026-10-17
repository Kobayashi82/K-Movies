import {
  AudioLines,
  Captions,
  ChevronDown,
  ChevronUp,
  Loader2,
  Maximize,
  Minimize,
  Minus,
  Pause,
  PictureInPicture2,
  Play,
  Plus,
  RotateCcw,
  RotateCw,
  Volume2,
  VolumeX,
  X,
} from "lucide-react";
import { useCallback, useEffect, useRef, useState } from "react";
import { useTranslation } from "react-i18next";
import { api, onPlayerState, type PlayerState, type Track } from "../api";
import { displayName, formatClock, formatNumber } from "../util";

/** mpv da los idiomas en ISO 639-2 (`spa`, `eng`); `Intl` los entiende mejor en 639-1. */
const LANG3: Record<string, string> = {
  spa: "es", eng: "en", fre: "fr", fra: "fr", ger: "de", deu: "de", ita: "it", por: "pt", jpn: "ja",
  kor: "ko", chi: "zh", zho: "zh", rus: "ru", cat: "ca", baq: "eu", eus: "eu", glg: "gl", dut: "nl",
  nld: "nl", swe: "sv", nor: "no", dan: "da", fin: "fi", pol: "pl", tur: "tr", ara: "ar", hin: "hi",
  tha: "th", cze: "cs", ces: "cs", gre: "el", ell: "el", heb: "he", hun: "hu", rum: "ro", ron: "ro", ukr: "uk",
};

const CODECS: Record<string, string> = {
  dts: "DTS", ac3: "AC3", eac3: "E-AC3", truehd: "TrueHD", aac: "AAC", opus: "Opus", flac: "FLAC",
  mp3: "MP3", vorbis: "Vorbis", pcm_s16le: "PCM", subrip: "SRT", ass: "ASS", ssa: "SSA",
  hdmv_pgs_subtitle: "PGS", dvd_subtitle: "VobSub", mov_text: "MP4", webvtt: "WebVTT",
};

const CHANNELS: Record<number, string> = { 1: "mono", 2: "2.0", 6: "5.1", 8: "7.1" };

const capitalize = (s: string) => s.charAt(0).toLocaleUpperCase() + s.slice(1);

function trackName(t: Track, fallback: string): string {
  const lang = t.lang ? capitalize(displayName(LANG3[t.lang] ?? t.lang, "language")) : null;
  const parts = [lang, t.title && t.title !== lang ? t.title : null].filter(Boolean);
  return parts.length ? parts.join(" · ") : fallback;
}

function trackDetail(t: Track, flags: { forced: string; external: string }): string {
  const codec = t.codec ? (CODECS[t.codec] ?? t.codec.toUpperCase()) : null;
  const channels = t["demux-channel-count"] ? (CHANNELS[t["demux-channel-count"]] ?? `${t["demux-channel-count"]} ch`) : null;
  return [codec, channels, t.forced ? flags.forced : null, t.external ? flags.external : null].filter(Boolean).join(" · ");
}

/** "+0,25 s" */
const signed = (v: number) => `${v > 0 ? "+" : v < 0 ? "−" : ""}${formatNumber(Math.abs(v), 2)} s`;

const HIDE_AFTER = 2500;

function Stepper({ label, value, onMinus, onPlus, onReset }: { label: string; value: string; onMinus: () => void; onPlus: () => void; onReset?: () => void }) {
  const { t } = useTranslation();
  return (
    <div className="pl-stepper">
      <span>{label}</span>
      <div>
        <button className="icon-btn small" onClick={onMinus}>
          <Minus size={14} />
        </button>
        <button className="pl-stepper-value" onClick={onReset} title={onReset ? t("player.reset") : undefined} disabled={!onReset}>
          {value}
        </button>
        <button className="icon-btn small" onClick={onPlus}>
          <Plus size={14} />
        </button>
      </div>
    </div>
  );
}

/** Ventana del reproductor integrado: mpv dibuja el vídeo debajo y esto pone los controles encima. */
export function Player() {
  const { t } = useTranslation();
  const [s, setS] = useState<PlayerState | null>(null);
  const [visible, setVisible] = useState(true);
  const [panel, setPanel] = useState<"audio" | "subs" | null>(null);
  const [drag, setDrag] = useState<number | null>(null);
  const [hover, setHover] = useState<{ x: number; time: number } | null>(null);
  const hideTimer = useRef(0);
  const clickTimer = useRef(0);
  const seekTimer = useRef(0);
  const barRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    void api.player.state().then(setS);
    const sub = onPlayerState(setS);
    return () => void sub.then((f) => f());
  }, []);

  // Los controles se esconden (y el cursor) tras unos segundos sin mover el ratón, salvo en pausa.
  const poke = useCallback(() => {
    setVisible(true);
    window.clearTimeout(hideTimer.current);
    hideTimer.current = window.setTimeout(() => setVisible(false), HIDE_AFTER);
  }, []);
  useEffect(() => {
    poke();
    return () => window.clearTimeout(hideTimer.current);
  }, [poke]);
  const shown = visible || !s || s.paused || panel != null || drag != null;

  // Con los controles a la vista, los subtítulos suben para no quedar debajo de la barra.
  // `sub-margin-y` va en píxeles de un vídeo de 720 de alto, sea cual sea el tamaño real.
  const ready = s != null;
  const pipMode = s?.pip ?? false;
  useEffect(() => {
    if (!ready) return;
    const update = () => {
      const lift = shown && !pipMode ? (120 * 720) / window.innerHeight : 22;
      void api.player.set("sub-margin-y", Math.round(lift)).catch(() => {});
    };
    update();
    window.addEventListener("resize", update);
    return () => window.removeEventListener("resize", update);
  }, [ready, shown, pipMode]);

  const togglePause = () => s && void api.player.set("pause", !s.paused);
  const seekBy = (sec: number) => void api.player.command("seek", sec, "relative");
  const fullscreen = (on: boolean) => void api.player.fullscreen(on);
  const pip = (on: boolean) => void api.player.pip(on);
  // Volumen, tamaño y posición de los subtítulos los guarda el backend cuando mpv confirma el cambio.
  const setVolume = (v: number) => {
    void api.player.set("volume", Math.max(0, Math.min(130, Math.round(v))));
    if (s?.mute) void api.player.set("mute", false);
  };
  /** Suma sobre el valor que tiene mpv ahora (no sobre el último estado recibido, que puede ir atrasado). */
  const add = (name: string, delta: number) => void api.player.command("add", name, delta);

  // Teclado: los atajos habituales de los reproductores.
  useEffect(() => {
    const onKey = (e: KeyboardEvent) => {
      if (!s) return;
      poke();
      const k = e.key.toLowerCase();
      if (k === " " || k === "k") togglePause();
      else if (k === "arrowleft") seekBy(e.shiftKey ? -60 : -10);
      else if (k === "arrowright") seekBy(e.shiftKey ? 60 : 10);
      else if (k === "arrowup") add("volume", 5);
      else if (k === "arrowdown") add("volume", -5);
      else if (k === "m") void api.player.set("mute", !s.mute);
      else if (k === "f" || k === "enter") fullscreen(!s.fullscreen);
      else if (k === "p") pip(!s.pip);
      else if (k === "z") add("sub-delay", -0.1);
      else if (k === "x") add("sub-delay", 0.1);
      else if (k === "escape") {
        if (panel) setPanel(null);
        else if (s.fullscreen) fullscreen(false);
        else if (s.pip) pip(false);
      } else return;
      e.preventDefault();
    };
    window.addEventListener("keydown", onKey);
    return () => window.removeEventListener("keydown", onKey);
  });

  // Un clic pausa; doble clic, pantalla completa (el clic simple espera por si llega el segundo).
  const onStageClick = () => {
    if (panel) return setPanel(null);
    window.clearTimeout(clickTimer.current);
    clickTimer.current = window.setTimeout(togglePause, 220);
  };
  const onStageDouble = () => {
    window.clearTimeout(clickTimer.current);
    if (s) fullscreen(!s.fullscreen);
  };

  const timeAt = (clientX: number) => {
    const rect = barRef.current?.getBoundingClientRect();
    if (!rect || !s?.duration) return 0;
    return Math.max(0, Math.min(1, (clientX - rect.left) / rect.width)) * s.duration;
  };
  const onBarDown = (e: React.PointerEvent) => {
    e.currentTarget.setPointerCapture(e.pointerId);
    setDrag(timeAt(e.clientX));
  };
  const onBarMove = (e: React.PointerEvent) => {
    const time = timeAt(e.clientX);
    const rect = barRef.current!.getBoundingClientRect();
    setHover({ x: e.clientX - rect.left, time });
    if (drag == null) return;
    setDrag(time);
    // Mientras se arrastra, saltos rápidos a fotogramas clave; al soltar, el punto exacto.
    window.clearTimeout(seekTimer.current);
    seekTimer.current = window.setTimeout(() => void api.player.command("seek", time.toFixed(2), "absolute+keyframes"), 60);
  };
  const onBarUp = (e: React.PointerEvent) => {
    if (drag == null) return;
    window.clearTimeout(seekTimer.current);
    void api.player.command("seek", timeAt(e.clientX).toFixed(2), "absolute+exact");
    setDrag(null);
  };

  if (!s) return <div className="player" />;

  const position = drag ?? s.position ?? 0;
  const progress = s.duration ? Math.min(1, position / s.duration) : 0;
  const audio = (s.tracks ?? []).filter((tr) => tr.type === "audio");
  const subs = (s.tracks ?? []).filter((tr) => tr.type === "sub");
  const flags = { forced: t("player.forced"), external: t("player.external") };
  const loading = s.duration == null || s.buffering;

  if (s.pip) {
    return (
      <div className={shown ? "player pip" : "player pip idle"} onMouseMove={poke} data-tauri-drag-region>
        {loading && <Loader2 className="pl-spinner spin" size={32} />}
        <div className="pl-pip-actions">
          <button className="pl-btn" onClick={() => pip(false)} title={t("player.exitPip")}>
            <PictureInPicture2 size={18} />
          </button>
          <button className="pl-btn" onClick={() => void api.player.close()} title={t("player.close")}>
            <X size={18} />
          </button>
        </div>
        <button className="pl-pip-play" onClick={togglePause} title={t(s.paused ? "player.play" : "player.pause")}>
          {s.paused ? <Play size={26} fill="currentColor" strokeWidth={0} /> : <Pause size={26} fill="currentColor" strokeWidth={0} />}
        </button>
        <div className="pl-pip-progress">
          <span style={{ width: `${progress * 100}%` }} />
        </div>
      </div>
    );
  }

  return (
    <div className={shown ? "player" : "player idle"} onMouseMove={poke}>
      <div className="pl-stage" onClick={onStageClick} onDoubleClick={onStageDouble} />
      {loading && <Loader2 className="pl-spinner spin" size={44} />}

      <div className="pl-top" data-tauri-drag-region>
        <span className="pl-title" data-tauri-drag-region>
          {s.title}
        </span>
        <button className="pl-btn" onClick={() => pip(true)} title={t("player.pip")}>
          <PictureInPicture2 size={18} />
        </button>
        <button className="pl-btn" onClick={() => void api.player.close()} title={t("player.close")}>
          <X size={20} />
        </button>
      </div>

      {panel && (
        <div className="pl-panel" onClick={(e) => e.stopPropagation()}>
          {panel === "audio" ? (
            <>
              <h4>{t("player.audio")}</h4>
              <ul>
                {audio.map((tr) => (
                  <li key={tr.id}>
                    <button className={tr.selected ? "pl-track active" : "pl-track"} onClick={() => void api.player.set("aid", tr.id)}>
                      <strong>{trackName(tr, t("player.track", { id: tr.id }))}</strong>
                      <small>{trackDetail(tr, flags)}</small>
                    </button>
                  </li>
                ))}
              </ul>
              <Stepper
                label={t("player.audioDelay")}
                value={signed(s.audioDelay)}
                onMinus={() => add("audio-delay", -0.05)}
                onPlus={() => add("audio-delay", 0.05)}
                onReset={s.audioDelay ? () => void api.player.set("audio-delay", 0) : undefined}
              />
            </>
          ) : (
            <>
              <h4>{t("player.subtitles")}</h4>
              <ul>
                <li>
                  <button className={subs.every((tr) => !tr.selected) ? "pl-track active" : "pl-track"} onClick={() => void api.player.set("sid", "no")}>
                    <strong>{t("player.off")}</strong>
                  </button>
                </li>
                {subs.map((tr) => (
                  <li key={tr.id}>
                    <button className={tr.selected ? "pl-track active" : "pl-track"} onClick={() => void api.player.set("sid", tr.id)}>
                      <strong>{trackName(tr, t("player.track", { id: tr.id }))}</strong>
                      <small>{trackDetail(tr, flags)}</small>
                    </button>
                  </li>
                ))}
              </ul>
              <Stepper
                label={t("player.subDelay")}
                value={signed(s.subDelay)}
                onMinus={() => add("sub-delay", -0.1)}
                onPlus={() => add("sub-delay", 0.1)}
                onReset={s.subDelay ? () => void api.player.set("sub-delay", 0) : undefined}
              />
              <Stepper
                label={t("player.subSize")}
                value={`${Math.round(s.subScale * 100)} %`}
                onMinus={() => s.subScale > 0.55 && add("sub-scale", -0.1)}
                onPlus={() => s.subScale < 2.95 && add("sub-scale", 0.1)}
                onReset={Math.abs(s.subScale - 1) > 0.01 ? () => void api.player.set("sub-scale", 1) : undefined}
              />
              <div className="pl-stepper">
                <span>{t("player.subPosition")}</span>
                <div>
                  <button className="icon-btn small" onClick={() => add("sub-pos", -5)} title={t("player.up")}>
                    <ChevronUp size={16} />
                  </button>
                  <button
                    className="pl-stepper-value"
                    onClick={s.subPos !== 100 ? () => void api.player.set("sub-pos", 100) : undefined}
                    disabled={s.subPos === 100}
                    title={t("player.reset")}
                  >
                    {s.subPos === 100 ? t("player.bottom") : `${Math.round(s.subPos)}`}
                  </button>
                  <button className="icon-btn small" onClick={() => add("sub-pos", 5)} title={t("player.down")}>
                    <ChevronDown size={16} />
                  </button>
                </div>
              </div>
              <label className="pl-switch">
                <span>{t("player.subBackground")}</span>
                <input
                  type="checkbox"
                  checked={s.subBackground}
                  onChange={(e) => {
                    void api.player.subBackground(e.target.checked);
                    void api.setSetting("subBackground", String(e.target.checked));
                  }}
                />
              </label>
            </>
          )}
        </div>
      )}

      <div className="pl-bottom">
        <div
          ref={barRef}
          className={drag != null ? "pl-bar dragging" : "pl-bar"}
          onPointerDown={onBarDown}
          onPointerMove={onBarMove}
          onPointerUp={onBarUp}
          onPointerLeave={() => setHover(null)}
        >
          <div className="pl-bar-track">
            <span className="pl-bar-fill" style={{ width: `${progress * 100}%` }} />
          </div>
          <span className="pl-bar-thumb" style={{ left: `${progress * 100}%` }} />
          {hover && s.duration != null && (
            <span className="pl-bar-tip" style={{ left: hover.x }}>
              {formatClock(drag ?? hover.time)}
            </span>
          )}
        </div>
        <div className="pl-row">
          <button className="pl-btn big" onClick={togglePause} title={t(s.paused ? "player.play" : "player.pause")}>
            {s.paused ? <Play size={24} fill="currentColor" strokeWidth={0} /> : <Pause size={24} fill="currentColor" strokeWidth={0} />}
          </button>
          <button className="pl-btn" onClick={() => seekBy(-10)} title={t("player.back10")}>
            <RotateCcw size={19} />
          </button>
          <button className="pl-btn" onClick={() => seekBy(10)} title={t("player.forward10")}>
            <RotateCw size={19} />
          </button>
          <div className="pl-volume">
            <button className="pl-btn" onClick={() => void api.player.set("mute", !s.mute)} title={t(s.mute ? "player.unmute" : "player.mute")}>
              {s.mute || s.volume === 0 ? <VolumeX size={20} /> : <Volume2 size={20} />}
            </button>
            <input
              type="range"
              min={0}
              max={130}
              value={s.mute ? 0 : s.volume}
              onChange={(e) => setVolume(Number(e.target.value))}
              style={{ ["--fill" as string]: `${((s.mute ? 0 : s.volume) / 130) * 100}%` }}
              title={`${Math.round(s.volume)} %`}
            />
          </div>
          <span className="pl-time">
            {formatClock(position)} <span>/ {formatClock(s.duration)}</span>
          </span>
          <div className="pl-spacer" />
          <button className={panel === "audio" ? "pl-btn active" : "pl-btn"} onClick={() => setPanel(panel === "audio" ? null : "audio")} title={t("player.audio")}>
            <AudioLines size={20} />
          </button>
          <button className={panel === "subs" ? "pl-btn active" : "pl-btn"} onClick={() => setPanel(panel === "subs" ? null : "subs")} title={t("player.subtitles")}>
            <Captions size={20} />
          </button>
          <button className="pl-btn" onClick={() => pip(true)} title={t("player.pip")}>
            <PictureInPicture2 size={20} />
          </button>
          <button className="pl-btn" onClick={() => fullscreen(!s.fullscreen)} title={t(s.fullscreen ? "player.exitFullscreen" : "player.fullscreen")}>
            {s.fullscreen ? <Minimize size={20} /> : <Maximize size={20} />}
          </button>
        </div>
      </div>
    </div>
  );
}
