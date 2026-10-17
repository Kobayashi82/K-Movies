import { motion } from "motion/react";
import { useEffect } from "react";
import { useStore, type Choice } from "../store";

/** Pregunta con varias respuestas, con el estilo de la app. Esc o clic fuera = cancelar. */
export function ChoiceDialog({ choice }: { choice: Choice }) {
  const answer = (value: string | null) => {
    useStore.setState({ choice: null });
    choice.resolve(value);
  };

  useEffect(() => {
    const onKey = (e: KeyboardEvent) => {
      if (e.key === "Escape") {
        e.stopImmediatePropagation();
        answer(null);
      }
    };
    window.addEventListener("keydown", onKey, { capture: true });
    return () => window.removeEventListener("keydown", onKey, { capture: true });
  });

  return (
    <motion.div className="modal-backdrop choice-backdrop" onClick={() => answer(null)} initial={{ opacity: 0 }} animate={{ opacity: 1 }} exit={{ opacity: 0 }}>
      <motion.div
        className="modal choice"
        role="alertdialog"
        aria-modal="true"
        aria-label={choice.title}
        onClick={(e) => e.stopPropagation()}
        initial={{ opacity: 0, y: 16, scale: 0.96 }}
        animate={{ opacity: 1, y: 0, scale: 1 }}
        exit={{ opacity: 0, scale: 0.97 }}
        transition={{ type: "spring", stiffness: 480, damping: 34 }}
      >
        <h2>{choice.title}</h2>
        <p>{choice.message}</p>
        <div className="choice-actions">
          {choice.options.map((o, i) => (
            <button
              key={o.value}
              className={o.kind === "danger" ? "btn danger" : o.kind === "primary" ? "btn primary" : "btn"}
              onClick={() => answer(o.value)}
              autoFocus={i === choice.options.length - 1}
            >
              {o.label}
            </button>
          ))}
        </div>
      </motion.div>
    </motion.div>
  );
}
