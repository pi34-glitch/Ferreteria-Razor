import { useMemo, useState } from "react";

export function useTablaInteractiva(
  filas,
  { textoBusqueda, ordenadores, tamanoPagina = 10 }
) {
  const [query, setQuery] = useState("");
  const [orden, setOrden] = useState({ columna: null, direccion: "asc" });
  const [pagina, setPagina] = useState(1);

  const filtradas = useMemo(() => {
    const texto = query.trim().toLowerCase();

    if (!texto) {
      return filas;
    }

    return filas.filter((fila) =>
      textoBusqueda(fila).toLowerCase().includes(texto)
    );
  }, [filas, query, textoBusqueda]);

  const ordenadas = useMemo(() => {
    const accessor = orden.columna ? ordenadores[orden.columna] : null;

    if (!accessor) {
      return filtradas;
    }

    const copia = [...filtradas];

    copia.sort((a, b) => {
      const valorA = accessor(a);
      const valorB = accessor(b);

      if (valorA < valorB) {
        return orden.direccion === "asc" ? -1 : 1;
      }

      if (valorA > valorB) {
        return orden.direccion === "asc" ? 1 : -1;
      }

      return 0;
    });

    return copia;
  }, [filtradas, orden, ordenadores]);

  const totalPaginas = Math.max(
    1,
    Math.ceil(ordenadas.length / tamanoPagina)
  );

  const paginaSegura = Math.min(pagina, totalPaginas);

  const filasPagina = useMemo(() => {
    const inicio = (paginaSegura - 1) * tamanoPagina;
    return ordenadas.slice(inicio, inicio + tamanoPagina);
  }, [ordenadas, paginaSegura, tamanoPagina]);

  function cambiarQuery(valor) {
    setQuery(valor);
    setPagina(1);
  }

  function alternarOrden(columna) {
    setPagina(1);

    setOrden((actual) => {
      if (actual.columna !== columna) {
        return { columna, direccion: "asc" };
      }

      if (actual.direccion === "asc") {
        return { columna, direccion: "desc" };
      }

      return { columna: null, direccion: "asc" };
    });
  }

  return {
    query,
    cambiarQuery,
    orden,
    alternarOrden,
    pagina: paginaSegura,
    setPagina,
    totalPaginas,
    totalFiltrado: ordenadas.length,
    totalOriginal: filas.length,
    filasPagina
  };
}
