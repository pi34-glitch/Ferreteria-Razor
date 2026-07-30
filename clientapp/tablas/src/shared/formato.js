export function formatearMoneda(valor) {
  return `Bs ${Number(valor).toFixed(2)}`;
}

export function leerDatosJson(elementId) {
  const nodo = document.getElementById(elementId);

  if (!nodo) {
    return [];
  }

  try {
    return JSON.parse(nodo.textContent);
  } catch {
    return [];
  }
}
