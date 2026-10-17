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
  Pin,
  Play,
  Plus,
  RotateCcw,
  RotateCw,
  Volume2,
  VolumeX,
  X,
} from "lucide-react";
import { useCallback, useEffect, useRef, useState, type ReactNode } from "react";
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
/** Píxeles (de un vídeo de 720 de alto) por cada 1 % de altura de los subtítulos. */
const PX_PER_PERCENT = 7.2;
/** Por debajo de este tamaño de ventana, controles mínimos (una ventana pequeña flotando). */
const isCompact = () => window.innerWidth < 560 || window.innerHeight < 320;

/** Botón que repite la acción mientras se mantiene pulsado (para ir de 1 en 1 sin cansarse). */
function RepeatButton({ onStep, title, children }: { onStep: () => void; title?: string; children: ReactNode }) {
  const timer = useRef(0);
  const step = useRef(onStep);
  step.current = onStep;
  const stop = () => window.clearTimeout(timer.current);
  const start = (e: React.PointerEvent) => {
    if (e.button !== 0) return;
    step.current();
    const repeat = (delay: number) => {
      timer.current = window.setTimeout(() => {
        step.current();
        repeat(70);
      }, delay);
    };
    repeat(400);
  };
  useEffect(() => stop, []);
  return (
    <button className="icon-btn small" onPointerDown={start} onPointerUp={stop} onPointerLeave={stop} title={title}>
      {children}
    </button>
  );
}

function Stepper(props: {
  label: string;
  value: string;
  onMinus: () => void;
  onPlus: () => void;
  onReset?: () => void;
  minus?: ReactNode;
  plus?: ReactNode;
  minusTitle?: string;
  plusTitle?: string;
}) {
  const { t } = useTranslation();
  return (
    <div className="pl-stepper">
      <span>{props.label}</span>
      <div>
        <RepeatButton onStep={props.onMinus} title={props.minusTitle}>
          {props.minus ?? <Minus size={14} />}
        </RepeatButton>
        <button className="pl-stepper-value" onClick={props.onReset} title={props.onReset ? t("player.reset") : undefined} disabled={!props.onReset}>
          {props.value}
        </button>
        <RepeatButton onStep={props.onPlus} title={props.plusTitle}>
          {props.plus ?? <Plus size={14} />}
        </RepeatButton>
      </div>
    </div>
  );
}

/** El reproductor integrado: mpv dibuja el vídeo debajo y esto pone los controles encima. */
export function Player() {
  const { t } = useTranslation();
  const [s, setS] = useState<PlayerState | null>(null);
  const [visible, setVisible] = useState(true);
  const [panel, setPanel] = useState<"audio" | "subs" | null>(null);
  const [drag, setDrag] = useState<number | null>(null);
  const [hover, setHover] = useState<{ x: number; time: number } | null>(null);
  const [compact, setCompact] = useState(isCompact);
  const hideTimer = useRef(0);
  const clickTimer = useRef(0);
  const seekTimer = useRef(0);
  const barRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    void api.player.state().then(setS);
    const sub = onPlayerState(setS);
    const onResize = () => setCompact(isCompact());
    window.addEventListener("resize", onResize);
    return () => {
      void sub.then((f) => f());
      window.removeEventListener("resize", onResize);
    };
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
  const margin = s?.subMargin;
  useEffect(() => {
    if (margin == null) return;
    const update = () => {
      const lift = shown && !compact ? (120 * 720) / window.innerHeight : 0;
      void api.player.set("sub-margin-y", Math.round(Math.max(margin, lift))).catch(() => {});
    };
    update();
    window.addEventListener("resize", update);
    return () => window.removeEventListener("resize", update);
  }, [margin, shown, compact]);

  const togglePause = () => s && void api.player.set("pause", !s.paused);
  const seekBy = (sec: number) => void api.player.command("seek", sec, "relative");
  const fullscreen = (on: boolean) => void api.player.fullscreen(on);
  const close = () => void api.player.close();
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
      else if (k === "t") void api.player.ontop(!s.ontop);
      else if (k === "z") add("sub-delay", -0.1);
      else if (k === "x") add("sub-delay", 0.1);
      else if (k === "escape") {
        if (panel) setPanel(null);
        else if (s.fullscreen) fullscreen(false);
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
  // Arrastrando la barra de arriba (o toda la ventana si es pequeña) se mueve la ventana.
  const dragRegion = s.fullscreen ? {} : { "data-tauri-drag-region": true };

  const windowButtons = (
    <>
      {!s.fullscreen && (
        <button className={s.ontop ? "pl-btn active" : "pl-btn"} onClick={() => void api.player.ontop(!s.ontop)} title={t(s.ontop ? "player.ontopOff" : "player.ontop")}>
          <Pin size={18} fill={s.ontop ? "currentColor" : "none"} />
        </button>
      )}
      <button className="pl-btn" onClick={close} title={t("player.close")}>
        <X size={20} />
      </button>
    </>
  );

  if (compact) {
    return (
      <div className={shown ? "player compact" : "player compact idle"} onMouseMove={poke} {...dragRegion}>
        {loading && <Loader2 className="pl-spinner spin" size={32} />}
        <div className="pl-compact-actions">{windowButtons}</div>
        <button className="pl-compact-play" onClick={togglePause} title={t(s.paused ? "player.play" : "player.pause")}>
          {s.paused ? <Play size={26} fill="currentColor" strokeWidth={0} /> : <Pause size={26} fill="currentColor" strokeWidth={0} />}
        </button>
        <div className="pl-compact-progress">
          <span style={{ width: `${progress * 100}%` }} />
        </div>
      </div>
    );
  }

  const marginPercent = Math.round(s.subMargin / PX_PER_PERCENT);

  return (
    <div className={shown ? "player" : "player idle"} onMouseMove={poke}>
      <div className="pl-stage" onClick={onStageClick} onDoubleClick={onStageDouble} />
      {loading && <Loader2 className="pl-spinner spin" size={44} />}

      <div className="pl-top" {...dragRegion}>
        <span className="pl-title" {...dragRegion}>
          {s.title}
        </span>
        {windowButtons}
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
                onMinus={() => s.subScale > 0.32 && add("sub-scale", -0.05)}
                onPlus={() => s.subScale < 2.98 && add("sub-scale", 0.05)}
              />
              <Stepper
                label={t("player.subHeight")}
                value={`${marginPercent} %`}
                onMinus={() => void api.player.subMargin(-1)}
                onPlus={() => void api.player.subMargin(1)}
                minus={<ChevronDown size={16} />}
                plus={<ChevronUp size={16} />}
                minusTitle={t("player.down")}
                plusTitle={t("player.up")}
              />
              <label className="pl-switch">
                <span>{t("player.subBackground")}</span>
                <input type="checkbox" checked={s.subBackground} onChange={(e) => void api.player.subBackground(e.target.checked)} />
              </label>
              <p className="pl-note">{t("player.perMovie")}</p>
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
          <button className="pl-btn" onClick={() => fullscreen(!s.fullscreen)} title={t(s.fullscreen ? "player.exitFullscreen" : "player.fullscreen")}>
            {s.fullscreen ? <Minimize size={20} /> : <Maximize size={20} />}
          </button>
        </div>
      </div>
    </div>
  );
}
