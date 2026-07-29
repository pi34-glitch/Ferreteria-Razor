using FerreteriaRazor.Data;
using FerreteriaRazor.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FerreteriaRazor.Pages.Ventas;

[Authorize(Roles = "Administrador,Gerente,Empleado")]
public class DetalleModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public DetalleModel(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public Venta Venta { get; set; } = null!;

    public bool EsAdministrador { get; set; }

    public bool EsGerente { get; set; }

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

        IQueryable<Venta> consulta = _context.Ventas
            .AsNoTracking()
            .Include(v => v.Sucursal)
            .Include(v => v.Usuario)
            .Include(v => v.Detalles)
                .ThenInclude(d => d.Producto)
            .Where(v => v.Id == id.Value);

        if (!EsAdministrador)
        {
            if (!usuarioActual.SucursalId.HasValue)
            {
                return Forbid();
            }

            int sucursalId = usuarioActual.SucursalId.Value;

            bool sucursalActiva = await _context.Sucursales
                .AsNoTracking()
                .AnyAsync(s =>
                    s.Id == sucursalId &&
                    s.Activa);

            if (!sucursalActiva)
            {
                return Forbid();
            }

            if (EsGerente)
            {
                consulta = consulta.Where(v =>
                    v.SucursalId == sucursalId);
            }
            else
            {
                consulta = consulta.Where(v =>
                    v.SucursalId == sucursalId &&
                    v.UsuarioId == usuarioActual.Id);
            }
        }


        var venta = await consulta.FirstOrDefaultAsync();

        if (venta is null)
        {
            return NotFound();
        }

        Venta = venta;

        return Page();
    }
}