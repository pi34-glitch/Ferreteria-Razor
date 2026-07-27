using FerreteriaRazor.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FerreteriaRazor.Pages;

[Authorize(Roles = "Administrador")]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    // Tarjetas principales
    public int TotalProductos { get; private set; }

    public int TotalSucursalesActivas { get; private set; }

    public int InventariosBajoStock { get; private set; }

    public int InventariosSinStock { get; private set; }

    public int StockTotal { get; private set; }

    public decimal ValorInventario { get; private set; }

    // Tablas y gráfico
    public IList<InventarioAlertaViewModel> AlertasStock { get; private set; }
        = new List<InventarioAlertaViewModel>();

    public IList<ProductoRecienteViewModel> ProductosRecientes { get; private set; }
        = new List<ProductoRecienteViewModel>();

    public IList<CategoriaResumenViewModel> ProductosPorCategoria { get; private set; }
        = new List<CategoriaResumenViewModel>();

    public async Task OnGetAsync()
    {
        /*
         * Cantidad de productos existentes en el catálogo.
         */
        TotalProductos = await _context.Productos
            .AsNoTracking()
            .CountAsync();

        /*
         * Cantidad de sucursales habilitadas.
         */
        TotalSucursalesActivas = await _context.Sucursales
            .AsNoTracking()
            .CountAsync(s => s.Activa);

        /*
         * Registros de inventario con stock bajo.
         * No incluye los que ya están completamente sin stock.
         */
        InventariosBajoStock = await _context.InventariosSucursales
            .AsNoTracking()
            .CountAsync(i =>
                i.Producto.Activo &&
                i.Sucursal.Activa &&
                i.Stock > 0 &&
                i.Stock <= i.StockMinimo);

        /*
         * Registros de inventario que no tienen unidades disponibles.
         */
        InventariosSinStock = await _context.InventariosSucursales
            .AsNoTracking()
            .CountAsync(i =>
                i.Producto.Activo &&
                i.Sucursal.Activa &&
                i.Stock == 0);

        /*
         * Suma de las unidades disponibles entre todas las sucursales.
         */
        StockTotal = await _context.InventariosSucursales
            .AsNoTracking()
            .Where(i =>
                i.Producto.Activo &&
                i.Sucursal.Activa)
            .SumAsync(i => (int?)i.Stock) ?? 0;

        /*
         * Valor estimado del inventario:
         * precio del producto multiplicado por su stock.
         */
        ValorInventario = await _context.InventariosSucursales
            .AsNoTracking()
            .Where(i =>
                i.Producto.Activo &&
                i.Sucursal.Activa)
            .SumAsync(i => (decimal?)(i.Producto.Precio * i.Stock)) ?? 0;

        /*
         * Alertas de stock por producto y sucursal.
         */
        AlertasStock = await _context.InventariosSucursales
            .AsNoTracking()
            .Where(i =>
                i.Producto.Activo &&
                i.Sucursal.Activa &&
                i.Stock <= i.StockMinimo)
            .OrderBy(i => i.Stock)
            .ThenBy(i => i.Producto.Nombre)
            .Select(i => new InventarioAlertaViewModel
            {
                InventarioId = i.Id,
                ProductoId = i.ProductoId,
                Producto = i.Producto.Nombre,
                Codigo = i.Producto.Codigo,
                Categoria = i.Producto.Categoria.Nombre,
                Sucursal = i.Sucursal.Nombre,
                Stock = i.Stock,
                StockMinimo = i.StockMinimo
            })
            .Take(8)
            .ToListAsync();

        /*
         * Productos agregados recientemente al catálogo.
         * El stock se obtiene sumando sus inventarios por sucursal.
         */
        ProductosRecientes = await _context.Productos
            .AsNoTracking()
            .OrderByDescending(p => p.FechaRegistro)
            .Select(p => new ProductoRecienteViewModel
            {
                ProductoId = p.Id,
                Codigo = p.Codigo,
                Nombre = p.Nombre,
                Categoria = p.Categoria.Nombre,
                Marca = p.Marca.Nombre,
                Precio = p.Precio,
                ImagenUrl = p.ImagenUrl,
                FechaRegistro = p.FechaRegistro,

                StockTotal = p.InventariosSucursales
                    .Where(i => i.Sucursal.Activa)
                    .Sum(i => (int?)i.Stock) ?? 0,

                TieneStockBajo = p.InventariosSucursales
                    .Any(i =>
                        i.Sucursal.Activa &&
                        i.Stock <= i.StockMinimo)
            })
            .Take(5)
            .ToListAsync();

        /*
         * Resumen de productos activos agrupados por categoría.
         */
        ProductosPorCategoria = await _context.Categorias
            .AsNoTracking()
            .Where(c => c.Activa)
            .Select(c => new CategoriaResumenViewModel
            {
                Nombre = c.Nombre,

                CantidadProductos = c.Productos
                    .Count(p => p.Activo)
            })
            .Where(c => c.CantidadProductos > 0)
            .OrderByDescending(c => c.CantidadProductos)
            .ThenBy(c => c.Nombre)
            .Take(8)
            .ToListAsync();
    }

    public class InventarioAlertaViewModel
    {
        public int InventarioId { get; set; }

        public int ProductoId { get; set; }

        public string Producto { get; set; } = string.Empty;

        public string Codigo { get; set; } = string.Empty;

        public string Categoria { get; set; } = string.Empty;

        public string Sucursal { get; set; } = string.Empty;

        public int Stock { get; set; }

        public int StockMinimo { get; set; }

        public bool SinStock => Stock == 0;
    }

    public class ProductoRecienteViewModel
    {
        public int ProductoId { get; set; }

        public string Codigo { get; set; } = string.Empty;

        public string Nombre { get; set; } = string.Empty;

        public string Categoria { get; set; } = string.Empty;

        public string Marca { get; set; } = string.Empty;

        public decimal Precio { get; set; }

        public string? ImagenUrl { get; set; }

        public DateTime FechaRegistro { get; set; }

        public int StockTotal { get; set; }

        public bool TieneStockBajo { get; set; }
    }

    public class CategoriaResumenViewModel
    {
        public string Nombre { get; set; } = string.Empty;

        public int CantidadProductos { get; set; }
    }
}