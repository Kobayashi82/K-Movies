import { useVirtualizer } from "@tanstack/react-virtual";
import { useEffect, useMemo, useRef } from "react";
import type { Item } from "../api";
import { useElementWidth } from "../hooks";
import { useStore } from "../store";
import { PosterCard, type CardDisplay } from "./PosterCard";

const PADDING = 28;
// Alto del texto bajo el póster: separación + una línea (título y año van en la misma).
const infoHeight = (d: CardDisplay) => (d.title || d.year !== "none" ? 8 + 22 : 0);

/**
 * Rejilla virtualizada por filas: solo existen en el DOM las filas visibles (más unas pocas
 * de margen), así que va igual de fluida con 50 películas que con 10 000.
 */
export function Grid({ items }: { items: Item[] }) {
  const scrollRef = useRef<HTMLDivElement>(null);
  const width = useElementWidth(scrollRef);
  const gridSize = useStore((s) => s.gridSize);
  const view = useStore((s) => s.view);
  const settings = useStore((s) => s.settings);
  const display = useMemo<CardDisplay>(
    () => ({
      rating: (["tmdb", "both", "none"] as const).find((r) => r === settings.posterRating) ?? "imdb",
      title: settings.posterTitle !== "false",
      year: (["before", "none"] as const).find((y) => y === settings.posterYear) ?? "after",
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
    paddingStart: 8,
    paddingEnd: 40,
  });

  useEffect(() => {
    virtualizer.measure();
  }, [rowHeight, cols, virtualizer]);
  useEffect(() => {
    scrollRef.current?.scrollTo({ top: 0 });
  }, [view]);

  return (
    <div ref={scrollRef} className="grid-scroll" onMouseLeave={() => useStore.getState().hover(null)}>
      <div style={{ height: virtualizer.getTotalSize(), position: "relative" }}>
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
            {items.slice(row.index * cols, row.index * cols + cols).map((item) => (
              <PosterCard key={item.id} item={item} display={display} />
            ))}
          </div>
        ))}
      </div>
    </div>
  );
}
