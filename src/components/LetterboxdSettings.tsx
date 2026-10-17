import { open, save } from "@tauri-apps/plugin-dialog";
import { openUrl, revealItemInDir } from "@tauri-apps/plugin-opener";
import { Download, Loader2, Upload } from "lucide-react";
import { useState } from "react";
import { useTranslation } from "react-i18next";
import { errorText } from "../actions";
import { api, type LetterboxdExport, type LetterboxdImport } from "../api";
import { useStore } from "../store";

const IMPORT_URL = "https://letterboxd.com/import/";
const EXPORT_URL = "https://letterboxd.com/settings/data/";

/** Ajustes → Letterboxd: exportar el historial a su importador e importar tu exportación. */
export function LetterboxdSettings() {
  const { t } = useTranslation();
  const settings = useStore((s) => s.settings);
  const setSetting = useStore((s) => s.setSetting);
  const busy = useStore((s) => s.scan != null);
  const onlyNew = settings.letterboxdOnlyNew !== "false";
  const withWatchlist = settings.letterboxdWatchlist === "true";

  const [exported, setExported] = useState<LetterboxdExport | null>(null);
  const [imported, setImported] = useState<LetterboxdImport | null>(null);
  const [importing, setImporting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const doExport = async () => {
    setError(null);
    setExported(null);
    const path = await save({
      title: t("letterboxd.exportTitle"),
      defaultPath: `letterboxd-${new Date().toISOString().slice(0, 10)}.csv`,
      filters: [{ name: "CSV", extensions: ["csv"] }],
    });
    if (!path) return;
    try {
      setExported(await api.letterboxdExport(path, onlyNew));
    } catch (e) {
      setError(errorText(String(e)));
    }
  };

  const doImport = async () => {
    setError(null);
    setImported(null);
    const path = await open({
      multiple: false,
      title: t("letterboxd.importTitle"),
      filters: [{ name: t("letterboxd.importFilter"), extensions: ["zip", "csv"] }],
    });
    if (typeof path !== "string") return;
    setImporting(true);
    try {
      setImported(await api.letterboxdImport(path, withWatchlist));
    } catch (e) {
      setError(errorText(String(e)));
    }
    setImporting(false);
  };

  return (
    <>
      <p className="hint">{t("letterboxd.intro")}</p>

      <h4 className="settings-sub">{t("letterboxd.export")}</h4>
      <label className="check settings-check">
        <input type="checkbox" checked={onlyNew} onChange={(e) => setSetting("letterboxdOnlyNew", String(e.target.checked))} />
        {t("letterboxd.onlyNew")}
      </label>
      <button className="btn" onClick={() => void doExport()}>
        <Upload size={16} />
        {t("letterboxd.exportButton")}
      </button>
      {exported && exported.count === 0 && <p className="hint">{t("letterboxd.nothingNew")}</p>}
      {exported && exported.count > 0 && (
        <p className="hint ok">
          {t("letterboxd.exported", { count: exported.count })}{" "}
          <button className="link-btn inline" onClick={() => void openUrl(IMPORT_URL)}>
            letterboxd.com/import
          </button>
          {" · "}
          <button className="link-btn inline" onClick={() => void revealItemInDir(exported.files[0])}>
            {t("letterboxd.showFile")}
          </button>
          {exported.files.length > 1 && ` ${t("letterboxd.parts", { count: exported.files.length })}`}
        </p>
      )}

      <h4 className="settings-sub">{t("letterboxd.import")}</h4>
      <p className="hint">
        {t("letterboxd.importHint")}{" "}
        <button className="link-btn inline" onClick={() => void openUrl(EXPORT_URL)}>
          letterboxd.com/settings/data
        </button>
      </p>
      <label className="check settings-check">
        <input type="checkbox" checked={withWatchlist} onChange={(e) => setSetting("letterboxdWatchlist", String(e.target.checked))} />
        {t("letterboxd.watchlist")}
      </label>
      <button className="btn" onClick={() => void doImport()} disabled={importing || busy}>
        {importing ? <Loader2 size={16} className="spin" /> : <Download size={16} />}
        {t(importing ? "letterboxd.importing" : "letterboxd.importButton")}
      </button>
      {!settings.tmdbKey && <p className="hint">{t("letterboxd.noKey")}</p>}
      {imported && (
        <div className="hint ok">
          {t("letterboxd.imported", {
            films: imported.films,
            viewings: imported.viewings,
            ratings: imported.ratings,
          })}
          {imported.added > 0 && ` ${t("letterboxd.added", { count: imported.added })}`}
          {imported.pending > 0 && ` ${t("letterboxd.pending", { count: imported.pending })}`}
          {imported.unmatched.length > 0 && (
            <details className="lb-unmatched">
              <summary>{t("letterboxd.unmatched", { count: imported.unmatched.length })}</summary>
              <ul>
                {imported.unmatched.map((u) => (
                  <li key={u}>{u}</li>
                ))}
              </ul>
            </details>
          )}
        </div>
      )}
      {error && <p className="hint error">{error}</p>}
    </>
  );
}
