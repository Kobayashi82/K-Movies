import { AnimatePresence, motion } from "motion/react";
import { useEffect, useState } from "react";
import { fileUrl } from "../api";
import { useDebounced } from "../hooks";
import { useStore } from "../store";

/**
 * Fondo de la app: el backdrop de la película bajo el ratón (o la abierta), con un fundido.
 * Se espera un instante antes de cambiar para que pasar el ratón por encima de muchas
 * tarjetas no provoque una sucesión de fundidos.
 */
export function Backdrop() {
  const target = useStore((s) => {
    const id = s.selectedId ?? s.hoveredId;
    return id == null ? null : (s.items.find((i) => i.id === id)?.backdrop ?? null);
  });
  const debounced = useDebounced(target, 180);
  // Si el ratón sale de la rejilla mantenemos el último fondo en vez de volver a negro.
  const [src, setSrc] = useState<string | null>(null);
  useEffect(() => {
    if (debounced) setSrc(debounced);
  }, [debounced]);

  const url = fileUrl(src);
  return (
    <div className="backdrop" aria-hidden="true">
      <AnimatePresence>
        {url && (
          <motion.img
            key={url}
            src={url}
            alt=""
            initial={{ opacity: 0, scale: 1.04 }}
            animate={{ opacity: 1, scale: 1 }}
            exit={{ opacity: 0 }}
            transition={{ duration: 0.9, ease: [0.22, 1, 0.36, 1] }}
          />
        )}
      </AnimatePresence>
      <div className="backdrop-shade" />
    </div>
  );
}
