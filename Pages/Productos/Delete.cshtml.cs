using FerreteriaRazor.Data;
using FerreteriaRazor.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace FerreteriaRazor.Pages.Productos;

[Authorize(Roles = "Administrador")]
public class DeleteModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public DeleteModel(
        ApplicationDbContext context,
        IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    [BindProperty]
    public Producto Producto { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var producto = await _context.Productos
            .AsNoTracking()
            .Include(p => p.Categoria)
            .Include(p => p.Marca)
            .Include(p => p.Distribuidora)
            .Include(p => p.InventariosSucursales)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (producto is null)
        {
            return NotFound();
        }

        Producto = producto;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var producto = await _context.Productos
            .FirstOrDefaultAsync(p => p.Id == Producto.Id);

        if (producto is null)
        {
            return NotFound();
        }

        /*
         * DetalleVenta -> Producto usa DeleteBehavior.Restrict para
         * no perder el historial de ventas. Si el producto ya fue
         * vendido alguna vez, no se puede eliminar.
         */
        var tieneVentas = await _context.DetallesVenta
            .AsNoTracking()
            .AnyAsync(d => d.ProductoId == producto.Id);

        if (tieneVentas)
        {
            TempData["MensajeError"] =
                "No se puede eliminar el producto porque tiene ventas registradas. " +
                "Puedes desactivarlo desde la edición del producto.";

            return RedirectToPage("./Index");
        }

        var imagenUrl = producto.ImagenUrl;

        await using var transaccion =
            await _context.Database.BeginTransactionAsync();

        /*
         * InventarioSucursal -> Producto también usa Restrict, así que
         * hay que retirar el producto de todas las sucursales antes de
         * poder eliminarlo.
         */
        var inventarios = await _context.InventariosSucursales
            .Where(i => i.ProductoId == producto.Id)
            .ToListAsync();

        _context.InventariosSucursales.RemoveRange(inventarios);
        _context.Productos.Remove(producto);

        await _context.SaveChangesAsync();
        await transaccion.CommitAsync();

        EliminarImagen(imagenUrl);

        TempData["Mensaje"] = "El producto fue eliminado correctamente.";

        return RedirectToPage("./Index");
    }

    private void EliminarImagen(string? imagenUrl)
    {
        if (string.IsNullOrWhiteSpace(imagenUrl))
        {
            return;
        }

        var rutaRelativa = imagenUrl
            .TrimStart('/')
            .Replace('/', Path.DirectorySeparatorChar);

        var rutaCompleta = Path.Combine(
            _environment.WebRootPath,
            rutaRelativa);

        if (System.IO.File.Exists(rutaCompleta))
        {
            System.IO.File.Delete(rutaCompleta);
        }
    }
}