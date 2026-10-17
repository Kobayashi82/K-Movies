import { getCurrentWindow } from "@tauri-apps/api/window";
import { Copy, Minus, Square, X } from "lucide-react";
import { useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import { Logo } from "./Logo";

const win = getCurrentWindow();

export function TitleBar() {
  const { t } = useTranslation();
  const [maximized, setMaximized] = useState(false);

  useEffect(() => {
    void win.isMaximized().then(setMaximized);
    const unlisten = win.onResized(() => void win.isMaximized().then(setMaximized));
    return () => void unlisten.then((f) => f());
  }, []);

  return (
    <header className="titlebar" data-tauri-drag-region>
      <div className="brand" data-tauri-drag-region>
        <Logo size={18} />
        <span data-tauri-drag-region>K-Movies</span>
        <span className="brand-version" data-tauri-drag-region>
          5.0
        </span>
      </div>
      <div className="window-controls">
        <button title={t("window.minimize")} onClick={() => void win.minimize()}>
          <Minus size={16} />
        </button>
        <button title={t(maximized ? "window.restore" : "window.maximize")} onClick={() => void win.toggleMaximize()}>
          {maximized ? <Copy size={13} /> : <Square size={13} />}
        </button>
        <button className="close" title={t("window.close")} onClick={() => void win.close()}>
          <X size={17} />
        </button>
      </div>
    </header>
  );
}
