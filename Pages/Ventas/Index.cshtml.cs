using FerreteriaRazor.Data;
using FerreteriaRazor.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FerreteriaRazor.Pages.Ventas;

[Authorize(Roles = "Administrador,Gerente,Empleado")]
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

    public List<Venta> Ventas { get; set; } = new();

    public SelectList ListaSucursales { get; set; } = null!;

    public bool EsAdministrador { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? SucursalId { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? FechaDesde { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? FechaHasta { get; set; }

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

        IQueryable<Venta> consulta = _context.Ventas
            .AsNoTracking()
            .Include(v => v.Sucursal)
            .Include(v => v.Usuario)
            .Include(v => v.Detalles)
                .ThenInclude(d => d.Producto);

        if (EsAdministrador)
        {
            await CargarSucursalesAsync();

            if (SucursalId.HasValue)
            {
                bool sucursalValida = await _context.Sucursales
                    .AsNoTracking()
                    .AnyAsync(s =>
                        s.Id == SucursalId.Value &&
                        s.Activa);

                if (sucursalValida)
                {
                    consulta = consulta.Where(
                        v => v.SucursalId == SucursalId.Value);
                }
                else
                {
                    SucursalId = null;
                }
            }
        }
        else
        {
            if (!usuarioActual.SucursalId.HasValue)
            {
                return Forbid();
            }

            int sucursalUsuarioId =
                usuarioActual.SucursalId.Value;

            bool sucursalActiva = await _context.Sucursales
                .AsNoTracking()
                .AnyAsync(s =>
                    s.Id == sucursalUsuarioId &&
                    s.Activa);

            if (!sucursalActiva)
            {
                return Forbid();
            }

            SucursalId = sucursalUsuarioId;

            consulta = consulta.Where(
                v => v.SucursalId == sucursalUsuarioId);
        }

        if (FechaDesde.HasValue)
        {
            DateTime fechaDesdeUtc =
                DateTime.SpecifyKind(
                    FechaDesde.Value.Date,
                    DateTimeKind.Local)
                .ToUniversalTime();

            consulta = consulta.Where(
                v => v.Fecha >= fechaDesdeUtc);
        }

        if (FechaHasta.HasValue)
        {
            DateTime fechaHastaUtc =
                DateTime.SpecifyKind(
                    FechaHasta.Value.Date.AddDays(1),
                    DateTimeKind.Local)
                .ToUniversalTime();

            consulta = consulta.Where(
                v => v.Fecha < fechaHastaUtc);
        }

        Ventas = await consulta
            .OrderByDescending(v => v.Fecha)
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
}