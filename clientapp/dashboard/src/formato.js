export function formatearMoneda(valor) {
  return `Bs ${Number(valor).toFixed(2)}`;
}

export function formatearTiempoRelativo(fecha) {
  if (!fecha) {
    return "";
  }

  const segundos = Math.max(
    0,
    Math.round((Date.now() - fecha.getTime()) / 1000)
  );

  if (segundos < 5) {
    return "recién actualizado";
  }

  if (segundos < 60) {
    return `hace ${segundos}s`;
  }

  const minutos = Math.round(segundos / 60);

  return `hace ${minutos} min`;
}

export function leerDatosJson(elementId) {
  const nodo = document.getElementById(elementId);

  if (!nodo) {
    return null;
  }

  try {
    return JSON.parse(nodo.textContent);
  } catch {
    return null;
  }
}
