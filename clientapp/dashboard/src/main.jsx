import React from "react";
import { createRoot } from "react-dom/client";
import App from "./App.jsx";
import "./estilos.css";
import { leerDatosJson } from "./formato.js";

const contenedor = document.getElementById("dashboard-widgets-root");

if (contenedor) {
  const datosIniciales = leerDatosJson("dashboard-datos");

  const props = {
    datosIniciales,
    datosUrl: contenedor.dataset.datosUrl,
    catalogoUrl: contenedor.dataset.catalogoUrl,
    inventarioUrl: contenedor.dataset.inventarioUrl,
    editarInventarioUrlBase: contenedor.dataset.editarInventarioUrlBase
  };

  createRoot(contenedor).render(<App {...props} />);
}
