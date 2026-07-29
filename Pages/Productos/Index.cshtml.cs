using FerreteriaRazor.Data;
using FerreteriaRazor.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FerreteriaRazor.Pages.Productos;

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

    public List<ProductoVistaModel> Productos { get; set; } = new();

    public bool EsAdministrador { get; set; }

    public bool EsGerente { get; set; }

    public class ProductoVistaModel
    {
        public int ProductoId { get; set; }

        public int? InventarioSucursalId { get; set; }

        public string Codigo { get; set; } = string.Empty;

        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        public string Categoria { get; set; } = string.Empty;

        public string Marca { get; set; } = string.Empty;

        public string? Distribuidora { get; set; }

        public string? ImagenUrl { get; set; }

        public decimal Precio { get; set; }

        public int? Stock { get; set; }

        public int? StockMinimo { get; set; }

        public bool Activo { get; set; }
    }

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

        EsGerente = await _userManager.IsInRoleAsync(
            usuarioActual,
            "Gerente");

        if (EsAdministrador)
        {
            Productos = await _context.Productos
                .AsNoTracking()
                .OrderBy(p => p.Nombre)
                .Select(p => new ProductoVistaModel
                {
                    ProductoId = p.Id,
                    Codigo = p.Codigo,
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion,

                    Categoria = p.Categoria != null
                        ? p.Categoria.Nombre
                        : "Sin categoría",

                    Marca = p.Marca != null
                        ? p.Marca.Nombre
                        : "Sin marca",

                    Distribuidora = p.Distribuidora != null
                        ? p.Distribuidora.Nombre
                        : null,

                    ImagenUrl = p.ImagenUrl,
                    Precio = p.Precio,
                    Activo = p.Activo,

                    Stock = p.InventariosSucursales
                        .Sum(i => i.Stock),

                    StockMinimo = null
                })
                .ToListAsync();

            return Page();
        }

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

        Productos = await _context.InventariosSucursales
            .AsNoTracking()
            .Where(i =>
                i.SucursalId == sucursalId &&
                i.Producto.Activo)
            .OrderBy(i => i.Producto.Nombre)
            .Select(i => new ProductoVistaModel
            {
                ProductoId = i.ProductoId,
                InventarioSucursalId = i.Id,
                Codigo = i.Producto.Codigo,
                Nombre = i.Producto.Nombre,
                Descripcion = i.Producto.Descripcion,

                Categoria = i.Producto.Categoria != null
                    ? i.Producto.Categoria.Nombre
                    : "Sin categoría",

                Marca = i.Producto.Marca != null
                    ? i.Producto.Marca.Nombre
                    : "Sin marca",

                Distribuidora = i.Producto.Distribuidora != null
                    ? i.Producto.Distribuidora.Nombre
                    : null,

                ImagenUrl = i.Producto.ImagenUrl,
                Precio = i.Producto.Precio,
                Stock = i.Stock,
                StockMinimo = i.StockMinimo,
                Activo = i.Producto.Activo
            })
            .ToListAsync();

        return Page();
    }
}