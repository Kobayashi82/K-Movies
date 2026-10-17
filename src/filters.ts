import type { Item } from "./api";

export type View = "library" | "pending" | "watched";

export type SortKey =
  | "title"
  | "added"
  | "release"
  | "imdb"
  | "tmdb"
  | "runtime"
  | "lastWatched"
  | "size";

export const SORT_KEYS: SortKey[] = ["title", "added", "release", "imdb", "tmdb", "runtime", "lastWatched", "size"];

export type Range = [number, number];

export interface Filters {
  genres: string[];
  directors: string[];
  actors: string[];
  countries: string[];
  languages: string[];
  certifications: string[];
  years: Range | null;
  imdb: Range | null;
  runtime: Range | null;
  watched: "all" | "watched" | "unwatched";
  subs: "all" | "yes" | "no";
  unmatched: boolean;
}

export const emptyFilters = (): Filters => ({
  genres: [],
  directors: [],
  actors: [],
  countries: [],
  languages: [],
  certifications: [],
  years: null,
  imdb: null,
  runtime: null,
  watched: "all",
  subs: "all",
  unmatched: false,
});

export type ListFacet = "genres" | "directors" | "actors" | "countries" | "languages" | "certifications";
export type RangeFacet = "years" | "imdb" | "runtime";

export const LIST_FACETS: ListFacet[] = ["genres", "directors", "actors", "countries", "languages", "certifications"];

export function activeFilterCount(f: Filters): number {
  return (
    LIST_FACETS.reduce((n, k) => n + f[k].length, 0) +
    (f.years ? 1 : 0) +
    (f.imdb ? 1 : 0) +
    (f.runtime ? 1 : 0) +
    (f.watched !== "all" ? 1 : 0) +
    (f.subs !== "all" ? 1 : 0) +
    (f.unmatched ? 1 : 0)
  );
}

/** Minúsculas y sin acentos, para buscar "amelie" y encontrar "Amélie". */
export function fold(s: string): string {
  return s.normalize("NFD").replace(/[̀-ͯ]/g, "").toLowerCase();
}

export function inView(item: Item, view: View): boolean {
  switch (view) {
    case "library":
      return item.fileCount > 0;
    case "pending":
      return item.watchlist;
    case "watched":
      return item.watchCount > 0;
  }
}

function facetValues(item: Item, facet: ListFacet): string[] {
  switch (facet) {
    case "genres":
      return item.genres;
    case "directors":
      return item.directors;
    case "actors":
      return item.cast;
    case "countries":
      return item.countries;
    case "languages":
      return item.languages;
    case "certifications":
      return item.certification ? [item.certification] : [];
  }
}

const inRange = (v: number | null, r: Range | null) => !r || (v != null && v >= r[0] && v <= r[1]);

/**
 * Dentro de una misma categoría basta con que coincida uno de los valores (O); entre
 * categorías tienen que cumplirse todas (Y). Los géneros son la excepción: elegir
 * "Comedia" y "Romance" busca películas que sean las dos cosas.
 */
export function matches(item: Item, f: Filters, query: string): boolean {
  for (const facet of LIST_FACETS) {
    const wanted = f[facet];
    if (!wanted.length) continue;
    const values = facetValues(item, facet);
    const ok = facet === "genres" ? wanted.every((w) => values.includes(w)) : wanted.some((w) => values.includes(w));
    if (!ok) return false;
  }
  if (!inRange(item.year, f.years)) return false;
  if (!inRange(item.imdbRating, f.imdb)) return false;
  if (!inRange(item.runtime, f.runtime)) return false;
  if (f.watched === "watched" && item.watchCount === 0) return false;
  if (f.watched === "unwatched" && item.watchCount > 0) return false;
  if (f.subs === "yes" && !item.hasSubs) return false;
  if (f.subs === "no" && item.hasSubs) return false;
  if (f.unmatched && item.matched > 0) return false;
  if (query) {
    const haystack = fold(
      [item.title, item.originalTitle ?? "", item.year ?? "", ...item.directors, ...item.cast].join(" "),
    );
    if (!fold(query).split(/\s+/).every((word) => haystack.includes(word))) return false;
  }
  return true;
}

const collator = new Intl.Collator(undefined, { sensitivity: "base", numeric: true, ignorePunctuation: true });

function sortValue(item: Item, key: SortKey): number | string | null {
  switch (key) {
    case "title":
      return item.title;
    case "added":
      return item.addedAt;
    case "release":
      return item.releaseDate ?? (item.year ? String(item.year) : null);
    case "imdb":
      return item.imdbRating;
    case "tmdb":
      return item.tmdbRating;
    case "runtime":
      return item.runtime;
    case "lastWatched":
      return item.lastWatched;
    case "size":
      return item.totalSize;
  }
}

/** Ordena; los valores vacíos van siempre al final y el título desempata. */
export function sortItems(items: Item[], key: SortKey, desc: boolean): Item[] {
  const dir = desc ? -1 : 1;
  return [...items].sort((a, b) => {
    const va = sortValue(a, key);
    const vb = sortValue(b, key);
    if (va == null && vb != null) return 1;
    if (vb == null && va != null) return -1;
    let c = 0;
    if (va != null && vb != null) {
      c = typeof va === "number" && typeof vb === "number" ? va - vb : collator.compare(String(va), String(vb));
    }
    return c !== 0 ? c * dir : collator.compare(a.title, b.title);
  });
}

/** Ordenación por defecto al elegir un criterio: lo más reciente / mejor nota primero. */
export const defaultDesc = (key: SortKey) => key !== "title";

export interface FacetOption {
  value: string;
  count: number;
}

/** Valores posibles de cada categoría, con cuántas películas tienen cada uno. */
export function facetOptions(items: Item[], facet: ListFacet): FacetOption[] {
  const counts = new Map<string, number>();
  for (const item of items) for (const v of facetValues(item, facet)) counts.set(v, (counts.get(v) ?? 0) + 1);
  return [...counts.entries()]
    .map(([value, count]) => ({ value, count }))
    .sort((a, b) => b.count - a.count || collator.compare(a.value, b.value));
}

export function rangeBounds(items: Item[], facet: RangeFacet): Range | null {
  let min = Infinity;
  let max = -Infinity;
  for (const item of items) {
    const v = facet === "years" ? item.year : facet === "imdb" ? item.imdbRating : item.runtime;
    if (v == null) continue;
    if (v < min) min = v;
    if (v > max) max = v;
  }
  return min <= max ? [Math.floor(min), Math.ceil(max)] : null;
}
