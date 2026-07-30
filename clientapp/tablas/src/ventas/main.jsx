import React from "react";
import { createRoot } from "react-dom/client";
import App from "./App.jsx";
import "../shared/estilos.css";
import { leerDatosJson } from "../shared/formato.js";

const contenedor = document.getElementById("ventas-tabla-root");

if (contenedor) {
  const ventas = leerDatosJson("ventas-data");

  const props = {
    ventas,
    detalleUrlBase: contenedor.dataset.detalleUrlBase
  };

  createRoot(contenedor).render(<App {...props} />);
}
