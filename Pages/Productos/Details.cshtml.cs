using FerreteriaRazor.Data;
using FerreteriaRazor.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FerreteriaRazor.Pages.Productos;

[Authorize(Roles = "Administrador,Gerente,Empleado")]
public class DetailsModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public DetailsModel(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }
    public List<InventarioSucursal> InventariosVisibles { get; set; } = new();

    public bool EsAdministrador { get; set; }

    public bool EsGerente { get; set; }
    public Producto Producto { get; private set; } = null!;

    public InventarioSucursal? InventarioSucursal { get; set; }

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (!id.HasValue)
        {
            return NotFound();
        }

        var usuarioActual = await _userManager.GetUserAsync(User);

        if (usuarioActual is null)
        {
            return Challenge();
        }

        EsAdministrador = await _userManager.IsInRoleAsync(
            usuarioActual,
            "Administrador");

        EsGerente = await _userManager.IsInRoleAsync(
            usuarioActual,
            "Gerente");

        if (EsAdministrador)
        {
            InventariosVisibles = Producto.InventariosSucursales
                .OrderBy(i => i.Sucursal.Nombre)
                .ToList();
        }
        else
        {
            if (!usuarioActual.SucursalId.HasValue)
            {
                return Forbid();
            }

            InventariosVisibles = Producto.InventariosSucursales
                .Where(i =>
                    i.SucursalId == usuarioActual.SucursalId.Value &&
                    i.Sucursal.Activa)
                .OrderBy(i => i.Sucursal.Nombre)
                .ToList();

            if (!InventariosVisibles.Any())
            {
                return NotFound();
            }
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