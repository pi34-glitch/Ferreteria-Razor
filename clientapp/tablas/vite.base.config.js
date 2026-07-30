import react from "@vitejs/plugin-react";

export function crearConfig({ entry, name, fileName }) {
  return {
    plugins: [react()],
    define: {
      "process.env.NODE_ENV": JSON.stringify("production")
    },
    build: {
      outDir: `dist/${fileName}`,
      emptyOutDir: true,
      cssCodeSplit: false,
      lib: {
        entry,
        name,
        formats: ["iife"],
        fileName: () => `${fileName}.js`
      },
      rollupOptions: {
        output: {
          assetFileNames: `${fileName}.[ext]`
        }
      }
    }
  };
}
