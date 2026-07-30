import React from "react";

export default function BuscadorTabla({ query, onChange, placeholder }) {
  return (
    <div className="ti-buscador">
      <i className="bi bi-search"></i>

      <input
        type="text"
        className="form-control"
        placeholder={placeholder ?? "Buscar..."}
        value={query}
        onChange={(evento) => onChange(evento.target.value)}
      />

      {query && (
        <button
          type="button"
          className="ti-buscador-limpiar"
          onClick={() => onChange("")}
          aria-label="Limpiar búsqueda"
        >
          <i className="bi bi-x-circle-fill"></i>
        </button>
      )}
    </div>
  );
}
