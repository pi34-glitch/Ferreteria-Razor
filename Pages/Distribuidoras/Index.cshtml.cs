using FerreteriaRazor.Data;
using FerreteriaRazor.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FerreteriaRazor.Pages.Distribuidoras;

[Authorize(Roles = "Administrador")]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public List<Distribuidora> Distribuidoras { get; set; } = new();

    public async Task OnGetAsync()
    {
        Distribuidoras = await _context.Distribuidoras
            .AsNoTracking()
            .Include(d => d.Productos)
            .OrderBy(d => d.Nombre)
            .ToListAsync();
    }
}