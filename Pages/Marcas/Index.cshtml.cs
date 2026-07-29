using FerreteriaRazor.Data;
using FerreteriaRazor.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FerreteriaRazor.Pages.Marcas;

[Authorize(Roles = "Administrador")]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public List<Marca> Marcas { get; set; } = new();

    public async Task OnGetAsync()
    {
        Marcas = await _context.Marcas
            .AsNoTracking()
            .Include(m => m.Productos)
            .OrderBy(m => m.Nombre)
            .ToListAsync();
    }
}