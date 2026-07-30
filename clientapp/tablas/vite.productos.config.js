import { defineConfig } from "vite";
import { crearConfig } from "./vite.base.config.js";

export default defineConfig(
  crearConfig({
    entry: "src/productos/main.jsx",
    name: "ProductosTablaApp",
    fileName: "productos-tabla"
  })
);
