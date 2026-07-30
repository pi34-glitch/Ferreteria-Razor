import React from "react";
import { createRoot } from "react-dom/client";
import App from "./App.jsx";
import "../shared/estilos.css";
import { leerDatosJson } from "../shared/formato.js";

const contenedor = document.getElementById("inventario-tabla-root");

if (contenedor) {
  const inventarios = leerDatosJson("inventarios-data");

  const props = {
    inventarios,
    editarUrlBase: contenedor.dataset.editarUrlBase
  };

  createRoot(contenedor).render(<App {...props} />);
}
