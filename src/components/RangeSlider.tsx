import type { Range } from "../filters";

interface Props {
  bounds: Range;
  value: Range | null;
  step?: number;
  format?: (v: number) => string;
  onChange: (value: Range | null) => void;
}

/** Deslizador de dos asas. Si cubre todo el rango, el filtro se desactiva (`null`). */
export function RangeSlider({ bounds, value, step = 1, format = String, onChange }: Props) {
  const [min, max] = bounds;
  const [lo, hi] = value ?? bounds;
  const span = max - min || 1;
  const set = (a: number, b: number) => {
    const next: Range = [Math.min(a, b), Math.max(a, b)];
    onChange(next[0] <= min && next[1] >= max ? null : next);
  };
  return (
    <div className="range">
      <div className="range-values">
        <span>{format(lo)}</span>
        <span>{format(hi)}</span>
      </div>
      <div
        className="range-track"
        style={{ ["--lo" as string]: `${((lo - min) / span) * 100}%`, ["--hi" as string]: `${((hi - min) / span) * 100}%` }}
      >
        <input type="range" min={min} max={max} step={step} value={lo} onChange={(e) => set(Number(e.target.value), hi)} />
        <input type="range" min={min} max={max} step={step} value={hi} onChange={(e) => set(lo, Number(e.target.value))} />
      </div>
    </div>
  );
}
