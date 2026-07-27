using FerreteriaRazor.Data;
using FerreteriaRazor.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FerreteriaRazor.Pages.Categorias;
[Authorize(Roles = "Administrador")]
public class DeleteModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public DeleteModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Categoria Categoria { get; set; } = null!;

    public int CantidadProductos { get; private set; }

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var categoria = await _context.Categorias
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);

        if (categoria is null)
        {
            return NotFound();
        }

        Categoria = categoria;

        CantidadProductos = await _context.Productos
            .CountAsync(p => p.CategoriaId == categoria.Id);

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var categoria = await _context.Categorias
            .FirstOrDefaultAsync(c => c.Id == Categoria.Id);

        if (categoria is null)
        {
            return NotFound();
        }

        var tieneProductos = await _context.Productos
            .AnyAsync(p => p.CategoriaId == categoria.Id);

        if (tieneProductos)
        {
            TempData["Error"] =
                "No se puede eliminar la categoría porque tiene productos asociados.";

            return RedirectToPage("./Index");
        }

        _context.Categorias.Remove(categoria);
        await _context.SaveChangesAsync();

        TempData["Mensaje"] =
            "La categoría fue eliminada correctamente.";

        return RedirectToPage("./Index");
    }
}