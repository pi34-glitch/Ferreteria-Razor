import React, { useEffect, useMemo, useRef, useState } from "react";
import { formatearMoneda } from "./formato.js";

const METODOS_PAGO = [
  { valor: 1, etiqueta: "Efectivo", icono: "bi-cash-stack", clase: "outline-success" },
  { valor: 2, etiqueta: "QR", icono: "bi-qr-code", clase: "outline-primary" },
  { valor: 3, etiqueta: "Tarjeta", icono: "bi-credit-card", clase: "outline-dark" }
];

export default function App({
  antiforgeryToken,
  nombreSucursal,
  productosUrl,
  confirmarUrl,
  detalleUrlBase,
  cancelarUrl
}) {
  const [query, setQuery] = useState("");
  const [resultados, setResultados] = useState([]);
  const [mostrarResultados, setMostrarResultados] = useState(false);
  const [buscando, setBuscando] = useState(false);
  const [carrito, setCarrito] = useState([]);
  const [metodoPago, setMetodoPago] = useState(null);
  const [enviando, setEnviando] = useState(false);
  const [error, setError] = useState(null);
  const cajaBusquedaRef = useRef(null);

  useEffect(() => {
    const timeout = setTimeout(() => {
      buscarProductos(query);
    }, 250);

    return () => clearTimeout(timeout);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [query]);

  useEffect(() => {
    function alClickearFuera(evento) {
      if (
        cajaBusquedaRef.current &&
        !cajaBusquedaRef.current.contains(evento.target)
      ) {
        setMostrarResultados(false);
      }
    }

    document.addEventListener("mousedown", alClickearFuera);
    return () => document.removeEventListener("mousedown", alClickearFuera);
  }, []);

  async function buscarProductos(texto) {
    setBuscando(true);

    try {
      const url = `${productosUrl}${texto ? `&q=${encodeURIComponent(texto)}` : ""}`;
      const respuesta = await fetch(url, {
        headers: { Accept: "application/json" }
      });

      if (!respuesta.ok) {
        setResultados([]);
        return;
      }

      const datos = await respuesta.json();
      setResultados(datos);
    } catch {
      setResultados([]);
    } finally {
      setBuscando(false);
    }
  }

  function agregarProducto(producto) {
    setCarrito((actual) => {
      const existente = actual.find(
        (item) => item.productoId === producto.productoId
      );

      if (existente) {
        const nuevaCantidad = Math.min(
          existente.cantidad + 1,
          producto.stock
        );

        return actual.map((item) =>
          item.productoId === producto.productoId
            ? { ...item, cantidad: nuevaCantidad }
            : item
        );
      }

      return [
        ...actual,
        {
          productoId: producto.productoId,
          codigo: producto.codigo,
          nombre: producto.nombre,
          precio: producto.precio,
          stock: producto.stock,
          cantidad: 1
        }
      ];
    });

    setQuery("");
    setMostrarResultados(false);
    setError(null);
  }

  function actualizarCantidad(productoId, cantidad) {
    setCarrito((actual) =>
      actual.map((item) => {
        if (item.productoId !== productoId) {
          return item;
        }

        const cantidadValida = Math.max(
          1,
          Math.min(Number(cantidad) || 1, item.stock)
        );

        return { ...item, cantidad: cantidadValida };
      })
    );
  }

  function quitarProducto(productoId) {
    setCarrito((actual) =>
      actual.filter((item) => item.productoId !== productoId)
    );
  }

  const total = useMemo(
    () =>
      carrito.reduce(
        (acumulado, item) => acumulado + item.precio * item.cantidad,
        0
      ),
    [carrito]
  );

  async function confirmarVenta() {
    setError(null);

    if (carrito.length === 0) {
      setError("Agrega al menos un producto al carrito.");
      return;
    }

    if (!metodoPago) {
      setError("Selecciona un método de pago.");
      return;
    }

    setEnviando(true);

    try {
      const respuesta = await fetch(confirmarUrl, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          "X-CSRF-TOKEN": antiforgeryToken
        },
        body: JSON.stringify({
          items: carrito.map((item) => ({
            productoId: item.productoId,
            cantidad: item.cantidad
          })),
          metodoPago
        })
      });

      const datos = await respuesta.json();

      if (!respuesta.ok) {
        setError(datos?.mensaje ?? "No se pudo registrar la venta.");
        return;
      }

      window.location.href = `${detalleUrlBase}${datos.ventaId}`;
    } catch {
      setError("No se pudo conectar con el servidor. Intenta nuevamente.");
    } finally {
      setEnviando(false);
    }
  }

  return (
    <div className="nv-card">
      <div className="nv-card-body">
        <div className="nv-sucursal">
          <i className="bi bi-shop"></i>
          <span>
            <strong>Sucursal:</strong> {nombreSucursal}
          </span>
        </div>

        {error && (
          <div className="alert alert-danger nv-alert" role="alert">
            <i className="bi bi-exclamation-triangle me-2"></i>
            {error}
          </div>
        )}

        <div className="nv-buscador" ref={cajaBusquedaRef}>
          <label className="form-label">Buscar producto</label>

          <div className="nv-buscador-input">
            <i className="bi bi-search"></i>

            <input
              type="text"
              className="form-control"
              placeholder="Escribe el código o nombre del producto..."
              value={query}
              onChange={(evento) => {
                setQuery(evento.target.value);
                setMostrarResultados(true);
              }}
              onFocus={() => setMostrarResultados(true)}
            />

            {buscando && <span className="nv-spinner" aria-hidden="true"></span>}
          </div>

          {mostrarResultados && (
            <div className="nv-resultados">
              {resultados.length === 0 && !buscando && (
                <div className="nv-resultado-vacio">
                  No se encontraron productos con stock disponible.
                </div>
              )}

              {resultados.map((producto) => (
                <button
                  type="button"
                  key={producto.productoId}
                  className="nv-resultado-item"
                  onClick={() => agregarProducto(producto)}
                >
                  <div className="nv-resultado-info">
                    <strong>{producto.nombre}</strong>
                    <span className="text-muted">{producto.codigo}</span>
                  </div>

                  <div className="nv-resultado-meta">
                    <span className="nv-resultado-precio">
                      {formatearMoneda(producto.precio)}
                    </span>
                    <span className="nv-resultado-stock">
                      Stock: {producto.stock}
                    </span>
                  </div>
                </button>
              ))}
            </div>
          )}
        </div>

        <div className="nv-carrito">
          {carrito.length === 0 ? (
            <div className="nv-carrito-vacio">
              <i className="bi bi-cart"></i>
              <p>Todavía no agregaste productos a la venta.</p>
            </div>
          ) : (
            <div className="table-responsive">
              <table className="table align-middle nv-tabla">
                <thead>
                  <tr>
                    <th>Producto</th>
                    <th className="text-center">Cantidad</th>
                    <th className="text-end">Precio</th>
                    <th className="text-end">Subtotal</th>
                    <th></th>
                  </tr>
                </thead>

                <tbody>
                  {carrito.map((item) => (
                    <tr key={item.productoId}>
                      <td>
                        <strong>{item.nombre}</strong>
                        <div className="small text-muted">{item.codigo}</div>
                      </td>

                      <td className="text-center">
                        <input
                          type="number"
                          className="form-control form-control-sm nv-input-cantidad"
                          min={1}
                          max={item.stock}
                          value={item.cantidad}
                          onChange={(evento) =>
                            actualizarCantidad(
                              item.productoId,
                              evento.target.value
                            )
                          }
                        />
                        <div className="small text-muted">
                          Máx: {item.stock}
                        </div>
                      </td>

                      <td className="text-end">
                        {formatearMoneda(item.precio)}
                      </td>

                      <td className="text-end">
                        <strong>
                          {formatearMoneda(item.precio * item.cantidad)}
                        </strong>
                      </td>

                      <td className="text-end">
                        <button
                          type="button"
                          className="btn btn-sm btn-outline-danger"
                          onClick={() => quitarProducto(item.productoId)}
                          title="Quitar producto"
                        >
                          <i className="bi bi-trash"></i>
                        </button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>

        <div className="nv-metodo-pago">
          <label className="form-label d-block">Método de pago</label>

          <div className="row g-3">
            {METODOS_PAGO.map((metodo) => (
              <div className="col-md-4" key={metodo.valor}>
                <button
                  type="button"
                  className={`btn btn-${metodo.clase} w-100 py-3 nv-metodo-btn ${
                    metodoPago === metodo.valor ? "active" : ""
                  }`}
                  onClick={() => setMetodoPago(metodo.valor)}
                >
                  <i className={`bi ${metodo.icono} d-block fs-4 mb-1`}></i>
                  {metodo.etiqueta}
                </button>
              </div>
            ))}
          </div>
        </div>

        <div className="nv-resumen">
          <div className="nv-resumen-total">
            <span>Total</span>
            <strong>{formatearMoneda(total)}</strong>
          </div>

          <div className="nv-resumen-acciones">
            <a href={cancelarUrl} className="btn btn-light">
              Cancelar
            </a>

            <button
              type="button"
              className="btn btn-primary"
              disabled={enviando}
              onClick={confirmarVenta}
            >
              {enviando ? (
                <>
                  <span className="spinner-border spinner-border-sm me-2"></span>
                  Registrando...
                </>
              ) : (
                <>
                  <i className="bi bi-cart-check me-1"></i>
                  Registrar venta
                </>
              )}
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}
