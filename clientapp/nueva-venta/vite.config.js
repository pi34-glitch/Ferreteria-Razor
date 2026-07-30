import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

export default defineConfig({
  plugins: [react()],
  build: {
    outDir: "dist",
    emptyOutDir: true,
    cssCodeSplit: false,
    lib: {
      entry: "src/main.jsx",
      name: "NuevaVentaApp",
      formats: ["iife"],
      fileName: () => "nueva-venta.js"
    },
    rollupOptions: {
      output: {
        assetFileNames: "nueva-venta.[ext]"
      }
    }
  }
});
