/**
 * Tooltips propios de la app en lugar de los del navegador. Los nativos (`title`) de WebView2 van en
 * una ventana aparte de Windows: no siempre salen (se pierden si el elemento se repinta mientras el
 * ratón espera) y, al aparecer, el botón pierde y recupera el `:hover` (parece que se ilumina dos
 * veces). En los componentes se sigue escribiendo `title`, pero nunca llega al DOM como tal: se
 * guarda en `data-tip` (así el navegador no tiene nada que enseñar) y lo enseña esto. Admiten
 * varias líneas (`\n`).
 */

const DELAY = 500;

/** `setAttribute("title")` y `removeAttribute("title")` (lo que usa React) pasan a `data-tip`. */
function redirectTitles() {
  const proto = Element.prototype;
  const set = proto.setAttribute;
  const remove = proto.removeAttribute;
  proto.setAttribute = function (name: string, value: string) {
    if (name === "title" && this instanceof HTMLElement) return set.call(this, "data-tip", value);
    return set.call(this, name, value);
  };
  proto.removeAttribute = function (name: string) {
    if (name === "title" && this instanceof HTMLElement) return remove.call(this, "data-tip");
    return remove.call(this, name);
  };
  // Por si alguno ya estaba en el HTML.
  for (const el of document.querySelectorAll<HTMLElement>("[title]")) {
    set.call(el, "data-tip", el.getAttribute("title") ?? "");
    remove.call(el, "title");
  }
}

export function installTooltips() {
  redirectTitles();
  const tip = document.createElement("div");
  tip.className = "tooltip";
  tip.setAttribute("role", "tooltip");
  document.body.appendChild(tip);

  let current: HTMLElement | null = null;
  let timer = 0;

  const show = () => {
    const el = current;
    const text = el?.isConnected ? el.dataset.tip : null;
    if (!el || !text) return tip.classList.remove("show");
    tip.textContent = text;
    tip.classList.add("show");
    const r = el.getBoundingClientRect();
    const t = tip.getBoundingClientRect();
    const margin = 8;
    const left = Math.max(margin, Math.min(r.left + r.width / 2 - t.width / 2, window.innerWidth - t.width - margin));
    // Debajo; si no cabe, encima.
    let top = r.bottom + 8;
    if (top + t.height > window.innerHeight - margin) top = r.top - t.height - 8;
    tip.style.left = `${Math.round(left)}px`;
    tip.style.top = `${Math.round(Math.max(margin, top))}px`;
  };

  // Si el texto cambia con el ratón encima (p. ej. las estrellas), se actualiza.
  const watcher = new MutationObserver(() => tip.classList.contains("show") && show());

  const hide = () => {
    window.clearTimeout(timer);
    watcher.disconnect();
    tip.classList.remove("show");
    current = null;
  };

  document.addEventListener("pointerover", (e) => {
    const el = (e.target as Element).closest<HTMLElement>("[data-tip]");
    if (el === current) return;
    hide();
    if (!el?.dataset.tip) return;
    current = el;
    watcher.observe(el, { attributes: true, attributeFilter: ["data-tip"] });
    timer = window.setTimeout(show, DELAY);
  });
  document.addEventListener("pointerleave", hide);
  for (const ev of ["pointerdown", "wheel", "keydown", "blur"] as const) window.addEventListener(ev, hide, true);
  window.addEventListener("scroll", hide, true);
}
