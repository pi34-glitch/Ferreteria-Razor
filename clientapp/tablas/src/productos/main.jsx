import React from "react";
import { createRoot } from "react-dom/client";
import App from "./App.jsx";
import "../shared/estilos.css";
import { leerDatosJson } from "../shared/formato.js";

const contenedor = document.getElementById("productos-tabla-root");

if (contenedor) {
  const productos = leerDatosJson("productos-data");

  const props = {
    productos,
    esAdministrador: contenedor.dataset.esAdministrador === "true",
    esGerente: contenedor.dataset.esGerente === "true",
    detailsUrlBase: contenedor.dataset.detailsUrlBase,
    editUrlBase: contenedor.dataset.editUrlBase,
    deleteUrlBase: contenedor.dataset.deleteUrlBase,
    editarInventarioUrlBase: contenedor.dataset.editarInventarioUrlBase
  };

  createRoot(contenedor).render(<App {...props} />);
}
