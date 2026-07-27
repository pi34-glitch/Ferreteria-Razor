using FerreteriaRazor.Data;
using FerreteriaRazor.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FerreteriaRazor.Pages.Productos;

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

        var 
        producto = await _context.Productos
            .AsNoTracking()
            .Include(p => p.Categoria)
            .Include(p => p.Marca)
            .Include(p => p.Distribuidora)
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

        var imagenUrl = producto.ImagenUrl;

        _context.Productos.Remove(producto);
        await _context.SaveChangesAsync();

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