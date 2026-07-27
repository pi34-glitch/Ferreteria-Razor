using FerreteriaRazor.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FerreteriaRazor.Pages.Usuarios;

[Authorize(Roles = "Administrador")]
public class IndexModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;

    public IndexModel(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public List<UsuarioListadoViewModel> Usuarios { get; set; } = new();

    public async Task OnGetAsync()
    {
        await CargarUsuariosAsync();
    }

    public async Task<IActionResult> OnPostCambiarEstadoAsync(string id)
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
                "No se puede modificar el estado del administrador principal.";

            return RedirectToPage();
        }

        var seraActivado = !usuario.Activo;

        if (seraActivado &&
            roles.Contains("Gerente") &&
            usuario.SucursalId.HasValue)
        {
            var usuariosSucursal = await _userManager.Users
                .Where(u =>
                    u.Id != usuario.Id &&
                    u.SucursalId == usuario.SucursalId &&
                    u.Activo)
                .ToListAsync();

            foreach (var otroUsuario in usuariosSucursal)
            {
                if (await _userManager.IsInRoleAsync(
                    otroUsuario,
                    "Gerente"))
                {
                    TempData["MensajeError"] =
                        "No se puede activar este usuario porque la sucursal ya tiene un gerente activo.";

                    return RedirectToPage();
                }
            }
        }

        usuario.Activo = seraActivado;

        var resultado = await _userManager.UpdateAsync(usuario);

        if (!resultado.Succeeded)
        {
            TempData["MensajeError"] = string.Join(
                ", ",
                resultado.Errors.Select(e => e.Description));

            return RedirectToPage();
        }

        await _userManager.UpdateSecurityStampAsync(usuario);

        TempData["MensajeExito"] = usuario.Activo
            ? $"El usuario {usuario.UserName} fue activado."
            : $"El usuario {usuario.UserName} fue desactivado.";

        return RedirectToPage();
    }

    private async Task CargarUsuariosAsync()
    {
        var usuarios = await _userManager.Users
            .Include(u => u.Sucursal)
            .OrderBy(u => u.NombreCompleto)
            .ToListAsync();

        foreach (var usuario in usuarios)
        {
            var roles = await _userManager.GetRolesAsync(usuario);

            var rol = roles.FirstOrDefault() ?? "Sin rol";

            Usuarios.Add(new UsuarioListadoViewModel
            {
                Id = usuario.Id,
                NombreCompleto = usuario.NombreCompleto,
                Usuario = usuario.UserName ?? string.Empty,
                Correo = usuario.Email ?? string.Empty,
                Rol = rol,
                Sucursal = usuario.Sucursal?.Nombre ?? "Acceso global",
                Activo = usuario.Activo,
                EsAdministrador = rol == "Administrador"
            });
        }
    }

    public class UsuarioListadoViewModel
    {
        public string Id { get; set; } = string.Empty;

        public string NombreCompleto { get; set; } = string.Empty;

        public string Usuario { get; set; } = string.Empty;

        public string Correo { get; set; } = string.Empty;

        public string Rol { get; set; } = string.Empty;

        public string Sucursal { get; set; } = string.Empty;

        public bool Activo { get; set; }

        public bool EsAdministrador { get; set; }
    }
}