import { flushSync } from "react-dom";
import i18n from "./i18n";

export function formatRuntime(minutes: number | null): string {
  if (!minutes) return "";
  const h = Math.floor(minutes / 60);
  const m = minutes % 60;
  return h ? i18n.t("format.hoursMinutes", { h, m }) : i18n.t("format.minutes", { m });
}

/** Segundos → `1:05:09` o `5:09`. */
export function formatClock(seconds: number | null): string {
  const s = Math.max(0, Math.floor(seconds ?? 0));
  const h = Math.floor(s / 3600);
  const mm = String(Math.floor(s / 60) % 60);
  const ss = String(s % 60).padStart(2, "0");
  return h ? `${h}:${mm.padStart(2, "0")}:${ss}` : `${mm}:${ss}`;
}

/** Lo que falta para terminar (`1 h 5 min`), redondeado al minuto y nunca menos de uno. */
export function formatRemaining(position: number | null, duration: number | null): string {
  if (!duration || position == null) return "";
  return formatRuntime(Math.max(1, Math.round((duration - position) / 60)));
}

export function formatSize(bytes: number): string {
  if (!bytes) return "";
  const units = ["B", "KB", "MB", "GB", "TB"];
  let i = 0;
  let v = bytes;
  while (v >= 1024 && i < units.length - 1) {
    v /= 1024;
    i++;
  }
  return `${new Intl.NumberFormat(i18n.language, { maximumFractionDigits: i >= 3 ? 1 : 0 }).format(v)} ${units[i]}`;
}

export function formatDate(date: Date | null, opts: Intl.DateTimeFormatOptions = { dateStyle: "medium" }): string {
  return date ? new Intl.DateTimeFormat(i18n.language, opts).format(date) : "";
}

export function formatNumber(n: number | null, digits = 1): string {
  return n == null ? "" : new Intl.NumberFormat(i18n.language, { minimumFractionDigits: digits, maximumFractionDigits: digits }).format(n);
}

const displayNames = new Map<string, Intl.DisplayNames>();

/** Nombre de país o idioma a partir del código ISO, en el idioma de la interfaz. */
export function displayName(code: string, type: "region" | "language"): string {
  const key = `${i18n.language}:${type}`;
  let dn = displayNames.get(key);
  if (!dn) {
    dn = new Intl.DisplayNames([i18n.language], { type });
    displayNames.set(key, dn);
  }
  try {
    return dn.of(code) ?? code;
  } catch {
    return code;
  }
}

/** Color estable a partir de un texto, para los pósters que no tienen imagen. */
export function hueOf(s: string): number {
  let h = 0;
  for (let i = 0; i < s.length; i++) h = (h * 31 + s.charCodeAt(i)) % 360;
  return h;
}

const HERO = "poster-hero";

/**
 * Transición de "elemento compartido": el póster de la rejilla vuela hasta su sitio en la ficha.
 * `source` es el póster de la tarjeta; la ficha usa `view-transition-name: poster-hero`.
 */
export function heroTransition(source: HTMLElement | null, update: () => void, closing = false) {
  const doc = document as Document & { startViewTransition?: (cb: () => void) => { finished: Promise<void> } };
  if (!doc.startViewTransition || window.matchMedia("(prefers-reduced-motion: reduce)").matches) {
    update();
    return;
  }
  if (source && !closing) source.style.viewTransitionName = HERO;
  const transition = doc.startViewTransition(() => {
    if (source) source.style.viewTransitionName = closing ? HERO : "";
    flushSync(update);
  });
  transition.finished.finally(() => {
    if (source) source.style.viewTransitionName = "";
  });
}
