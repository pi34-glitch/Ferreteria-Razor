using System.ComponentModel.DataAnnotations;
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
public class AgregarModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public AgregarModel(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public SelectList ListaProductos { get; set; } = null!;

    public SelectList ListaSucursales { get; set; } = null!;

    public bool EsAdministrador { get; set; }

    public string? NombreSucursalGerente { get; set; }

    public class InputModel
    {
        [Required(ErrorMessage = "Debe seleccionar un producto.")]
        [Display(Name = "Producto")]
        public int? ProductoId { get; set; }

        [Display(Name = "Sucursal")]
        public int? SucursalId { get; set; }

        [Required(ErrorMessage = "Debe ingresar el stock inicial.")]
        [Range(0, int.MaxValue,
            ErrorMessage = "El stock no puede ser negativo.")]
        [Display(Name = "Stock inicial")]
        public int? Stock { get; set; }

        [Required(ErrorMessage = "Debe ingresar el stock mínimo.")]
        [Range(0, int.MaxValue,
            ErrorMessage = "El stock mínimo no puede ser negativo.")]
        [Display(Name = "Stock mínimo")]
        public int? StockMinimo { get; set; } = 5;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var resultadoAcceso = await PrepararPaginaAsync();

        if (resultadoAcceso is not null)
        {
            return resultadoAcceso;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var usuarioActual = await _userManager.GetUserAsync(User);

        if (usuarioActual is null)
        {
            return Challenge();
        }

        EsAdministrador = await _userManager.IsInRoleAsync(
            usuarioActual,
            "Administrador");

        await CargarProductosAsync();

        int sucursalEfectivaId;

        if (EsAdministrador)
        {
            await CargarSucursalesAsync();

            if (!Input.SucursalId.HasValue)
            {
                ModelState.AddModelError(
                    nameof(Input.SucursalId),
                    "Debe seleccionar una sucursal.");

                return Page();
            }

            sucursalEfectivaId = Input.SucursalId.Value;
        }
        else
        {
            if (!usuarioActual.SucursalId.HasValue)
            {
                return Forbid();
            }

            sucursalEfectivaId = usuarioActual.SucursalId.Value;
            Input.SucursalId = sucursalEfectivaId;

            NombreSucursalGerente = await _context.Sucursales
                .Where(s => s.Id == sucursalEfectivaId)
                .Select(s => s.Nombre)
                .FirstOrDefaultAsync();
        }

       if (!ModelState.IsValid)
        {
            return Page();
        }

        if (!Input.ProductoId.HasValue)
        {
            ModelState.AddModelError(
                nameof(Input.ProductoId),
                "Debe seleccionar un producto.");

            return Page();
        }

        var productoId = Input.ProductoId.Value;

        var productoExiste = await _context.Productos
            .AnyAsync(p => p.Id == productoId);

        if (!productoExiste)
        {
            ModelState.AddModelError(
                nameof(Input.ProductoId),
                "El producto seleccionado no existe.");

            return Page();
        }

        var sucursalExiste = await _context.Sucursales
            .AnyAsync(s =>
                s.Id == sucursalEfectivaId &&
                s.Activa);

        if (!sucursalExiste)
        {
            ModelState.AddModelError(
                nameof(Input.SucursalId),
                "La sucursal seleccionada no existe o está inactiva.");

            return Page();
        }

        var productoYaRegistrado =
            await _context.InventariosSucursales
                .AnyAsync(i =>
                    i.ProductoId == productoId &&
                    i.SucursalId == sucursalEfectivaId);

        if (productoYaRegistrado)
        {
            ModelState.AddModelError(
                nameof(Input.ProductoId),
                "El producto ya se encuentra registrado en el inventario de esta sucursal.");

            return Page();
        }

        var inventario = new InventarioSucursal
        {
            ProductoId = productoId,
            SucursalId = sucursalEfectivaId,
            Stock = Input.Stock!.Value,
            StockMinimo = Input.StockMinimo!.Value,
            FechaActualizacion = DateTime.UtcNow
        };

        _context.InventariosSucursales.Add(inventario);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(
                nameof(Input.ProductoId),
                "No se pudo registrar el producto. Es posible que ya exista en el inventario de esta sucursal.");

            await CargarProductosAsync();

            if (EsAdministrador)
            {
                await CargarSucursalesAsync();
            }

            return Page();
        }

        TempData["MensajeExito"] =
            "El producto fue agregado al inventario correctamente.";

        return RedirectToPage("./Index");
    }

    private async Task<IActionResult?> PrepararPaginaAsync()
    {
        var usuarioActual = await _userManager.GetUserAsync(User);

        if (usuarioActual is null)
        {
            return Challenge();
        }

        EsAdministrador = await _userManager.IsInRoleAsync(
            usuarioActual,
            "Administrador");

        await CargarProductosAsync();

        if (EsAdministrador)
        {
            await CargarSucursalesAsync();
        }
        else
        {
            if (!usuarioActual.SucursalId.HasValue)
            {
                return Forbid();
            }

            int sucursalId = usuarioActual.SucursalId.Value;

            var sucursalGerente = await _context.Sucursales
                .AsNoTracking()
                .Where(s =>
                    s.Id == sucursalId &&
                    s.Activa)
                .Select(s => new
                {
                    s.Id,
                    s.Nombre
                })
                .FirstOrDefaultAsync();

            if (sucursalGerente is null)
            {
                return Forbid();
            }

            Input.SucursalId = sucursalGerente.Id;
            NombreSucursalGerente = sucursalGerente.Nombre;
        }

        return null;
    }

    private async Task CargarProductosAsync()
    {
        var productos = await _context.Productos
            .AsNoTracking()
            .Where(p => p.Activo)
            .OrderBy(p => p.Nombre)
            .Select(p => new
            {
                p.Id,
                Descripcion = p.Codigo + " — " + p.Nombre
            })
            .ToListAsync();

        ListaProductos = new SelectList(
            productos,
            "Id",
            "Descripcion",
            Input.ProductoId);
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
            Input.SucursalId);
    }
}