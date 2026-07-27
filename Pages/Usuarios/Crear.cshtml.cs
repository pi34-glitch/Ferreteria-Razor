using System.ComponentModel.DataAnnotations;
using FerreteriaRazor.Data;
using FerreteriaRazor.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FerreteriaRazor.Pages.Usuarios;

[Authorize(Roles = "Administrador")]
public class CrearModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;

    public CrearModel(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public SelectList ListaSucursales { get; set; } = null!;

    public List<SelectListItem> ListaRoles { get; set; } = new();

    public class InputModel
    {
        [Required(ErrorMessage = "El nombre completo es obligatorio.")]
        [StringLength(100)]
        [Display(Name = "Nombre completo")]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
        [StringLength(50)]
        [Display(Name = "Usuario")]
        public string Usuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "El correo no es válido.")]
        [Display(Name = "Correo electrónico")]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        [MinLength(8)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe seleccionar un rol.")]
        [Display(Name = "Rol")]
        public string Rol { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe seleccionar una sucursal.")]
        [Display(Name = "Sucursal")]
        public int? SucursalId { get; set; }
    }

    public async Task OnGetAsync()
    {
        await CargarListasAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await CargarListasAsync();

        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (Input.Rol != "Gerente" &&
            Input.Rol != "Empleado")
        {
            ModelState.AddModelError(
                nameof(Input.Rol),
                "El rol seleccionado no es válido.");

            return Page();
        }

        var sucursalExiste = await _context.Sucursales
            .AnyAsync(s =>
                s.Id == Input.SucursalId &&
                s.Activa);

        if (!sucursalExiste)
        {
            ModelState.AddModelError(
                nameof(Input.SucursalId),
                "La sucursal seleccionada no existe o está inactiva.");

            return Page();
        }

        if (Input.Rol == "Gerente")
        {
            var usuariosSucursal = await _userManager.Users
                .Where(u =>
                    u.SucursalId == Input.SucursalId &&
                    u.Activo)
                .ToListAsync();

            foreach (var usuarioSucursal in usuariosSucursal)
            {
                if (await _userManager.IsInRoleAsync(
                    usuarioSucursal,
                    "Gerente"))
                {
                    ModelState.AddModelError(
                        nameof(Input.Rol),
                        "Esta sucursal ya tiene un gerente activo.");

                    return Page();
                }
            }
        }

        var usuarioExistente = await _userManager
            .FindByNameAsync(Input.Usuario.Trim());

        if (usuarioExistente is not null)
        {
            ModelState.AddModelError(
                nameof(Input.Usuario),
                "El nombre de usuario ya está registrado.");

            return Page();
        }

        var correoExistente = await _userManager
            .FindByEmailAsync(Input.Correo.Trim());

        if (correoExistente is not null)
        {
            ModelState.AddModelError(
                nameof(Input.Correo),
                "El correo ya está registrado.");

            return Page();
        }

        var usuario = new ApplicationUser
        {
            NombreCompleto = Input.NombreCompleto.Trim(),
            UserName = Input.Usuario.Trim(),
            Email = Input.Correo.Trim(),
            EmailConfirmed = true,
            Activo = true,
            SucursalId = Input.SucursalId
        };

        var resultado = await _userManager.CreateAsync(
            usuario,
            Input.Password);

        if (!resultado.Succeeded)
        {
            foreach (var error in resultado.Errors)
            {
                ModelState.AddModelError(
                    string.Empty,
                    error.Description);
            }

            return Page();
        }

        var resultadoRol = await _userManager.AddToRoleAsync(
            usuario,
            Input.Rol);

        if (!resultadoRol.Succeeded)
        {
            await _userManager.DeleteAsync(usuario);

            foreach (var error in resultadoRol.Errors)
            {
                ModelState.AddModelError(
                    string.Empty,
                    error.Description);
            }

            return Page();
        }

        TempData["MensajeExito"] =
            $"El usuario {usuario.UserName} fue registrado correctamente.";

        return RedirectToPage("./Index");
    }

    private async Task CargarListasAsync()
    {
        var sucursales = await _context.Sucursales
            .AsNoTracking()
            .Where(s => s.Activa)
            .OrderBy(s => s.Nombre)
            .ToListAsync();

        ListaSucursales = new SelectList(
            sucursales,
            nameof(Sucursal.Id),
            nameof(Sucursal.Nombre));

        ListaRoles = new List<SelectListItem>
        {
            new()
            {
                Value = "Gerente",
                Text = "Gerente"
            },
            new()
            {
                Value = "Empleado",
                Text = "Empleado"
            }
        };
    }
}