using FerreteriaRazor.Data;
using FerreteriaRazor.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FerreteriaRazor.Pages.Ventas;

[Authorize(Roles = "Administrador,Gerente,Empleado")]
public class NuevaModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public NuevaModel(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public string NombreSucursal { get; set; } = string.Empty;

    public class ProductoDisponibleDto
    {
        public int ProductoId { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public int Stock { get; set; }
    }

    public class ItemCarritoDto
    {
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
    }

    public class ConfirmarVentaRequest
    {
        public List<ItemCarritoDto> Items { get; set; } = new();
        public MetodoPago MetodoPago { get; set; }
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var resultado = await ValidarSucursalAsync();

        if (resultado.Error is not null)
        {
            return resultado.Error;
        }

        NombreSucursal = await _context.Sucursales
            .AsNoTracking()
            .Where(s => s.Id == resultado.SucursalId)
            .Select(s => s.Nombre)
            .FirstOrDefaultAsync()
            ?? string.Empty;

        return Page();
    }

    public async Task<IActionResult> OnGetProductosAsync(string? q)
    {
        var resultado = await ValidarSucursalAsync();

        if (resultado.Error is not null)
        {
            return resultado.Error;
        }

        var consulta = _context.InventariosSucursales
            .AsNoTracking()
            .Where(i =>
                i.SucursalId == resultado.SucursalId &&
                i.Stock > 0 &&
                i.Producto.Activo);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var texto = q.Trim();

            consulta = consulta.Where(i =>
                EF.Functions.ILike(i.Producto.Nombre, $"%{texto}%") ||
                EF.Functions.ILike(i.Producto.Codigo, $"%{texto}%"));
        }

        var productos = await consulta
            .OrderBy(i => i.Producto.Nombre)
            .Take(25)
            .Select(i => new ProductoDisponibleDto
            {
                ProductoId = i.ProductoId,
                Codigo = i.Producto.Codigo,
                Nombre = i.Producto.Nombre,
                Precio = i.Producto.Precio,
                Stock = i.Stock
            })
            .ToListAsync();

        return new JsonResult(productos);
    }

    public async Task<IActionResult> OnPostConfirmarAsync(
        [FromBody] ConfirmarVentaRequest request)
    {
        var resultado = await ValidarSucursalAsync();

        if (resultado.Error is not null)
        {
            return resultado.Error;
        }

        int sucursalId = resultado.SucursalId;

        if (request.Items is null || request.Items.Count == 0)
        {
            return BadRequest(new { mensaje = "El carrito está vacío." });
        }

        if (request.MetodoPago is not
            (MetodoPago.Efectivo or MetodoPago.QR or MetodoPago.Tarjeta))
        {
            return BadRequest(new { mensaje = "Debe seleccionar un método de pago." });
        }

        // Suma cantidades si el mismo producto aparece más de una vez.
        var cantidadesPorProducto = request.Items
            .GroupBy(i => i.ProductoId)
            .ToDictionary(g => g.Key, g => g.Sum(i => i.Cantidad));

        if (cantidadesPorProducto.Values.Any(c => c <= 0))
        {
            return BadRequest(new { mensaje = "Las cantidades deben ser mayores que cero." });
        }

        var productoIds = cantidadesPorProducto.Keys.ToList();

        await using var transaccion =
            await _context.Database.BeginTransactionAsync();

        try
        {
            var inventarios = await _context.InventariosSucursales
                .Include(i => i.Producto)
                .Where(i =>
                    i.SucursalId == sucursalId &&
                    productoIds.Contains(i.ProductoId) &&
                    i.Producto.Activo)
                .ToListAsync();

            if (inventarios.Count != productoIds.Count)
            {
                return BadRequest(new
                {
                    mensaje = "Uno o más productos ya no están disponibles en esta sucursal."
                });
            }

            foreach (var inventario in inventarios)
            {
                var cantidad = cantidadesPorProducto[inventario.ProductoId];

                if (inventario.Stock < cantidad)
                {
                    return BadRequest(new
                    {
                        mensaje =
                            $"Stock insuficiente para {inventario.Producto.Nombre}. " +
                            $"Disponible: {inventario.Stock}."
                    });
                }
            }

            var venta = new Venta
            {
                Fecha = DateTime.UtcNow,
                SucursalId = sucursalId,
                UsuarioId = resultado.UsuarioId!,
                MetodoPago = request.MetodoPago,
                Total = 0
            };

            decimal total = 0;

            foreach (var inventario in inventarios)
            {
                var cantidad = cantidadesPorProducto[inventario.ProductoId];
                var precioUnitario = inventario.Producto.Precio;
                var subtotal = precioUnitario * cantidad;

                venta.Detalles.Add(new DetalleVenta
                {
                    ProductoId = inventario.ProductoId,
                    Cantidad = cantidad,
                    PrecioUnitario = precioUnitario,
                    Subtotal = subtotal
                });

                inventario.Stock -= cantidad;
                inventario.FechaActualizacion = DateTime.UtcNow;

                total += subtotal;
            }

            venta.Total = total;

            _context.Ventas.Add(venta);

            await _context.SaveChangesAsync();
            await transaccion.CommitAsync();

            return new JsonResult(new { ventaId = venta.Id });
        }
        catch (DbUpdateException)
        {
            await transaccion.RollbackAsync();

            return BadRequest(new
            {
                mensaje = "No se pudo registrar la venta. Intente nuevamente."
            });
        }
    }

    private async Task<(int SucursalId, string? UsuarioId, IActionResult? Error)>
        ValidarSucursalAsync()
    {
        var usuarioActual = await _userManager.GetUserAsync(User);

        if (usuarioActual is null)
        {
            return (0, null, Challenge());
        }

        if (!usuarioActual.SucursalId.HasValue)
        {
            return (0, null, Forbid());
        }

        bool sucursalActiva = await _context.Sucursales
            .AsNoTracking()
            .AnyAsync(s =>
                s.Id == usuarioActual.SucursalId.Value &&
                s.Activa);

        if (!sucursalActiva)
        {
            return (0, null, Forbid());
        }

        return (usuarioActual.SucursalId.Value, usuarioActual.Id, null);
    }
}
