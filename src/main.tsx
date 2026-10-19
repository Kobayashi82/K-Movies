import { getCurrentWindow } from "@tauri-apps/api/window";
import React from "react";
import ReactDOM from "react-dom/client";
import App from "./App";
import { ErrorBoundary } from "./components/ErrorBoundary";
import { Player } from "./components/Player";
import { api } from "./api";
import { defaultMetadataLanguage, defaultPlayerLanguages } from "./components/Settings";
import i18n, { initI18n } from "./i18n";
import { useStore } from "./store";
import { installTooltips } from "./tooltips";
import "./styles.css";

/** La misma interfaz sirve para la ventana principal y para la del reproductor integrado. */
const isPlayer = window.location.hash === "#player";

/** Sin cosas del navegador: ni su menú del botón derecho (imprimir, guardar…) salvo para editar
 *  texto, ni sus atajos de imprimir, guardar, ver el código o recargar. */
function noBrowserChrome() {
  document.addEventListener("contextmenu", (e) => {
    const target = e.target as HTMLElement;
    const editable = target.closest("input:not([type=range]):not([type=checkbox]), textarea, [contenteditable]");
    if (isPlayer || !editable) e.preventDefault();
  });
  if (!import.meta.env.PROD) return;
  window.addEventListener("keydown", (e) => {
    const key = e.key.toLowerCase();
    if (key === "f5" || (e.ctrlKey && ["p", "s", "u", "r"].includes(key))) e.preventDefault();
  });
}

/** Los deslizadores también se mueven con la rueda del ratón. En los de dos asas (filtros) se mueve
 *  la más cercana al ratón. En el reproductor la rueda ya es el volumen. */
function wheelSliders() {
  const setValue = Object.getOwnPropertyDescriptor(HTMLInputElement.prototype, "value")!.set!;
  document.addEventListener(
    "wheel",
    (e) => {
      const target = e.target as Element;
      if (!e.deltaY || target.closest(".player")) return;
      let input: HTMLInputElement | null = null;
      const track = target.closest(".range-track");
      if (track) {
        const [lo, hi] = track.querySelectorAll<HTMLInputElement>("input[type=range]");
        const rect = track.getBoundingClientRect();
        const at = Number(lo.min) + ((e.clientX - rect.left) / rect.width) * (Number(lo.max) - Number(lo.min));
        input = Math.abs(Number(lo.value) - at) <= Math.abs(Number(hi.value) - at) ? lo : hi;
      } else {
        input = target.closest<HTMLInputElement>("input[type=range]");
      }
      if (!input || input.disabled) return;
      e.preventDefault();
      const min = Number(input.min || 0);
      const max = Number(input.max || 100);
      const step = Number(input.step) || 1;
      // Unos 40 golpes de rueda de punta a punta (y nunca menos de un paso).
      const jump = step * Math.max(1, Math.round((max - min) / step / 40));
      const next = Math.min(max, Math.max(min, Number(input.value) + (e.deltaY < 0 ? jump : -jump)));
      setValue.call(input, String(Number((Math.round(next / step) * step).toFixed(4))));
      // React escucha el evento `input` para su `onChange`.
      input.dispatchEvent(new Event("input", { bubbles: true }));
    },
    { passive: false },
  );
}

/** Idioma de la información de TMDb: "Automático" sigue al de la interfaz. El backend no sabe cuál
 *  es el de la interfaz (puede ser el del sistema), así que se le apunta en `metadataLanguageAuto`. */
async function syncMetadataLanguage(settings: Record<string, string>) {
  const { setSetting } = useStore.getState();
  const auto = defaultMetadataLanguage(i18n.language);
  if (settings.metadataLanguageAuto !== auto) {
    setSetting("metadataLanguageAuto", auto);
    await api.setSetting("metadataLanguageAuto", auto);
  }
  // Antes se apuntaba el de la interfaz como elegido: si coincide, pasa a "Automático" (una vez).
  if (!settings.metadataLanguage || (!settings.metadataLanguageAutoMigrated && settings.metadataLanguage === auto)) {
    setSetting("metadataLanguage", "auto");
    await api.setSetting("metadataLanguage", "auto");
  }
  if (!settings.metadataLanguageAutoMigrated) setSetting("metadataLanguageAutoMigrated", "true");
}

async function boot() {
  const settings = await api.getSettings();
  await initI18n(settings.uiLanguage);
  noBrowserChrome();
  wheelSliders();
  installTooltips();
  if (isPlayer) {
    // Fondo transparente: el vídeo lo dibuja mpv debajo de la interfaz.
    document.documentElement.classList.add("player-mode");
    ReactDOM.createRoot(document.getElementById("root") as HTMLElement).render(
      <ErrorBoundary>
        <Player />
      </ErrorBoundary>,
    );
    return;
  }
  await useStore.getState().init();
  await syncMetadataLanguage(settings);
  // Igual con los idiomas del reproductor integrado.
  if (settings.audioLang == null) {
    for (const [k, v] of Object.entries(defaultPlayerLanguages(i18n.language.split("-")[0]))) useStore.getState().setSetting(k, v);
  }

  ReactDOM.createRoot(document.getElementById("root") as HTMLElement).render(
    <React.StrictMode>
      <ErrorBoundary>
        <App />
      </ErrorBoundary>
    </React.StrictMode>,
  );
}

// La ventana arranca oculta (evita el destello blanco) y la muestra App; si el arranque falla,
// hay que mostrarla aquí o quedaría invisible.
boot().catch((e) => {
  console.error(e);
  document.body.style.cssText = "color:#eee;font:14px system-ui;padding:24px";
  document.body.textContent = `K-Movies: ${String(e)}`;
  void getCurrentWindow().show();
});
