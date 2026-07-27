using FerreteriaRazor.Data;
using FerreteriaRazor.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FerreteriaRazor.Pages.Inventario;

[Authorize(Roles = "Administrador,Gerente")]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public IndexModel(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public List<InventarioSucursal> Inventarios { get; set; } = new();

    public SelectList ListaSucursales { get; set; } = null!;

    [BindProperty(SupportsGet = true)]
    public int? SucursalId { get; set; }

    public bool EsAdministrador { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var usuarioActual = await _userManager.GetUserAsync(User);

        if (usuarioActual is null)
        {
            return Challenge();
        }

        EsAdministrador = await _userManager.IsInRoleAsync(
            usuarioActual,
            "Administrador");

        IQueryable<InventarioSucursal> consulta =
            _context.InventariosSucursales
                .AsNoTracking()
                .Include(i => i.Producto)
                .Include(i => i.Sucursal);

        if (EsAdministrador)
        {
            await CargarSucursalesAsync();

            if (SucursalId.HasValue)
            {
                consulta = consulta.Where(
                    i => i.SucursalId == SucursalId.Value);
            }
        }
        else
        {
            if (!usuarioActual.SucursalId.HasValue)
            {
                return Forbid();
            }

            SucursalId = usuarioActual.SucursalId.Value;

            consulta = consulta.Where(
                i => i.SucursalId == usuarioActual.SucursalId.Value);
        }

        Inventarios = await consulta
            .OrderBy(i => i.Sucursal.Nombre)
            .ThenBy(i => i.Producto.Nombre)
            .ToListAsync();

        return Page();
    }

    private async Task CargarSucursalesAsync()
    {
        var sucursales = await _context.Sucursales
            .AsNoTracking()
            .Where(s => s.Activa)
            .OrderBy(s => s.Nombre)
            .ToListAsync();

        ListaSucursales = new SelectList(
            sucursales,
            nameof(Sucursal.Id),
            nameof(Sucursal.Nombre),
            SucursalId);
    }
    public async Task<IActionResult> OnPostRetirarAsync(int id)
    {
        var usuarioActual = await _userManager.GetUserAsync(User);

        if (usuarioActual is null)
        {
            return Challenge();
        }

        var esAdministrador = await _userManager.IsInRoleAsync(
            usuarioActual,
            "Administrador");

        var consulta = _context.InventariosSucursales
            .Include(i => i.Producto)
            .Include(i => i.Sucursal)
            .Where(i => i.Id == id);

        if (!esAdministrador)
        {
            if (!usuarioActual.SucursalId.HasValue)
            {
                return Forbid();
            }

            consulta = consulta.Where(
                i => i.SucursalId == usuarioActual.SucursalId.Value);
        }

        var inventario = await consulta.FirstOrDefaultAsync();

        if (inventario is null)
        {
            return NotFound();
        }

        /*
        * Más adelante, cuando implementemos ventas,
        * podremos comprobar si existen operaciones pendientes.
        */
        _context.InventariosSucursales.Remove(inventario);

        await _context.SaveChangesAsync();

        TempData["MensajeExito"] =
            $"{inventario.Producto.Nombre} fue retirado del inventario de {inventario.Sucursal.Nombre}.";

        return RedirectToPage();
    }
}