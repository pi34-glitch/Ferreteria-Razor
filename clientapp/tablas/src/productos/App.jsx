import React, { useCallback } from "react";
import { useTablaInteractiva } from "../shared/useTablaInteractiva.js";
import EncabezadoOrdenable from "../shared/EncabezadoOrdenable.jsx";
import Paginacion from "../shared/Paginacion.jsx";
import BuscadorTabla from "../shared/BuscadorTabla.jsx";
import { formatearMoneda } from "../shared/formato.js";

const ORDENADORES = {
  nombre: (p) => p.nombre?.toLowerCase() ?? "",
  codigo: (p) => p.codigo?.toLowerCase() ?? "",
  categoria: (p) => p.categoria?.toLowerCase() ?? "",
  marca: (p) => p.marca?.toLowerCase() ?? "",
  precio: (p) => p.precio,
  stock: (p) => (p.stock ?? -1)
};

export default function App({
  productos,
  esAdministrador,
  esGerente,
  detailsUrlBase,
  editUrlBase,
  deleteUrlBase,
  editarInventarioUrlBase
}) {
  const textoBusqueda = useCallback(
    (p) =>
      [p.nombre, p.codigo, p.categoria, p.marca, p.descripcion]
        .filter(Boolean)
        .join(" "),
    []
  );

  const tabla = useTablaInteractiva(productos, {
    textoBusqueda,
    ordenadores: ORDENADORES,
    tamanoPagina: 10
  });

  if (productos.length === 0) {
    return null;
  }

  return (
    <div>
      <div className="ti-toolbar">
        <BuscadorTabla
          query={tabla.query}
          onChange={tabla.cambiarQuery}
          placeholder="Buscar por nombre, código, categoría o marca..."
        />
      </div>

      {tabla.filasPagina.length === 0 ? (
        <div className="ti-vacio">
          <i className="bi bi-search"></i>
          No se encontraron productos que coincidan con "{tabla.query}".
        </div>
      ) : (
        <div className="table-responsive">
          <table className="table table-modern align-middle">
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
                  columna="codigo"
                  orden={tabla.orden}
                  onOrdenar={tabla.alternarOrden}
                >
                  Código
                </EncabezadoOrdenable>

                <EncabezadoOrdenable
                  columna="categoria"
                  orden={tabla.orden}
                  onOrdenar={tabla.alternarOrden}
                >
                  Categoría
                </EncabezadoOrdenable>

                <EncabezadoOrdenable
                  columna="marca"
                  orden={tabla.orden}
                  onOrdenar={tabla.alternarOrden}
                >
                  Marca
                </EncabezadoOrdenable>

                <EncabezadoOrdenable
                  columna="precio"
                  orden={tabla.orden}
                  onOrdenar={tabla.alternarOrden}
                >
                  Precio
                </EncabezadoOrdenable>

                <EncabezadoOrdenable
                  columna="stock"
                  orden={tabla.orden}
                  onOrdenar={tabla.alternarOrden}
                >
                  Stock
                </EncabezadoOrdenable>

                <th>Estado</th>
                <th className="text-end">Acciones</th>
              </tr>
            </thead>

            <tbody>
              {tabla.filasPagina.map((producto) => (
                <tr key={producto.productoId}>
                  <td>
                    <div className="d-flex align-items-center gap-3">
                      {producto.imagenUrl ? (
                        <img
                          src={producto.imagenUrl}
                          alt={producto.nombre}
                          className="product-image"
                        />
                      ) : (
                        <div className="product-image d-grid place-items-center">
                          <i className="bi bi-image text-muted"></i>
                        </div>
                      )}

                      <div>
                        <strong className="d-block">
                          {producto.nombre}
                        </strong>
                        <small className="text-muted">
                          {producto.descripcion || "Sin descripción"}
                        </small>
                      </div>
                    </div>
                  </td>

                  <td>
                    <span className="text-muted">
                      {producto.codigo || "—"}
                    </span>
                  </td>

                  <td>{producto.categoria || "Sin categoría"}</td>
                  <td>{producto.marca || "Sin marca"}</td>

                  <td>
                    <strong>{formatearMoneda(producto.precio)}</strong>
                  </td>

                  <td>
                    {producto.stock !== null &&
                    producto.stock !== undefined ? (
                      <>
                        <div className="fw-semibold">{producto.stock}</div>

                        {producto.stock === 0 ? (
                          <span className="badge bg-danger">Agotado</span>
                        ) : producto.stock <=
                          (producto.stockMinimo ?? 0) ? (
                          <span className="badge bg-warning text-dark">
                            Stock bajo
                          </span>
                        ) : (
                          <span className="badge bg-success">
                            Disponible
                          </span>
                        )}
                      </>
                    ) : (
                      <span className="text-muted">—</span>
                    )}
                  </td>

                  <td>
                    {producto.activo ? (
                      <span className="status-badge status-active">
                        <i
                          className="bi bi-circle-fill"
                          style={{ fontSize: "6px" }}
                        ></i>
                        Activo
                      </span>
                    ) : (
                      <span className="status-badge status-inactive">
                        <i
                          className="bi bi-circle-fill"
                          style={{ fontSize: "6px" }}
                        ></i>
                        Inactivo
                      </span>
                    )}
                  </td>

                  <td>
                    <div className="d-flex gap-2 justify-content-end">
                      <a
                        href={`${detailsUrlBase}${producto.productoId}`}
                        className="btn btn-light btn-sm"
                        title="Ver detalles"
                      >
                        <i className="bi bi-eye"></i>
                      </a>

                      {esAdministrador && (
                        <>
                          <a
                            href={`${editUrlBase}${producto.productoId}`}
                            className="btn btn-warning btn-sm"
                            title="Editar producto"
                          >
                            <i className="bi bi-pencil"></i>
                          </a>

                          <a
                            href={`${deleteUrlBase}${producto.productoId}`}
                            className="btn btn-danger btn-sm"
                            title="Eliminar producto"
                          >
                            <i className="bi bi-trash"></i>
                          </a>
                        </>
                      )}

                      {!esAdministrador &&
                        esGerente &&
                        producto.inventarioSucursalId && (
                          <a
                            href={`${editarInventarioUrlBase}${producto.inventarioSucursalId}`}
                            className="btn btn-warning btn-sm"
                            title="Editar inventario"
                          >
                            <i className="bi bi-box-seam"></i>
                          </a>
                        )}
                    </div>
                  </td>
                </tr>
              ))}
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
        etiqueta="productos"
      />
    </div>
  );
}
