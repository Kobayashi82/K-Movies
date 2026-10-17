import { useEffect } from "react";

/** Aplica el tema (`system` | `dark` | `light`) y sigue los cambios del sistema. */
export function useTheme(setting: string | undefined) {
  useEffect(() => {
    const media = window.matchMedia("(prefers-color-scheme: light)");
    const apply = () => {
      const theme = setting === "dark" || setting === "light" ? setting : media.matches ? "light" : "dark";
      document.documentElement.dataset.theme = theme;
    };
    apply();
    media.addEventListener("change", apply);
    return () => media.removeEventListener("change", apply);
  }, [setting]);
}
