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
public class NuevoProductoModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public NuevoProductoModel(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public SelectList ListaCategorias { get; set; } = null!;
    public SelectList ListaMarcas { get; set; } = null!;
    public SelectList ListaDistribuidoras { get; set; } = null!;
    public SelectList ListaSucursales { get; set; } = null!;

    public bool EsAdministrador { get; set; }

    public string NombreSucursalGerente { get; set; } = string.Empty;

    public class InputModel
    {
        [Required(ErrorMessage = "El nombre del producto es obligatorio.")]
        [StringLength(120)]
        [Display(Name = "Nombre del producto")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Descripción")]
        public string? Descripcion { get; set; }

        [Required(ErrorMessage = "El precio es obligatorio.")]
        [Range(0.01, 99999999,
            ErrorMessage = "El precio debe ser mayor que cero.")]
        [Display(Name = "Precio")]
        public decimal Precio { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una categoría.")]
        [Display(Name = "Categoría")]
        public int? CategoriaId { get; set; }

        [Required(ErrorMessage = "El código es obligatorio.")]
        [StringLength(50)]
        [Display(Name = "Código")]
        public string Codigo { get; set; } = string.Empty;

        [Display(Name = "Marca")]
        public int? MarcaId { get; set; }

        [Display(Name = "Distribuidora")]
        public int? DistribuidoraId { get; set; }

        [Display(Name = "Sucursal")]
        public int? SucursalId { get; set; }

        [Required(ErrorMessage = "Debe ingresar el stock inicial.")]
        [Range(0, int.MaxValue,
            ErrorMessage = "El stock no puede ser negativo.")]
        [Display(Name = "Stock inicial")]
        public int Stock { get; set; }

        [Required(ErrorMessage = "Debe ingresar el stock mínimo.")]
        [Range(0, int.MaxValue,
            ErrorMessage = "El stock mínimo no puede ser negativo.")]
        [Display(Name = "Stock mínimo")]
        public int StockMinimo { get; set; } = 5;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var resultado = await PrepararPaginaAsync();

        if (resultado is not null)
        {
            return resultado;
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

        await CargarListasAsync();

        int sucursalEfectivaId;

        /*
        * El administrador elige la sucursal.
        * El gerente siempre utiliza la sucursal asignada a su cuenta.
        */
        if (EsAdministrador)
        {
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

            // Impide que el gerente cambie la sucursal manipulando el formulario.
            Input.SucursalId = sucursalEfectivaId;

            NombreSucursalGerente = await _context.Sucursales
                .Where(s => s.Id == sucursalEfectivaId)
                .Select(s => s.Nombre)
                .FirstOrDefaultAsync() ?? string.Empty;
        }

        /*
        * Aunque Required ya valida estos campos, comprobamos HasValue
        * para que el compilador sepa que no son null.
        */
        if (!Input.CategoriaId.HasValue)
        {
            ModelState.AddModelError(
                nameof(Input.CategoriaId),
                "Debe seleccionar una categoría.");
        }

        if (!Input.MarcaId.HasValue)
        {
            ModelState.AddModelError(
                nameof(Input.MarcaId),
                "Debe seleccionar una marca.");
        }

        if (!Input.DistribuidoraId.HasValue)
        {
            ModelState.AddModelError(
                nameof(Input.DistribuidoraId),
                "Debe seleccionar una distribuidora.");
        }

        if (!ModelState.IsValid ||
            !Input.CategoriaId.HasValue ||
            !Input.MarcaId.HasValue ||
            !Input.DistribuidoraId.HasValue)
        {
            return Page();
        }

        int categoriaId = Input.CategoriaId.Value;
        int marcaId = Input.MarcaId.Value;
        int distribuidoraId = Input.DistribuidoraId.Value;

        var sucursalValida = await _context.Sucursales
            .AnyAsync(s =>
                s.Id == sucursalEfectivaId &&
                s.Activa);

        if (!sucursalValida)
        {
            ModelState.AddModelError(
                nameof(Input.SucursalId),
                "La sucursal no existe o se encuentra inactiva.");

            return Page();
        }

        var categoriaValida = await _context.Categorias
            .AnyAsync(c => c.Id == categoriaId);

        if (!categoriaValida)
        {
            ModelState.AddModelError(
                nameof(Input.CategoriaId),
                "La categoría seleccionada no existe.");

            return Page();
        }

        var marcaValida = await _context.Marcas
            .AnyAsync(m => m.Id == marcaId);

        if (!marcaValida)
        {
            ModelState.AddModelError(
                nameof(Input.MarcaId),
                "La marca seleccionada no existe.");

            return Page();
        }

        var distribuidoraValida = await _context.Distribuidoras
            .AnyAsync(d => d.Id == distribuidoraId);

        if (!distribuidoraValida)
        {
            ModelState.AddModelError(
                nameof(Input.DistribuidoraId),
                "La distribuidora seleccionada no existe.");

            return Page();
        }

        var codigoLimpio = Input.Codigo.Trim();
        var codigoExistente = await _context.Productos
            .AsNoTracking()
            .AnyAsync(p => p.Codigo.ToUpper() == codigoLimpio.ToUpper());

        if (codigoExistente)
        {
            ModelState.AddModelError(
                nameof(Input.Codigo),
                "Ya existe un producto con ese código.");

            return Page();
        }
        var nombreLimpio = Input.Nombre.Trim();

        var productoExistente = await _context.Productos
            .AsNoTracking()
            .AnyAsync(p =>
                p.Nombre.ToUpper() == nombreLimpio.ToUpper());

        if (productoExistente)
        {
            ModelState.AddModelError(
                nameof(Input.Nombre),
                "Ya existe un producto con ese nombre. Utiliza la opción \"Agregar producto existente\".");

            return Page();
        }

        await using var transaccion =
            await _context.Database.BeginTransactionAsync();

        try
        {
            var producto = new Producto
            {
                Codigo = Input.Codigo.Trim(),
                Nombre = nombreLimpio,
                Descripcion = Input.Descripcion?.Trim(),
                Precio = Input.Precio,
                CategoriaId = categoriaId,
                MarcaId = marcaId,
                DistribuidoraId = distribuidoraId,
                FechaRegistro = DateTime.UtcNow,
                Activo = true
            };

            _context.Productos.Add(producto);

            /*
            * Guardamos primero el producto para que Entity Framework
            * obtenga su Id generado.
            */
            await _context.SaveChangesAsync();

            var inventario = new InventarioSucursal
            {
                ProductoId = producto.Id,
                SucursalId = sucursalEfectivaId,
                Stock = Input.Stock,
                StockMinimo = Input.StockMinimo,
                FechaActualizacion = DateTime.UtcNow
            };

            _context.InventariosSucursales.Add(inventario);

            await _context.SaveChangesAsync();
            await transaccion.CommitAsync();

            TempData["MensajeExito"] =
                $"El producto {producto.Nombre} fue registrado y agregado al inventario.";

            return RedirectToPage("./Index");
        }
        catch
        {
            await transaccion.RollbackAsync();

            ModelState.AddModelError(
                string.Empty,
                "No se pudo registrar el producto. No se realizó ningún cambio.");

            return Page();
        }
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

        await CargarListasAsync();

        if (EsAdministrador)
        {
            return null;
        }

        if (!usuarioActual.SucursalId.HasValue)
        {
            return Forbid();
        }

        Input.SucursalId = usuarioActual.SucursalId.Value;

        NombreSucursalGerente = await _context.Sucursales
            .Where(s => s.Id == usuarioActual.SucursalId.Value)
            .Select(s => s.Nombre)
            .FirstOrDefaultAsync() ?? string.Empty;

        return null;
    }

    private async Task CargarListasAsync()
    {
        var categorias = await _context.Categorias
            .AsNoTracking()
            .OrderBy(c => c.Nombre)
            .ToListAsync();

        var marcas = await _context.Marcas
            .AsNoTracking()
            .OrderBy(m => m.Nombre)
            .ToListAsync();

        var distribuidoras = await _context.Distribuidoras
            .AsNoTracking()
            .OrderBy(d => d.Nombre)
            .ToListAsync();

        var sucursales = await _context.Sucursales
            .AsNoTracking()
            .Where(s => s.Activa)
            .OrderBy(s => s.Nombre)
            .ToListAsync();

        ListaCategorias = new SelectList(
            categorias,
            nameof(Categoria.Id),
            nameof(Categoria.Nombre),
            Input.CategoriaId);

        ListaMarcas = new SelectList(
            marcas,
            nameof(Marca.Id),
            nameof(Marca.Nombre),
            Input.MarcaId);

        ListaDistribuidoras = new SelectList(
            distribuidoras,
            nameof(Distribuidora.Id),
            nameof(Distribuidora.Nombre),
            Input.DistribuidoraId);

        ListaSucursales = new SelectList(
            sucursales,
            nameof(Sucursal.Id),
            nameof(Sucursal.Nombre),
            Input.SucursalId);
    }
}