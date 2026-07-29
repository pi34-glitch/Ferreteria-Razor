using FerreteriaRazor.Data;
using FerreteriaRazor.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FerreteriaRazor.Pages.Distribuidoras;

[Authorize(Roles = "Administrador")]
public class DeleteModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public DeleteModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Distribuidora Distribuidora { get; set; } = null!;

    public int CantidadProductos { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var distribuidora = await _context.Distribuidoras
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id);

        if (distribuidora is null)
        {
            return NotFound();
        }

        Distribuidora = distribuidora;

        CantidadProductos = await _context.Productos
            .AsNoTracking()
            .CountAsync(p => p.DistribuidoraId == id);

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var distribuidora = await _context.Distribuidoras
            .FirstOrDefaultAsync(d => d.Id == Distribuidora.Id);

        if (distribuidora is null)
        {
            return NotFound();
        }

        bool tieneProductos = await _context.Productos
            .AsNoTracking()
            .AnyAsync(p =>
                p.DistribuidoraId == distribuidora.Id);

        if (tieneProductos)
        {
            TempData["MensajeError"] =
                "No se puede eliminar la distribuidora porque tiene productos asociados. Puede desactivarla desde Editar.";

            return RedirectToPage("./Index");
        }

        _context.Distribuidoras.Remove(distribuidora);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            TempData["MensajeError"] =
                "No se pudo eliminar la distribuidora porque está siendo utilizada.";

            return RedirectToPage("./Index");
        }

        TempData["MensajeExito"] =
            $"La distribuidora {distribuidora.Nombre} fue eliminada.";

        return RedirectToPage("./Index");
    }
}