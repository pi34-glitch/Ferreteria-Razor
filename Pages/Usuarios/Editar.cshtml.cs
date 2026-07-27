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
public class EditarModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;

    public EditarModel(
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
        [Required]
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre completo es obligatorio.")]
        [StringLength(100)]
        [Display(Name = "Nombre completo")]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
        [StringLength(50)]
        [Display(Name = "Usuario")]
        public string Usuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "El correo electrónico no es válido.")]
        [Display(Name = "Correo electrónico")]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe seleccionar un rol.")]
        [Display(Name = "Rol")]
        public string Rol { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe seleccionar una sucursal.")]
        [Display(Name = "Sucursal")]
        public int? SucursalId { get; set; }

        [Display(Name = "Usuario activo")]
        public bool Activo { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return NotFound();
        }

        var usuario = await _userManager.FindByIdAsync(id);

        if (usuario is null)
        {
            return NotFound();
        }

        var roles = await _userManager.GetRolesAsync(usuario);

        if (roles.Contains("Administrador"))
        {
            TempData["MensajeError"] =
                "El administrador principal no puede editarse desde este módulo.";

            return RedirectToPage("./Index");
        }

        Input = new InputModel
        {
            Id = usuario.Id,
            NombreCompleto = usuario.NombreCompleto,
            Usuario = usuario.UserName ?? string.Empty,
            Correo = usuario.Email ?? string.Empty,
            Rol = roles.FirstOrDefault() ?? string.Empty,
            SucursalId = usuario.SucursalId,
            Activo = usuario.Activo
        };

        await CargarListasAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await CargarListasAsync();

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var usuario = await _userManager.FindByIdAsync(Input.Id);

        if (usuario is null)
        {
            return NotFound();
        }

        var rolesActuales = await _userManager.GetRolesAsync(usuario);

        if (rolesActuales.Contains("Administrador"))
        {
            ModelState.AddModelError(
                string.Empty,
                "El administrador principal no puede modificarse.");

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

        /*
         * Si se asignará el rol Gerente, verificamos que no exista
         * otro gerente activo en la misma sucursal.
         */
        if (Input.Rol == "Gerente" && Input.Activo)
        {
            var usuariosSucursal = await _userManager.Users
                .Where(u =>
                    u.Id != usuario.Id &&
                    u.SucursalId == Input.SucursalId &&
                    u.Activo)
                .ToListAsync();

            foreach (var otroUsuario in usuariosSucursal)
            {
                if (await _userManager.IsInRoleAsync(
                    otroUsuario,
                    "Gerente"))
                {
                    ModelState.AddModelError(
                        nameof(Input.Rol),
                        "La sucursal seleccionada ya tiene un gerente activo.");

                    return Page();
                }
            }
        }

        var usuarioConMismoNombre = await _userManager
            .FindByNameAsync(Input.Usuario.Trim());

        if (usuarioConMismoNombre is not null &&
            usuarioConMismoNombre.Id != usuario.Id)
        {
            ModelState.AddModelError(
                nameof(Input.Usuario),
                "El nombre de usuario ya está registrado.");

            return Page();
        }

        var usuarioConMismoCorreo = await _userManager
            .FindByEmailAsync(Input.Correo.Trim());

        if (usuarioConMismoCorreo is not null &&
            usuarioConMismoCorreo.Id != usuario.Id)
        {
            ModelState.AddModelError(
                nameof(Input.Correo),
                "El correo electrónico ya está registrado.");

            return Page();
        }

        usuario.NombreCompleto = Input.NombreCompleto.Trim();
        usuario.UserName = Input.Usuario.Trim();
        usuario.Email = Input.Correo.Trim();
        usuario.SucursalId = Input.SucursalId;
        usuario.Activo = Input.Activo;

        var resultadoActualizar =
            await _userManager.UpdateAsync(usuario);

        if (!resultadoActualizar.Succeeded)
        {
            foreach (var error in resultadoActualizar.Errors)
            {
                ModelState.AddModelError(
                    string.Empty,
                    error.Description);
            }

            return Page();
        }

        var rolActual = rolesActuales.FirstOrDefault();

        if (rolActual != Input.Rol)
        {
            if (rolesActuales.Any())
            {
                var resultadoQuitarRoles =
                    await _userManager.RemoveFromRolesAsync(
                        usuario,
                        rolesActuales);

                if (!resultadoQuitarRoles.Succeeded)
                {
                    foreach (var error in resultadoQuitarRoles.Errors)
                    {
                        ModelState.AddModelError(
                            string.Empty,
                            error.Description);
                    }

                    return Page();
                }
            }

            var resultadoAgregarRol =
                await _userManager.AddToRoleAsync(
                    usuario,
                    Input.Rol);

            if (!resultadoAgregarRol.Succeeded)
            {
                /*
                 * Intentamos restaurar el rol anterior si falla
                 * la asignación del nuevo rol.
                 */
                if (!string.IsNullOrWhiteSpace(rolActual))
                {
                    await _userManager.AddToRoleAsync(
                        usuario,
                        rolActual);
                }

                foreach (var error in resultadoAgregarRol.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description);
                }

                return Page();
            }
        }

        /*
         * Fuerza la renovación de la cookie de seguridad.
         * Esto es importante cuando cambia el rol, la sucursal
         * o el estado del usuario.
         */
        await _userManager.UpdateSecurityStampAsync(usuario);

        TempData["MensajeExito"] =
            $"El usuario {usuario.UserName} fue actualizado correctamente.";

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