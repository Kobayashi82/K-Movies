import { AnimatePresence, motion } from "motion/react";
import { useEffect, useMemo, useState } from "react";
import { fileUrl, type Item } from "../api";
import type { View } from "../filters";
import { useDebounced } from "../hooks";
import { useStore } from "../store";

/** Con el ratón quieto este tiempo sobre una película, el fondo pasa a ser el suyo. */
const HOVER_DELAY = 1000;

/** La película que pone el fondo al entrar en una pestaña: en Vistas, la última que viste; en
 *  Pendientes, la última que añadiste; en Películas, la primera de la rejilla. */
function initial(items: Item[], view: View): Item | undefined {
  const withImage = items.filter((i) => i.backdrop || i.frame);
  const latest = (key: (i: Item) => string | null) =>
    withImage.reduce<Item | undefined>((best, i) => ((key(i) ?? "") > (best ? (key(best) ?? "") : "") ? i : best), undefined);
  if (view === "watched") return latest((i) => i.lastWatched) ?? withImage[0];
  if (view === "pending") return latest((i) => i.addedAt) ?? withImage[0];
  return withImage[0];
}

/**
 * Fondo de la app: el backdrop de la película bajo el ratón (o la abierta), con un fundido.
 * Con el ratón se espera un segundo antes de cambiar, para que pasar por encima de muchas tarjetas
 * no provoque una sucesión de fundidos. `items`: lo que se ve en la pestaña. Si la última película
 * ya no está ahí (p. ej. se acaba de borrar o se ha cambiado de pestaña), se usa la de `initial`.
 */
export function Backdrop({ items }: { items: Item[] }) {
  const view = useStore((s) => s.view);
  const selectedId = useStore((s) => s.selectedId);
  const hoveredId = useStore((s) => s.hoveredId);
  const all = useStore((s) => s.items);
  // El ratón cuenta cuando lleva un segundo sobre la misma película (si solo pasa por encima, no).
  const settled = useDebounced(hoveredId, HOVER_DELAY);
  const wanted = selectedId ?? (settled === hoveredId ? hoveredId : null);
  // Si el ratón sale de la rejilla se mantiene el último fondo en vez de volver a negro.
  const [last, setLast] = useState<number | null>(null);
  useEffect(() => {
    if (wanted != null) setLast(wanted);
  }, [wanted]);
  // Al cambiar de pestaña se empieza de nuevo.
  useEffect(() => setLast(null), [view]);

  // `undefined`: se deja el que haya (la película no tiene fondo); `null`: ninguno (no hay nada que ver).
  const target = useMemo(() => {
    const id = wanted ?? last;
    const shown = id != null && (id === selectedId || items.some((i) => i.id === id));
    const item = shown ? all.find((i) => i.id === id) : initial(items, view);
    return item ? (item.backdrop ?? item.frame ?? undefined) : null;
  }, [wanted, last, selectedId, items, all, view]);
  const debounced = useDebounced(target, 180);
  const [src, setSrc] = useState<string | null>(null);
  useEffect(() => {
    if (debounced !== undefined) setSrc(debounced);
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
