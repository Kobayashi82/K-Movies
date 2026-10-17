import { Star } from "lucide-react";
import { useState } from "react";
import { useTranslation } from "react-i18next";
import { formatNumber } from "../util";

const FIVE = [0, 1, 2, 3, 4];

/**
 * Tu nota en estrellas, de 0,5 a 5 en medias, como Letterboxd. La mitad izquierda de una estrella
 * es media. Pulsar la nota que ya tiene la quita. Sin `onChange`, solo se muestra.
 */
export function Stars({ value, onChange, size = 16 }: { value: number | null; onChange?: (v: number | null) => void; size?: number }) {
  const { t } = useTranslation();
  const [hover, setHover] = useState<number | null>(null);
  const shown = hover ?? value ?? 0;
  const label = value ? t("rating.value", { value: formatNumber(value) }) : t("rating.none");
  const stars = (fill: boolean) => FIVE.map((i) => <Star key={i} size={size} fill={fill ? "currentColor" : "none"} strokeWidth={fill ? 0 : 1.6} />);
  const row = (
    <>
      <span className="stars-base">{stars(false)}</span>
      <span className="stars-fill" style={{ width: `${(shown / 5) * 100}%` }}>
        {stars(true)}
      </span>
    </>
  );
  if (!onChange) {
    return (
      <span className="stars" role="img" aria-label={label} title={label}>
        {row}
      </span>
    );
  }

  const at = (e: React.MouseEvent<HTMLElement>) => {
    const rect = e.currentTarget.getBoundingClientRect();
    return Math.min(5, Math.max(0.5, Math.ceil(((e.clientX - rect.left) / rect.width) * 10) / 2));
  };
  const set = (v: number | null) => onChange(v === value ? null : v);
  const step = (delta: number) => onChange(Math.min(5, Math.max(0.5, (value ?? 0) + delta)));
  return (
    <span
      className="stars editable"
      role="slider"
      tabIndex={0}
      aria-label={t("rating.label")}
      aria-valuemin={0.5}
      aria-valuemax={5}
      aria-valuenow={value ?? undefined}
      aria-valuetext={label}
      title={hover ? t("rating.value", { value: formatNumber(hover) }) : value ? t("rating.change") : t("rating.rate")}
      onMouseMove={(e) => setHover(at(e))}
      onMouseLeave={() => setHover(null)}
      onClick={(e) => {
        e.stopPropagation();
        set(at(e));
      }}
      onKeyDown={(e) => {
        if (e.key === "ArrowRight" || e.key === "ArrowUp") step(0.5);
        else if (e.key === "ArrowLeft" || e.key === "ArrowDown") step(-0.5);
        else if (e.key === "Delete" || e.key === "Backspace") onChange(null);
        else return;
        e.preventDefault();
        e.stopPropagation();
      }}
    >
      {row}
    </span>
  );
}
