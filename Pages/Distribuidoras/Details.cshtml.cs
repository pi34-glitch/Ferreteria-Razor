using FerreteriaRazor.Data;
using FerreteriaRazor.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FerreteriaRazor.Pages.Distribuidoras;

[Authorize(Roles = "Administrador")]
public class DetailsModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public DetailsModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public Distribuidora Distribuidora { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var distribuidora = await _context.Distribuidoras
            .AsNoTracking()
            .Include(d => d.Productos)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (distribuidora is null)
        {
            return NotFound();
        }

        Distribuidora = distribuidora;

        return Page();
    }
}