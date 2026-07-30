import React from "react";

export default function EncabezadoOrdenable({
  columna,
  orden,
  onOrdenar,
  className,
  children
}) {
  const activo = orden.columna === columna;

  return (
    <th
      className={`ti-th-ordenable ${className ?? ""}`}
      onClick={() => onOrdenar(columna)}
      role="button"
      tabIndex={0}
      onKeyDown={(evento) => {
        if (evento.key === "Enter" || evento.key === " ") {
          evento.preventDefault();
          onOrdenar(columna);
        }
      }}
    >
      <span className="ti-th-contenido">
        {children}
        <i
          className={`bi ti-th-icono ${
            activo
              ? orden.direccion === "asc"
                ? "bi-caret-up-fill"
                : "bi-caret-down-fill"
              : "bi-caret-down"
          } ${activo ? "activo" : ""}`}
        ></i>
      </span>
    </th>
  );
}
