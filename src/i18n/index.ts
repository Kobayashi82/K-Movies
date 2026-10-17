import i18n from "i18next";
import { initReactI18next } from "react-i18next";
import { api } from "../api";

type Resource = Record<string, unknown>;

// Idiomas incluidos en la app: cada archivo de `locales/` es uno. Para añadir un idioma basta con
// copiar `en.json` con otro código (p. ej. `fr.json`) y traducirlo. Los usuarios también pueden
// dejar archivos en `<carpeta de datos>/locales/` sin recompilar.
const bundled = import.meta.glob<Resource>("./locales/*.json", { eager: true, import: "default" });

export interface LanguageOption {
  code: string;
  name: string;
}

export const available: LanguageOption[] = [];

function register(code: string, resource: Resource) {
  i18n.addResourceBundle(code, "translation", resource, true, true);
  const name = typeof resource._name === "string" ? resource._name : code;
  const existing = available.find((l) => l.code === code);
  if (existing) existing.name = name;
  else available.push({ code, name });
  available.sort((a, b) => a.name.localeCompare(b.name));
}

/** Idioma del sistema si hay traducción para él; si no, inglés. */
export function systemLanguage(): string {
  const codes = available.map((l) => l.code);
  for (const lang of navigator.languages ?? [navigator.language]) {
    if (codes.includes(lang)) return lang;
    const base = lang.split("-")[0];
    if (codes.includes(base)) return base;
  }
  return "en";
}

export function resolveLanguage(setting: string | undefined): string {
  if (setting && setting !== "system" && available.some((l) => l.code === setting)) return setting;
  return systemLanguage();
}

export async function initI18n(setting: string | undefined) {
  await i18n.use(initReactI18next).init({
    resources: {},
    fallbackLng: "en",
    interpolation: { escapeValue: false },
    returnNull: false,
  });
  for (const [path, resource] of Object.entries(bundled)) {
    register(path.match(/([\w-]+)\.json$/)![1], resource);
  }
  try {
    for (const [code, resource] of Object.entries(await api.userLocales())) register(code, resource);
  } catch {
    // Sin idiomas de usuario: no pasa nada.
  }
  await i18n.changeLanguage(resolveLanguage(setting));
  document.documentElement.lang = i18n.language;
}

export async function setLanguage(setting: string) {
  await i18n.changeLanguage(resolveLanguage(setting));
  document.documentElement.lang = i18n.language;
}

export default i18n;
