import { motion } from "motion/react";
import { useEffect, useLayoutEffect, useRef, useState } from "react";
import { useStore, type ContextMenu as Menu, type MenuEntry } from "../store";

const MARGIN = 8;

/** Menú del botón derecho con el estilo de la app. Se cierra al elegir, al pulsar fuera, con Esc o al desplazar. */
export function ContextMenu({ menu }: { menu: Menu }) {
  const ref = useRef<HTMLDivElement>(null);
  const [pos, setPos] = useState({ left: menu.x, top: menu.y });
  const close = () => useStore.setState({ menu: null });

  // Que no se salga de la ventana: si no cabe a la derecha o abajo, se abre hacia el otro lado.
  useLayoutEffect(() => {
    const el = ref.current;
    if (!el) return;
    const { width, height } = el.getBoundingClientRect();
    const left = menu.x + width + MARGIN > window.innerWidth ? Math.max(MARGIN, menu.x - width) : menu.x;
    const top = menu.y + height + MARGIN > window.innerHeight ? Math.max(MARGIN, menu.y - height) : menu.y;
    setPos({ left, top });
  }, [menu]);

  useEffect(() => {
    const onDown = (e: PointerEvent) => {
      if (!ref.current?.contains(e.target as Node)) close();
    };
    const onKey = (e: KeyboardEvent) => {
      if (e.key === "Escape") {
        e.stopPropagation();
        close();
      }
    };
    window.addEventListener("pointerdown", onDown, true);
    window.addEventListener("keydown", onKey, true);
    window.addEventListener("wheel", close, { passive: true });
    window.addEventListener("blur", close);
    window.addEventListener("resize", close);
    ref.current?.querySelector<HTMLButtonElement>("button:not(:disabled)")?.focus({ preventScroll: true });
    return () => {
      window.removeEventListener("pointerdown", onDown, true);
      window.removeEventListener("keydown", onKey, true);
      window.removeEventListener("wheel", close);
      window.removeEventListener("blur", close);
      window.removeEventListener("resize", close);
    };
  }, []);

  // Flechas arriba y abajo para moverse por las opciones.
  const onKeyDown = (e: React.KeyboardEvent) => {
    if (e.key !== "ArrowDown" && e.key !== "ArrowUp") return;
    e.preventDefault();
    e.stopPropagation();
    const buttons = [...(ref.current?.querySelectorAll<HTMLButtonElement>("button:not(:disabled)") ?? [])];
    const i = buttons.indexOf(document.activeElement as HTMLButtonElement);
    const next = e.key === "ArrowDown" ? (i + 1) % buttons.length : (i - 1 + buttons.length) % buttons.length;
    buttons[next]?.focus();
  };

  // Sin separadores al principio, al final ni dos seguidos (las opciones que no tocan ya se han quitado).
  const entries: MenuEntry[] = [];
  for (const e of menu.entries) if (e !== null || (entries.length && entries[entries.length - 1] !== null)) entries.push(e);
  while (entries.length && entries[entries.length - 1] === null) entries.pop();

  return (
    <motion.div
      ref={ref}
      className="context-menu"
      role="menu"
      style={pos}
      initial={{ opacity: 0, scale: 0.97 }}
      animate={{ opacity: 1, scale: 1 }}
      transition={{ duration: 0.1 }}
      onKeyDown={onKeyDown}
      onContextMenu={(e) => e.preventDefault()}
    >
      {entries.map((entry, i) =>
        entry === null ? (
          <div key={`sep${i}`} className="context-sep" role="separator" />
        ) : (
          <button
            key={entry.label}
            role="menuitem"
            className={entry.danger ? "context-item danger" : "context-item"}
            disabled={entry.disabled}
            onClick={() => {
              close();
              void entry.action();
            }}
          >
            {entry.icon ? <entry.icon size={15} /> : <span className="context-icon" />}
            <span>{entry.label}</span>
          </button>
        ),
      )}
    </motion.div>
  );
}
