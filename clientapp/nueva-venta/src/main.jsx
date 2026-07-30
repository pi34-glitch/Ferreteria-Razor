import React from "react";
import { createRoot } from "react-dom/client";
import App from "./App.jsx";
import "./styles.css";

const contenedor = document.getElementById("nueva-venta-root");

if (contenedor) {
  const props = {
    antiforgeryToken: contenedor.dataset.antiforgeryToken,
    nombreSucursal: contenedor.dataset.nombreSucursal,
    productosUrl: contenedor.dataset.productosUrl,
    confirmarUrl: contenedor.dataset.confirmarUrl,
    detalleUrlBase: contenedor.dataset.detalleUrlBase,
    cancelarUrl: contenedor.dataset.cancelarUrl
  };

  createRoot(contenedor).render(<App {...props} />);
}
