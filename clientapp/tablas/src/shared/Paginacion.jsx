import React from "react";

export default function Paginacion({
  pagina,
  totalPaginas,
  onCambiar,
  totalFiltrado,
  totalOriginal,
  etiqueta = "resultados"
}) {
  return (
    <div className="ti-pie">
      <span className="text-muted small">
        {totalFiltrado === totalOriginal
          ? `${totalOriginal} ${etiqueta}`
          : `${totalFiltrado} de ${totalOriginal} ${etiqueta}`}
      </span>

      {totalPaginas > 1 && (
        <div className="btn-group" role="group">
          <button
            type="button"
            className="btn btn-sm btn-light"
            disabled={pagina <= 1}
            onClick={() => onCambiar(pagina - 1)}
          >
            <i className="bi bi-chevron-left"></i>
          </button>

          <span className="btn btn-sm btn-light disabled">
            Página {pagina} de {totalPaginas}
          </span>

          <button
            type="button"
            className="btn btn-sm btn-light"
            disabled={pagina >= totalPaginas}
            onClick={() => onCambiar(pagina + 1)}
          >
            <i className="bi bi-chevron-right"></i>
          </button>
        </div>
      )}
    </div>
  );
}
