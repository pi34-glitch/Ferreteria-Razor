import React, { useCallback, useEffect, useRef, useState } from "react";
import GraficoCategorias from "./GraficoCategorias.jsx";
import { formatearMoneda, formatearTiempoRelativo } from "./formato.js";

const INTERVALO_MS = 30000;

export default function App({
  datosIniciales,
  datosUrl,
  catalogoUrl,
  inventarioUrl,
  editarInventarioUrlBase
}) {
  const [datos, setDatos] = useState(datosIniciales);
  const [actualizando, setActualizando] = useState(false);
  const [ultimaActualizacion, setUltimaActualizacion] = useState(new Date());
  const [, forzarRender] = useState(0);

  const actualizar = useCallback(async () => {
    setActualizando(true);

    try {
      const respuesta = await fetch(datosUrl, {
        headers: { Accept: "application/json" }
      });

      if (respuesta.ok) {
        const nuevosDatos = await respuesta.json();
        setDatos(nuevosDatos);
        setUltimaActualizacion(new Date());
      }
    } catch {
      // Si falla, se mantienen los últimos datos conocidos.
    } finally {
      setActualizando(false);
    }
  }, [datosUrl]);

  useEffect(() => {
    const intervalo = setInterval(() => {
      if (document.visibilityState === "visible") {
        actualizar();
      }
    }, INTERVALO_MS);

    return () => clearInterval(intervalo);
  }, [actualizar]);

  useEffect(() => {
    const tick = setInterval(() => forzarRender((n) => n + 1), 1000);
    return () => clearInterval(tick);
  }, []);

  if (!datos) {
    return null;
  }

  const totalAlertas = datos.inventariosBajoStock + datos.inventariosSinStock;

  return (
    <>
      <div className="db-toolbar">
        <span className="text-muted small">
          <i className="bi bi-arrow-repeat me-1"></i>
          Se actualiza solo cada 30s · {formatearTiempoRelativo(ultimaActualizacion)}
        </span>

        <button
          type="button"
          className="btn btn-sm btn-light"
          onClick={actualizar}
          disabled={actualizando}
        >
          <i
            className={`bi bi-arrow-clockwise ${
              actualizando ? "db-girando" : ""
            }`}
          ></i>{" "}
          Actualizar ahora
        </button>
      </div>

      <div className="row g-3 mb-4">
        <div className="col-sm-6 col-xl-3">
          <div className="card border-0 shadow-sm h-100">
            <div className="card-body">
              <div className="d-flex justify-content-between align-items-start">
                <div>
                  <span className="text-muted">Productos registrados</span>
                  <h2 className="display-6 fw-bold mt-2 mb-0">
                    {datos.totalProductos}
                  </h2>
                </div>
                <div className="fs-2 text-primary">
                  <i className="bi bi-box-seam"></i>
                </div>
              </div>

              <a
                href={catalogoUrl}
                className="text-decoration-none d-inline-block mt-3"
              >
                Ver catálogo <i className="bi bi-arrow-right"></i>
              </a>
            </div>
          </div>
        </div>

        <div className="col-sm-6 col-xl-3">
          <div className="card border-0 shadow-sm h-100">
            <div className="card-body">
              <div className="d-flex justify-content-between align-items-start">
                <div>
                  <span className="text-muted">Stock total</span>
                  <h2 className="display-6 fw-bold mt-2 mb-0">
                    {datos.stockTotal}
                  </h2>
                </div>
                <div className="fs-2 text-success">
                  <i className="bi bi-boxes"></i>
                </div>
              </div>

              <span className="small text-muted d-inline-block mt-3">
                Unidades entre todas las sucursales
              </span>
            </div>
          </div>
        </div>

        <div className="col-sm-6 col-xl-3">
          <div className="card border-0 shadow-sm h-100">
            <div className="card-body">
              <div className="d-flex justify-content-between align-items-start">
                <div>
                  <span className="text-muted">Alertas de stock</span>
                  <h2 className="display-6 fw-bold mt-2 mb-0">
                    {totalAlertas}
                  </h2>
                </div>
                <div className="fs-2 text-warning">
                  <i className="bi bi-exclamation-triangle"></i>
                </div>
              </div>

              <span className="small text-muted d-inline-block mt-3">
                {datos.inventariosBajoStock} con stock bajo ·{" "}
                {datos.inventariosSinStock} sin stock
              </span>
            </div>
          </div>
        </div>

        <div className="col-sm-6 col-xl-3">
          <div className="card border-0 shadow-sm h-100">
            <div className="card-body">
              <div className="d-flex justify-content-between align-items-start">
                <div>
                  <span className="text-muted">Valor del inventario</span>
                  <h2 className="h3 fw-bold mt-2 mb-0">
                    {formatearMoneda(datos.valorInventario)}
                  </h2>
                </div>
                <div className="fs-2 text-info">
                  <i className="bi bi-cash-stack"></i>
                </div>
              </div>

              <span className="small text-muted d-inline-block mt-3">
                {datos.totalSucursalesActivas} sucursales activas
              </span>
            </div>
          </div>
        </div>
      </div>

      <div className="row g-4 mb-4">
        <div className="col-xl-7">
          <div className="card border-0 shadow-sm h-100">
            <div className="card-header bg-white border-0 pt-4 px-4">
              <div className="d-flex justify-content-between align-items-center">
                <div>
                  <h2 className="h5 fw-bold mb-1">Alertas de inventario</h2>
                  <p className="text-muted small mb-0">
                    Productos que requieren reposición por sucursal
                  </p>
                </div>

                <span className="badge bg-warning text-dark">
                  {totalAlertas}
                </span>
              </div>
            </div>

            <div className="card-body px-4">
              {datos.alertasStock.length === 0 ? (
                <div className="text-center py-5">
                  <i className="bi bi-check-circle-fill text-success display-5"></i>
                  <h3 className="h6 fw-bold mt-3">Inventario saludable</h3>
                  <p className="text-muted mb-0">
                    No existen registros con stock bajo.
                  </p>
                </div>
              ) : (
                <>
                  <div className="table-responsive">
                    <table className="table align-middle">
                      <thead>
                        <tr>
                          <th>Producto</th>
                          <th>Sucursal</th>
                          <th>Stock</th>
                          <th>Mínimo</th>
                          <th>Estado</th>
                          <th></th>
                        </tr>
                      </thead>

                      <tbody>
                        {datos.alertasStock.map((alerta) => (
                          <tr key={alerta.inventarioId}>
                            <td>
                              <strong className="d-block">
                                {alerta.producto}
                              </strong>
                              <small className="text-muted">
                                {alerta.codigo} · {alerta.categoria}
                              </small>
                            </td>

                            <td>{alerta.sucursal}</td>
                            <td className="fw-bold">{alerta.stock}</td>
                            <td>{alerta.stockMinimo}</td>

                            <td>
                              {alerta.sinStock ? (
                                <span className="badge bg-danger">
                                  Sin stock
                                </span>
                              ) : (
                                <span className="badge bg-warning text-dark">
                                  Stock bajo
                                </span>
                              )}
                            </td>

                            <td className="text-end">
                              <a
                                href={`${editarInventarioUrlBase}${alerta.inventarioId}`}
                                className="btn btn-sm btn-outline-primary"
                                title="Editar inventario"
                              >
                                <i className="bi bi-pencil"></i>
                              </a>
                            </td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>

                  <div className="text-end">
                    <a href={inventarioUrl} className="text-decoration-none">
                      Ver inventario completo{" "}
                      <i className="bi bi-arrow-right"></i>
                    </a>
                  </div>
                </>
              )}
            </div>
          </div>
        </div>

        <div className="col-xl-5">
          <div className="card border-0 shadow-sm h-100">
            <div className="card-header bg-white border-0 pt-4 px-4">
              <h2 className="h5 fw-bold mb-1">Productos por categoría</h2>
              <p className="text-muted small mb-0">
                Distribución actual del catálogo
              </p>
            </div>

            <div className="card-body">
              {datos.productosPorCategoria.length > 0 ? (
                <GraficoCategorias categorias={datos.productosPorCategoria} />
              ) : (
                <div className="text-center py-5">
                  <i className="bi bi-pie-chart text-muted display-5"></i>
                  <p className="text-muted mt-3 mb-0">
                    No existen datos para mostrar.
                  </p>
                </div>
              )}
            </div>
          </div>
        </div>
      </div>
    </>
  );
}
