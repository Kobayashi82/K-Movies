import { useVirtualizer } from "@tanstack/react-virtual";
import { useEffect, useMemo, useRef, useState, type ReactNode } from "react";
import { play } from "../actions";
import type { Item } from "../api";
import { useElementWidth } from "../hooks";
import { useStore } from "../store";
import { PosterCard, type CardDisplay } from "./PosterCard";

const PADDING = 28;
// Alto del texto bajo el póster: separación + una línea (título y año van en la misma).
const infoHeight = (d: CardDisplay) => (d.title || d.year ? 8 + 22 : 0);

/**
 * Rejilla virtualizada por filas: solo existen en el DOM las filas visibles (más unas pocas
 * de margen), así que va igual de fluida con 50 películas que con 10 000.
 * `header` (p. ej. "Seguir viendo") va encima de la primera fila y se desplaza con la rejilla.
 */
export function Grid({ items, header }: { items: Item[]; header?: ReactNode }) {
  const scrollRef = useRef<HTMLDivElement>(null);
  const width = useElementWidth(scrollRef);
  const [headerEl, setHeaderEl] = useState<HTMLDivElement | null>(null);
  const [measured, setMeasured] = useState(0);
  useEffect(() => {
    if (!headerEl) return;
    const ro = new ResizeObserver(([entry]) => setMeasured(entry.borderBoxSize[0]?.blockSize ?? entry.contentRect.height));
    ro.observe(headerEl);
    return () => ro.disconnect();
  }, [headerEl]);
  const headerHeight = header ? measured : 0;
  const gridSize = useStore((s) => s.gridSize);
  const view = useStore((s) => s.view);
  const settings = useStore((s) => s.settings);
  const display = useMemo<CardDisplay>(
    () => ({
      rating: (["tmdb", "both", "none"] as const).find((r) => r === settings.posterRating) ?? "imdb",
      title: settings.posterTitle !== "false",
      // El año va siempre a la derecha del título; solo se elige si se ve.
      year: settings.posterYear !== "false" && settings.posterYear !== "none",
    }),
    [settings.posterRating, settings.posterTitle, settings.posterYear],
  );

  const gap = Math.round(Math.min(28, Math.max(14, gridSize * 0.11)));
  const inner = Math.max(0, width - PADDING * 2);
  const cols = Math.max(1, Math.floor((inner + gap) / (gridSize + gap)));
  const cardWidth = cols > 0 ? (inner - gap * (cols - 1)) / cols : gridSize;
  const rowHeight = Math.round(cardWidth * 1.5 + infoHeight(display) + gap);
  const rows = Math.ceil(items.length / cols);

  const virtualizer = useVirtualizer({
    count: rows,
    getScrollElement: () => scrollRef.current,
    estimateSize: () => rowHeight,
    overscan: 3,
    paddingStart: 8 + headerHeight,
    paddingEnd: 40,
  });

  useEffect(() => {
    virtualizer.measure();
  }, [rowHeight, cols, headerHeight, virtualizer]);
  useEffect(() => {
    scrollRef.current?.scrollTo({ top: 0 });
  }, [view]);

  // Teclado: flechas para moverse por los pósters, Re Pág / Av Pág de pantalla en pantalla,
  // Inicio / Fin, Intro abre la ficha (es un botón) y P reproduce.
  const cursor = useRef<number | null>(null);
  const focusCard = (index: number) => {
    cursor.current = index;
    virtualizer.scrollToIndex(Math.floor(index / cols), { align: "auto" });
    // La fila puede no existir aún (virtualizada): se reintenta unos fotogramas.
    let tries = 0;
    const attempt = () => {
      const card = scrollRef.current?.querySelector<HTMLElement>(`[data-index="${index}"]`);
      if (card) card.focus({ preventScroll: true });
      else if (++tries < 10) requestAnimationFrame(attempt);
    };
    requestAnimationFrame(attempt);
  };
  useEffect(() => {
    const onKey = (e: KeyboardEvent) => {
      const s = useStore.getState();
      if (s.selectedId != null || s.dialog || s.settingsOpen || s.trailer || s.choice) return;
      if (e.ctrlKey || e.altKey || e.metaKey || !items.length) return;
      const target = e.target as HTMLElement;
      const inGrid = !!scrollRef.current?.contains(target) && target.closest("[data-index]") != null;
      // Fuera de la rejilla solo si no hay nada con el foco (para no quitarle las flechas a otros controles).
      if (!inGrid && target !== document.body) return;
      const current = inGrid ? cursor.current : null;
      const last = items.length - 1;
      const page = Math.max(1, Math.floor((scrollRef.current?.clientHeight ?? rowHeight) / rowHeight)) * cols;
      const from = current ?? -1;
      let next: number;
      switch (e.key) {
        case "ArrowRight":
          next = from + 1;
          break;
        case "ArrowLeft":
          next = current == null ? 0 : from - 1;
          break;
        case "ArrowDown":
          // Desde la penúltima fila, a la última aunque esté a medias.
          next = current == null ? 0 : from + cols > last && Math.floor(from / cols) < Math.floor(last / cols) ? last : from + cols;
          break;
        case "ArrowUp":
          next = current == null ? 0 : from - cols;
          break;
        case "PageDown":
          next = Math.min(last, Math.max(0, from) + page);
          break;
        case "PageUp":
          next = Math.max(0, from - page);
          break;
        case "Home":
          next = 0;
          break;
        case "End":
          next = last;
          break;
        case "p":
        case "P":
          if (current == null || !items[current] || items[current].fileCount === 0) return;
          e.preventDefault();
          void play(items[current].id);
          return;
        default:
          return;
      }
      e.preventDefault();
      if (next >= 0 && next <= last) focusCard(next);
    };
    window.addEventListener("keydown", onKey);
    return () => window.removeEventListener("keydown", onKey);
  });

  return (
    <div
      ref={scrollRef}
      className="grid-scroll"
      onMouseLeave={() => useStore.getState().hover(null)}
      onFocus={(e) => {
        const index = (e.target as HTMLElement).closest<HTMLElement>("[data-index]")?.dataset.index;
        if (index != null) cursor.current = Number(index);
      }}
    >
      <div style={{ height: virtualizer.getTotalSize(), position: "relative" }}>
        {header && (
          <div ref={setHeaderEl} className="grid-header" style={{ padding: `8px ${PADDING}px 0` }}>
            {header}
          </div>
        )}
        {virtualizer.getVirtualItems().map((row) => (
          <div
            key={row.key}
            className="grid-row"
            style={{
              transform: `translateY(${row.start}px)`,
              height: rowHeight - gap,
              gap,
              padding: `0 ${PADDING}px`,
              gridTemplateColumns: `repeat(${cols}, minmax(0, 1fr))`,
            }}
          >
            {items.slice(row.index * cols, row.index * cols + cols).map((item, i) => (
              <PosterCard key={item.id} item={item} display={display} index={row.index * cols + i} />
            ))}
          </div>
        ))}
      </div>
    </div>
  );
}
