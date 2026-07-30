import React, { useCallback } from "react";
import { useTablaInteractiva } from "../shared/useTablaInteractiva.js";
import EncabezadoOrdenable from "../shared/EncabezadoOrdenable.jsx";
import Paginacion from "../shared/Paginacion.jsx";
import BuscadorTabla from "../shared/BuscadorTabla.jsx";

const ORDENADORES = {
  nombre: (i) => i.nombre?.toLowerCase() ?? "",
  sucursal: (i) => i.sucursal?.toLowerCase() ?? "",
  stock: (i) => i.stock,
  stockMinimo: (i) => i.stockMinimo,
  fecha: (i) => i.fechaOrden
};

export default function App({ inventarios, editarUrlBase }) {
  const textoBusqueda = useCallback(
    (i) => [i.nombre, i.codigo, i.sucursal].filter(Boolean).join(" "),
    []
  );

  const tabla = useTablaInteractiva(inventarios, {
    textoBusqueda,
    ordenadores: ORDENADORES,
    tamanoPagina: 10
  });

  if (inventarios.length === 0) {
    return null;
  }

  return (
    <div>
      <div className="ti-toolbar">
        <BuscadorTabla
          query={tabla.query}
          onChange={tabla.cambiarQuery}
          placeholder="Buscar por producto, código o sucursal..."
        />
      </div>

      {tabla.filasPagina.length === 0 ? (
        <div className="ti-vacio">
          <i className="bi bi-search"></i>
          No se encontraron productos que coincidan con "{tabla.query}".
        </div>
      ) : (
        <div className="table-responsive">
          <table className="table align-middle">
            <thead>
              <tr>
                <EncabezadoOrdenable
                  columna="nombre"
                  orden={tabla.orden}
                  onOrdenar={tabla.alternarOrden}
                >
                  Producto
                </EncabezadoOrdenable>

                <EncabezadoOrdenable
                  columna="sucursal"
                  orden={tabla.orden}
                  onOrdenar={tabla.alternarOrden}
                >
                  Sucursal
                </EncabezadoOrdenable>

                <EncabezadoOrdenable
                  columna="stock"
                  orden={tabla.orden}
                  onOrdenar={tabla.alternarOrden}
                >
                  Stock
                </EncabezadoOrdenable>

                <EncabezadoOrdenable
                  columna="stockMinimo"
                  orden={tabla.orden}
                  onOrdenar={tabla.alternarOrden}
                >
                  Stock mínimo
                </EncabezadoOrdenable>

                <EncabezadoOrdenable
                  columna="fecha"
                  orden={tabla.orden}
                  onOrdenar={tabla.alternarOrden}
                >
                  Actualización
                </EncabezadoOrdenable>

                <th className="text-end">Acciones</th>
              </tr>
            </thead>

            <tbody>
              {tabla.filasPagina.map((inventario) => {
                const stockBajo = inventario.stock <= inventario.stockMinimo;

                const clase =
                  inventario.stock === 0
                    ? "table-danger"
                    : stockBajo
                      ? "table-warning"
                      : "";

                return (
                  <tr key={inventario.id} className={clase}>
                    <td>
                      <strong>{inventario.nombre}</strong>
                      <div className="small text-muted">
                        {inventario.codigo}
                      </div>
                    </td>

                    <td>{inventario.sucursal}</td>

                    <td>
                      <span className="fw-bold">{inventario.stock}</span>
                    </td>

                    <td>{inventario.stockMinimo}</td>

                    <td>{inventario.fechaTexto}</td>

                    <td className="text-end">
                      <a
                        href={`${editarUrlBase}${inventario.id}`}
                        className="btn btn-sm btn-outline-primary"
                      >
                        <i className="bi bi-pencil"></i>
                        Editar
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
        etiqueta="productos en inventario"
      />
    </div>
  );
}
