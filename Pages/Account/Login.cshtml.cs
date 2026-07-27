using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using FerreteriaRazor.Data;
using FerreteriaRazor.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FerreteriaRazor.Pages.Account;

[AllowAnonymous]
public class LoginModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;

    public LoginModel(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _context = context;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public SelectList ListaSucursales { get; set; } = null!;

    public string? ReturnUrl { get; set; }

    public class InputModel
    {
        [Required(ErrorMessage = "Debe ingresar su usuario o correo.")]
        [Display(Name = "Usuario o correo")]
        public string Usuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe ingresar su contraseña.")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe seleccionar una sucursal.")]
        [Display(Name = "Sucursal")]
        public int? SucursalId { get; set; }

        [Display(Name = "Recordarme")]
        public bool Recordarme { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(
        string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToPage("/Index");
        }

        ReturnUrl = returnUrl ?? Url.Content("~/");

        await CargarSucursalesAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(
        string? returnUrl = null)
    {
        ReturnUrl = returnUrl ?? Url.Content("~/");

        await CargarSucursalesAsync();

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var identificador = Input.Usuario.Trim();

        var usuario = await _userManager.Users
            .FirstOrDefaultAsync(u =>
                u.UserName == identificador ||
                u.Email == identificador);

        if (usuario is null || !usuario.Activo)
        {
            AgregarErrorLogin();
            return Page();
        }

        var sucursal = await _context.Sucursales
            .AsNoTracking()
            .FirstOrDefaultAsync(s =>
                s.Id == Input.SucursalId &&
                s.Activa);

        if (sucursal is null)
        {
            ModelState.AddModelError(
                nameof(Input.SucursalId),
                "La sucursal seleccionada no está disponible.");

            return Page();
        }

        var roles = await _userManager.GetRolesAsync(usuario);

        var esAdministrador = roles.Contains("Administrador");

        /*
         * El administrador puede seleccionar cualquier sucursal.
         * El gerente y el empleado solamente pueden seleccionar
         * la sucursal que tienen asignada.
         */
        if (!esAdministrador &&
            usuario.SucursalId != Input.SucursalId)
        {
            ModelState.AddModelError(
                string.Empty,
                "El usuario no se encuentra registrado en la sucursal seleccionada.");

            return Page();
        }

        var resultadoPassword =
            await _signInManager.CheckPasswordSignInAsync(
                usuario,
                Input.Password,
                lockoutOnFailure: true);

        if (resultadoPassword.IsLockedOut)
        {
            ModelState.AddModelError(
                string.Empty,
                "La cuenta se encuentra bloqueada temporalmente por varios intentos fallidos.");

            return Page();
        }

        if (!resultadoPassword.Succeeded)
        {
            AgregarErrorLogin();
            return Page();
        }

        var claims = new List<Claim>
        {
            new("SucursalId", sucursal.Id.ToString()),
            new("SucursalNombre", sucursal.Nombre)
        };

        await _signInManager.SignInWithClaimsAsync(
            usuario,
            Input.Recordarme,
            claims);

        if (Url.IsLocalUrl(ReturnUrl))
        {
            return LocalRedirect(ReturnUrl);
        }

        return RedirectToPage("/Index");
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
            nameof(Sucursal.Nombre));
    }

    private void AgregarErrorLogin()
    {
        ModelState.AddModelError(
            string.Empty,
            "El usuario, la contraseña o la sucursal seleccionada no son correctos.");
    }
}