using FerreteriaRazor.Data;
using FerreteriaRazor.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FerreteriaRazor.Pages.Marcas;

[Authorize(Roles = "Administrador")]
public class DeleteModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public DeleteModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Marca Marca { get; set; } = null!;

    public int CantidadProductos { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var marca = await _context.Marcas
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);

        if (marca is null)
        {
            return NotFound();
        }

        Marca = marca;

        CantidadProductos = await _context.Productos
            .AsNoTracking()
            .CountAsync(p => p.MarcaId == id);

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var marca = await _context.Marcas
            .FirstOrDefaultAsync(m => m.Id == Marca.Id);

        if (marca is null)
        {
            return NotFound();
        }

        bool tieneProductos = await _context.Productos
            .AsNoTracking()
            .AnyAsync(p => p.MarcaId == marca.Id);

        if (tieneProductos)
        {
            TempData["MensajeError"] =
                "No se puede eliminar la marca porque tiene productos asociados. Puede desactivarla desde Editar.";

            return RedirectToPage("./Index");
        }

        _context.Marcas.Remove(marca);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            TempData["MensajeError"] =
                "No se pudo eliminar la marca porque está siendo utilizada.";

            return RedirectToPage("./Index");
        }

        TempData["MensajeExito"] =
            $"La marca {marca.Nombre} fue eliminada correctamente.";

        return RedirectToPage("./Index");
    }
}