import { defineConfig } from "vite";
import { crearConfig } from "./vite.base.config.js";

export default defineConfig(
  crearConfig({
    entry: "src/inventario/main.jsx",
    name: "InventarioTablaApp",
    fileName: "inventario-tabla"
  })
);
