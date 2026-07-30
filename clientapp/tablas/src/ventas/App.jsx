import React, { useCallback } from "react";
import { useTablaInteractiva } from "../shared/useTablaInteractiva.js";
import EncabezadoOrdenable from "../shared/EncabezadoOrdenable.jsx";
import Paginacion from "../shared/Paginacion.jsx";
import BuscadorTabla from "../shared/BuscadorTabla.jsx";
import { formatearMoneda } from "../shared/formato.js";

const ORDENADORES = {
  id: (v) => v.id,
  fecha: (v) => v.fechaOrden,
  sucursal: (v) => v.sucursal?.toLowerCase() ?? "",
  usuario: (v) => v.usuarioNombre?.toLowerCase() ?? "",
  total: (v) => v.total
};

const METODO_PAGO = {
  1: { texto: "Efectivo", clase: "bg-success", icono: "bi-cash-stack" },
  2: { texto: "QR", clase: "bg-primary", icono: "bi-qr-code" },
  3: { texto: "Tarjeta", clase: "bg-dark", icono: "bi-credit-card" }
};

export default function App({ ventas, detalleUrlBase }) {
  const textoBusqueda = useCallback(
    (v) =>
      [
        `#${v.id}`,
        v.sucursal,
        v.usuarioNombre,
        v.usuarioEmail,
        ...v.productos.map((p) => p.nombre)
      ]
        .filter(Boolean)
        .join(" "),
    []
  );

  const tabla = useTablaInteractiva(ventas, {
    textoBusqueda,
    ordenadores: ORDENADORES,
    tamanoPagina: 10
  });

  if (ventas.length === 0) {
    return null;
  }

  return (
    <div>
      <div className="ti-toolbar">
        <BuscadorTabla
          query={tabla.query}
          onChange={tabla.cambiarQuery}
          placeholder="Buscar por N.º, sucursal, usuario o producto..."
        />
      </div>

      {tabla.filasPagina.length === 0 ? (
        <div className="ti-vacio">
          <i className="bi bi-search"></i>
          No se encontraron ventas que coincidan con "{tabla.query}".
        </div>
      ) : (
        <div className="table-responsive">
          <table className="table align-middle">
            <thead>
              <tr>
                <EncabezadoOrdenable
                  columna="id"
                  orden={tabla.orden}
                  onOrdenar={tabla.alternarOrden}
                >
                  N.º
                </EncabezadoOrdenable>

                <EncabezadoOrdenable
                  columna="fecha"
                  orden={tabla.orden}
                  onOrdenar={tabla.alternarOrden}
                >
                  Fecha
                </EncabezadoOrdenable>

                <EncabezadoOrdenable
                  columna="sucursal"
                  orden={tabla.orden}
                  onOrdenar={tabla.alternarOrden}
                >
                  Sucursal
                </EncabezadoOrdenable>

                <EncabezadoOrdenable
                  columna="usuario"
                  orden={tabla.orden}
                  onOrdenar={tabla.alternarOrden}
                >
                  Usuario
                </EncabezadoOrdenable>

                <th>Productos</th>
                <th>Método de pago</th>

                <EncabezadoOrdenable
                  columna="total"
                  orden={tabla.orden}
                  onOrdenar={tabla.alternarOrden}
                  className="text-end"
                >
                  Total
                </EncabezadoOrdenable>

                <th className="text-end">Acciones</th>
              </tr>
            </thead>

            <tbody>
              {tabla.filasPagina.map((venta) => {
                const metodo =
                  METODO_PAGO[venta.metodoPago] ?? {
                    texto: "No especificado",
                    clase: "bg-secondary",
                    icono: "bi-question-circle"
                  };

                return (
                  <tr key={venta.id}>
                    <td>
                      <span className="fw-semibold">#{venta.id}</span>
                    </td>

                    <td>
                      <div>{venta.fechaTexto}</div>
                      <small className="text-muted">{venta.horaTexto}</small>
                    </td>

                    <td>
                      <i className="bi bi-shop me-1 text-muted"></i>
                      {venta.sucursal}
                    </td>

                    <td>
                      <div className="d-flex align-items-center">
                        <i className="bi bi-person-circle fs-5 me-2 text-muted"></i>
                        <div>
                          <div className="fw-semibold">
                            {venta.usuarioNombre}
                          </div>

                          {venta.usuarioEmail &&
                            venta.usuarioEmail !== venta.usuarioNombre && (
                              <small className="text-muted">
                                {venta.usuarioEmail}
                              </small>
                            )}
                        </div>
                      </div>
                    </td>

                    <td>
                      {venta.productos.map((detalle, indice) => (
                        <div className="mb-1" key={indice}>
                          <span className="fw-semibold">
                            {detalle.cantidad} ×
                          </span>{" "}
                          {detalle.nombre}{" "}
                          <small className="text-muted">
                            ({detalle.precioUnitario.toFixed(2)})
                          </small>
                        </div>
                      ))}
                    </td>

                    <td>
                      <span className={`badge ${metodo.clase}`}>
                        <i className={`bi ${metodo.icono} me-1`}></i>
                        {metodo.texto}
                      </span>
                    </td>

                    <td className="text-end">
                      <span className="fw-bold">
                        {formatearMoneda(venta.total)}
                      </span>
                    </td>

                    <td className="text-end">
                      <a
                        href={`${detalleUrlBase}${venta.id}`}
                        className="btn btn-outline-primary btn-sm"
                        title="Ver comprobante"
                      >
                        <i className="bi bi-eye"></i>
                      </a>
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>
      )}

      <Paginacion
        pagina={tabla.pagina}
        totalPaginas={tabla.totalPaginas}
        onCambiar={tabla.setPagina}
        totalFiltrado={tabla.totalFiltrado}
        totalOriginal={tabla.totalOriginal}
        etiqueta="ventas"
      />
    </div>
  );
}
