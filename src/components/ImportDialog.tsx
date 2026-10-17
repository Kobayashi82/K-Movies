import { FolderInput, X } from "lucide-react";
import { motion } from "motion/react";
import { useTranslation } from "react-i18next";
import { importPaths } from "../actions";
import { useStore } from "../store";

/** Con varias carpetas de biblioteca, elige a cuál se importan los archivos. */
export function ImportDialog({ paths }: { paths: string[] }) {
  const { t } = useTranslation();
  const libraries = useStore((s) => s.libraries);
  const close = () => useStore.getState().openDialog(null);
  const pick = async (id: number) => {
    close();
    await importPaths(paths, id);
  };
  return (
    <motion.div className="modal-backdrop" onClick={close} initial={{ opacity: 0 }} animate={{ opacity: 1 }} exit={{ opacity: 0 }}>
      <motion.div
        className="modal small"
        onClick={(e) => e.stopPropagation()}
        initial={{ opacity: 0, y: 20 }}
        animate={{ opacity: 1, y: 0 }}
        exit={{ opacity: 0, y: 12 }}
      >
        <header className="modal-header">
          <h2>{t("import.where", { count: paths.length })}</h2>
          <button className="icon-btn" onClick={close} title={t("common.close")}>
            <X size={18} />
          </button>
        </header>
        <div className="modal-body">
          <ul className="library-list">
            {libraries.map((l) => (
              <li key={l.id}>
                <button className="library-pick" onClick={() => void pick(l.id)}>
                  <FolderInput size={18} />
                  <span className="library-path">{l.path}</span>
                </button>
              </li>
            ))}
          </ul>
        </div>
      </motion.div>
    </motion.div>
  );
}
