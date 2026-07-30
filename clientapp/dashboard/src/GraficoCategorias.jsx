import React, { useEffect, useRef } from "react";
import {
  Chart,
  ArcElement,
  Tooltip,
  Legend
} from "chart.js";

Chart.register(ArcElement, Tooltip, Legend);

const COLORES = [
  "#f97316",
  "#111827",
  "#d97706",
  "#dc2626",
  "#2563eb",
  "#16a34a",
  "#fb923c",
  "#6b7280"
];

export default function GraficoCategorias({ categorias }) {
  const canvasRef = useRef(null);
  const chartRef = useRef(null);

  useEffect(() => {
    if (!canvasRef.current) {
      return;
    }

    chartRef.current = new Chart(canvasRef.current, {
      type: "doughnut",
      data: {
        labels: categorias.map((c) => c.nombre),
        datasets: [
          {
            data: categorias.map((c) => c.cantidadProductos),
            backgroundColor: COLORES,
            borderWidth: 0,
            hoverOffset: 8
          }
        ]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        cutout: "65%",
        animation: { duration: 400 },
        plugins: {
          legend: {
            position: "bottom",
            labels: {
              usePointStyle: true,
              padding: 18
            }
          }
        }
      }
    });

    return () => {
      chartRef.current?.destroy();
      chartRef.current = null;
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  useEffect(() => {
    const grafico = chartRef.current;

    if (!grafico) {
      return;
    }

    grafico.data.labels = categorias.map((c) => c.nombre);
    grafico.data.datasets[0].data = categorias.map(
      (c) => c.cantidadProductos
    );
    grafico.update();
  }, [categorias]);

  return (
    <div style={{ height: "320px" }}>
      <canvas ref={canvasRef}></canvas>
    </div>
  );
}
