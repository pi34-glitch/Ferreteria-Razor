using FerreteriaRazor.Data;
using FerreteriaRazor.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FerreteriaRazor.Pages.Marcas;

[Authorize(Roles = "Administrador")]
public class DetailsModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public DetailsModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public Marca Marca { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var marca = await _context.Marcas
            .AsNoTracking()
            .Include(m => m.Productos)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (marca is null)
        {
            return NotFound();
        }

        Marca = marca;

        return Page();
    }
}