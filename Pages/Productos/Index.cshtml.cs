using FerreteriaRazor.Data;
using FerreteriaRazor.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FerreteriaRazor.Pages.Productos;

[Authorize(Roles = "Administrador,Gerente")]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public IList<Producto> Productos { get; private set; }
        = new List<Producto>();

    public async Task OnGetAsync()
    {
        Productos = await _context.Productos
            .AsNoTracking()
            .Include(p => p.Categoria)
            .Include(p => p.Marca)
            .Include(p => p.Distribuidora)
            .Include(p => p.InventariosSucursales)
                .ThenInclude(i => i.Sucursal)
            .OrderBy(p => p.Nombre)
            .ToListAsync();
    }
}