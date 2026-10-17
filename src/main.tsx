import { getCurrentWindow } from "@tauri-apps/api/window";
import React from "react";
import ReactDOM from "react-dom/client";
import App from "./App";
import { ErrorBoundary } from "./components/ErrorBoundary";
import { api } from "./api";
import { defaultMetadataLanguage } from "./components/Settings";
import i18n, { initI18n } from "./i18n";
import { useStore } from "./store";
import "./styles.css";

async function boot() {
  const settings = await api.getSettings();
  await initI18n(settings.uiLanguage);
  await useStore.getState().init();
  // El idioma de los datos de TMDb sigue al de la interfaz hasta que se cambie en Ajustes.
  if (!settings.metadataLanguage) useStore.getState().setSetting("metadataLanguage", defaultMetadataLanguage(i18n.language));

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
