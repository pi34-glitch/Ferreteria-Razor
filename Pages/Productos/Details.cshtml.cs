using FerreteriaRazor.Data;
using FerreteriaRazor.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FerreteriaRazor.Pages.Productos;

public class DetailsModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public DetailsModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public Producto Producto { get; private set; } = null!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (!id.HasValue)
        {
            return NotFound();
        }

        var producto = await _context.Productos
            .AsNoTracking()
            .Include(p => p.Categoria)
            .Include(p => p.Marca)
            .Include(p => p.Distribuidora)
            .Include(p => p.InventariosSucursales)
                .ThenInclude(i => i.Sucursal)
            .FirstOrDefaultAsync(p => p.Id == id.Value);

        if (producto is null)
        {
            return NotFound();
        }

        Producto = producto;

        return Page();
    }
}