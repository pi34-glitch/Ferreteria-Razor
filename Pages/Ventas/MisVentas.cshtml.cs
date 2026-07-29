using FerreteriaRazor.Data;
using FerreteriaRazor.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FerreteriaRazor.Pages.Ventas;

[Authorize(Roles = "Administrador,Gerente,Empleado")]
public class MisVentasModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public MisVentasModel(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public List<Venta> Ventas { get; set; } = new();

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

        IQueryable<Venta> consulta = _context.Ventas
            .AsNoTracking()
            .Include(v => v.Sucursal)
            .Include(v => v.Detalles)
                .ThenInclude(d => d.Producto)
            .Where(v => v.UsuarioId == usuarioActual.Id);

        if (usuarioActual.SucursalId.HasValue)
        {
            consulta = consulta.Where(v =>
                v.SucursalId == usuarioActual.SucursalId.Value);
        }

        if (FechaDesde.HasValue)
        {
            DateTime fechaDesdeUtc = DateTime.SpecifyKind(
                    FechaDesde.Value.Date,
                    DateTimeKind.Local)
                .ToUniversalTime();

            consulta = consulta.Where(v =>
                v.Fecha >= fechaDesdeUtc);
        }

        if (FechaHasta.HasValue)
        {
            DateTime fechaHastaUtc = DateTime.SpecifyKind(
                    FechaHasta.Value.Date.AddDays(1),
                    DateTimeKind.Local)
                .ToUniversalTime();

            consulta = consulta.Where(v =>
                v.Fecha < fechaHastaUtc);
        }

        Ventas = await consulta
            .OrderByDescending(v => v.Fecha)
            .ToListAsync();

        return Page();
    }
}