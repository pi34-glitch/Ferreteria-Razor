using System.ComponentModel.DataAnnotations;
using FerreteriaRazor.Data;
using FerreteriaRazor.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FerreteriaRazor.Pages.Inventario;

[Authorize(Roles = "Administrador,Gerente")]
public class EditarModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public EditarModel(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string NombreProducto { get; set; } = string.Empty;

    public string NombreSucursal { get; set; } = string.Empty;

    public string CodigoProducto { get; set; } = string.Empty;

    public class InputModel
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [Range(0, int.MaxValue,
            ErrorMessage = "El stock no puede ser negativo.")]
        [Display(Name = "Stock actual")]
        public int Stock { get; set; }

        [Required]
        [Range(0, int.MaxValue,
            ErrorMessage = "El stock mínimo no puede ser negativo.")]
        [Display(Name = "Stock mínimo")]
        public int StockMinimo { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        
        var inventario = await BuscarInventarioAutorizadoAsync(id);

        if (inventario is null)
        {
            return NotFound();
        }

        Input = new InputModel
        {
            Id = inventario.Id,
            Stock = inventario.Stock,
            StockMinimo = inventario.StockMinimo
        };

        NombreProducto = inventario.Producto.Nombre;
        NombreSucursal = inventario.Sucursal.Nombre;
        CodigoProducto = inventario.Producto.Codigo;

        return Page();
        
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var inventario = await BuscarInventarioAutorizadoAsync(Input.Id);

        if (inventario is null)
        {
            return NotFound();
        }

        NombreProducto = inventario.Producto.Nombre;
        NombreSucursal = inventario.Sucursal.Nombre;

        if (!ModelState.IsValid)
        {
            return Page();
        }

        bool huboCambios =
            inventario.Stock != Input.Stock ||
            inventario.StockMinimo != Input.StockMinimo;
        if (!huboCambios)
        {
            TempData["MensajeInfo"] =
                "No se realizaron cambios en el inventario.";

            return RedirectToPage("./Index");
        }
        inventario.Stock = Input.Stock;
        inventario.StockMinimo = Input.StockMinimo;
        inventario.FechaActualizacion = DateTime.UtcNow;
        CodigoProducto = inventario.Producto.Codigo;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(
                string.Empty,
                "No se pudo actualizar el inventario. Intente nuevamente.");

            return Page();
        }
    
        TempData["MensajeExito"] =
            "El inventario fue actualizado correctamente.";

        return RedirectToPage("./Index");
        
    }

    private async Task<InventarioSucursal?>
        BuscarInventarioAutorizadoAsync(int id)
    {
        var usuarioActual = await _userManager.GetUserAsync(User);

        if (usuarioActual is null)
        {
            return null;
        }

        var esAdministrador = await _userManager.IsInRoleAsync(
            usuarioActual,
            "Administrador");

        var consulta = _context.InventariosSucursales
            .Include(i => i.Producto)
            .Include(i => i.Sucursal)
            .Where(i =>
                i.Id == id &&
                i.Producto.Activo);
        if (!esAdministrador)
        {
            if (!usuarioActual.SucursalId.HasValue)
            {
                return null;
            }
            int sucursalId = usuarioActual.SucursalId.Value;

            consulta = consulta.Where(i =>
                i.SucursalId == sucursalId &&
                i.Sucursal.Activa);
        }

        return await consulta.FirstOrDefaultAsync();
    }
}