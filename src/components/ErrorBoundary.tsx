import { Component, type ReactNode } from "react";
import i18n from "../i18n";

/** Si un componente falla, muestra el error y un botón para recargar en vez de dejar la ventana en blanco. */
export class ErrorBoundary extends Component<{ children: ReactNode }, { error: Error | null }> {
  state = { error: null as Error | null };

  static getDerivedStateFromError(error: Error) {
    return { error };
  }

  render() {
    if (!this.state.error) return this.props.children;
    return (
      <div className="empty" style={{ height: "100%" }}>
        <p>{i18n.t("common.error")}</p>
        <code style={{ userSelect: "text", maxWidth: 700 }}>{String(this.state.error)}</code>
        <button className="btn" onClick={() => location.reload()}>
          {i18n.t("common.reload")}
        </button>
      </div>
    );
  }
}
