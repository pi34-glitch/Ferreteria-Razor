using System.ComponentModel.DataAnnotations;
using FerreteriaRazor.Data;
using FerreteriaRazor.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
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

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public SelectList ListaProductos { get; set; } = null!;

    public string NombreSucursal { get; set; } = string.Empty;

    public class InputModel
    {
        [Required(ErrorMessage = "Debe seleccionar un producto.")]
        [Display(Name = "Producto")]
        public int? ProductoId { get; set; }

        [Required(ErrorMessage = "Debe ingresar una cantidad.")]
        [Range(1, int.MaxValue,
            ErrorMessage = "La cantidad debe ser mayor que cero.")]
        public int? Cantidad { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un método de pago.")]
        [Display(Name = "Método de pago")]
        public MetodoPago? MetodoPago { get; set; }
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var usuarioActual = await _userManager.GetUserAsync(User);

        if (usuarioActual is null)
        {
            return Challenge();
        }

        if (!usuarioActual.SucursalId.HasValue)
        {
            return Forbid();
        }

        bool sucursalActiva = await _context.Sucursales
            .AsNoTracking()
            .AnyAsync(s =>
                s.Id == usuarioActual.SucursalId.Value &&
                s.Activa);

        if (!sucursalActiva)
        {
            return Forbid();
        }

        await PrepararPaginaAsync(usuarioActual.SucursalId.Value);

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var usuarioActual = await _userManager.GetUserAsync(User);

        if (usuarioActual is null)
        {
            return Challenge();
        }

        if (!usuarioActual.SucursalId.HasValue)
        {
            return Forbid();
        }

        int sucursalId = usuarioActual.SucursalId.Value;

        await PrepararPaginaAsync(sucursalId);

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var inventario = await _context.InventariosSucursales
            .Include(i => i.Producto)
            .FirstOrDefaultAsync(i =>
                i.ProductoId == Input.ProductoId!.Value &&
                i.SucursalId == sucursalId &&
                i.Producto.Activo);

        if (inventario is null)
        {
            ModelState.AddModelError(
                "Input.ProductoId",
                "El producto no pertenece al inventario de esta sucursal.");

            return Page();
        }

        int cantidad = Input.Cantidad!.Value;

        if (inventario.Stock < cantidad)
        {
            ModelState.AddModelError(
                "Input.Cantidad",
                $"Stock insuficiente. Disponible: {inventario.Stock}.");

            return Page();
        }

        decimal precioUnitario = inventario.Producto.Precio;
        decimal subtotal = precioUnitario * cantidad;
        Venta? ventaRegistrada = null;
        await using var transaccion =
            await _context.Database.BeginTransactionAsync();

        try
        {
            ventaRegistrada = new Venta
            {
                Fecha = DateTime.UtcNow,
                SucursalId = sucursalId,
                UsuarioId = usuarioActual.Id,
                MetodoPago = Input.MetodoPago!.Value,
                Total = subtotal
            };

            ventaRegistrada.Detalles.Add(new DetalleVenta
            {
                ProductoId = inventario.ProductoId,
                Cantidad = cantidad,
                PrecioUnitario = precioUnitario,
                Subtotal = subtotal
            });

            inventario.Stock -= cantidad;
            inventario.FechaActualizacion = DateTime.UtcNow;

            _context.Ventas.Add(ventaRegistrada);

            await _context.SaveChangesAsync();
            await transaccion.CommitAsync();
        }
        catch (DbUpdateException)
        {
            await transaccion.RollbackAsync();

            ModelState.AddModelError(
                string.Empty,
                "No se pudo registrar la venta. Intente nuevamente.");

            return Page();
        }

        TempData["MensajeExito"] =
            "La venta fue registrada correctamente.";

        return RedirectToPage("./Detalle", new
        {
            id = ventaRegistrada!.Id
        });
    }

    private async Task PrepararPaginaAsync(int sucursalId)
    {
        NombreSucursal = await _context.Sucursales
            .AsNoTracking()
            .Where(s => s.Id == sucursalId)
            .Select(s => s.Nombre)
            .FirstOrDefaultAsync()
            ?? string.Empty;

        var productos = await _context.InventariosSucursales
            .AsNoTracking()
            .Where(i =>
                i.SucursalId == sucursalId &&
                i.Stock > 0 &&
                i.Producto.Activo)
            .OrderBy(i => i.Producto.Nombre)
            .Select(i => new
            {
                i.ProductoId,
                Texto = i.Producto.Codigo
                    + " — "
                    + i.Producto.Nombre
                    + " — Stock: "
                    + i.Stock
            })
            .ToListAsync();

        ListaProductos = new SelectList(
            productos,
            "ProductoId",
            "Texto",
            Input.ProductoId);
    }
}