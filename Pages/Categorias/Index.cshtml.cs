using Microsoft.AspNetCore.Authorization;
using FerreteriaRazor.Data;
using FerreteriaRazor.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FerreteriaRazor.Pages.Categorias;

[Authorize(Roles = "Administrador")]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public IList<Categoria> Categorias { get; private set; }
        = new List<Categoria>();

    public async Task OnGetAsync()
    {
        Categorias = await _context.Categorias
            .AsNoTracking()
            .Include(c => c.Productos)
            .OrderBy(c => c.Nombre)
            .ToListAsync();
    }
}