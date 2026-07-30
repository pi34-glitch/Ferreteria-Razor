import { defineConfig } from "vite";
import { crearConfig } from "./vite.base.config.js";

export default defineConfig(
  crearConfig({
    entry: "src/ventas/main.jsx",
    name: "VentasTablaApp",
    fileName: "ventas-tabla"
  })
);
