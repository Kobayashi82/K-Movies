import { useEffect, useMemo, useState, type RefObject } from "react";
import { useShallow } from "zustand/react/shallow";
import type { Item, NowPlaying } from "./api";
import { activeFilterCount, inView, matches, sortItems, type View } from "./filters";
import { useStore } from "./store";

/** Películas a medias que siguen en disco, de la última que se vio a la primera. */
export function useContinueItems(): Item[] {
  const items = useStore((s) => s.items);
  return useMemo(
    () =>
      items
        .filter((i) => i.position != null && i.duration && i.fileCount > 0)
        .sort((a, b) => (b.playbackAt ?? "").localeCompare(a.playbackAt ?? "")),
    [items],
  );
}

/** ¿Se enseña "Seguir viendo"? Solo en Películas y sin buscar ni filtrar (ahí estorbaría). */
export function useShowContinue(): boolean {
  return useStore((s) => s.view === "library" && !s.query.trim() && activeFilterCount(s.filters) === 0);
}

/** La sesión de reproducción en curso de esa película, si la hay. */
export function useNowPlaying(itemId: number): NowPlaying | undefined {
  return useStore((s) => s.playing.find((p) => p.itemId === itemId));
}

/** Películas de la pestaña actual, antes de filtrar (sirve para calcular las opciones de filtro). */
export function useViewItems(): Item[] {
  const { items, view } = useStore(useShallow((s) => ({ items: s.items, view: s.view })));
  return useMemo(() => items.filter((i) => inView(i, view)), [items, view]);
}

/** Lo que se ve en la rejilla: pestaña + filtros + búsqueda + orden. */
export function useVisibleItems(): Item[] {
  const viewItems = useViewItems();
  const { filters, query, sortKey, sortDesc } = useStore(
    useShallow((s) => ({ filters: s.filters, query: s.query.trim(), sortKey: s.sortKey, sortDesc: s.sortDesc })),
  );
  return useMemo(
    () => sortItems(viewItems.filter((i) => matches(i, filters, query)), sortKey, sortDesc),
    [viewItems, filters, query, sortKey, sortDesc],
  );
}

export function useViewCounts(): Record<View, number> {
  const items = useStore((s) => s.items);
  return useMemo(() => {
    const counts: Record<View, number> = { library: 0, pending: 0, watched: 0 };
    for (const i of items) for (const v of ["library", "pending", "watched"] as View[]) if (inView(i, v)) counts[v]++;
    return counts;
  }, [items]);
}

export function useElementWidth(ref: RefObject<HTMLElement | null>): number {
  const [width, setWidth] = useState(0);
  useEffect(() => {
    const el = ref.current;
    if (!el) return;
    const ro = new ResizeObserver(([entry]) => setWidth(entry.contentRect.width));
    ro.observe(el);
    return () => ro.disconnect();
  }, [ref]);
  return width;
}

/** Valor que solo cambia cuando lleva `delay` ms estable (evita cambios bruscos al mover el ratón). */
export function useDebounced<T>(value: T, delay: number): T {
  const [v, setV] = useState(value);
  useEffect(() => {
    const id = window.setTimeout(() => setV(value), delay);
    return () => window.clearTimeout(id);
  }, [value, delay]);
  return v;
}
