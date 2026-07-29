using System.ComponentModel.DataAnnotations;
using FerreteriaRazor.Data;
using FerreteriaRazor.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FerreteriaRazor.Pages.Distribuidoras;

[Authorize(Roles = "Administrador")]
public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public CreateModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(150)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Contacto { get; set; }

        [Phone(ErrorMessage = "El teléfono no es válido.")]
        [StringLength(30)]
        public string? Telefono { get; set; }

        [EmailAddress(ErrorMessage = "El correo no es válido.")]
        [StringLength(150)]
        public string? Correo { get; set; }

        [StringLength(250)]
        public string? Direccion { get; set; }
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        string nombreLimpio = Input.Nombre.Trim();

        bool nombreExistente = await _context.Distribuidoras
            .AsNoTracking()
            .AnyAsync(d =>
                d.Nombre.ToUpper() == nombreLimpio.ToUpper());

        if (nombreExistente)
        {
            ModelState.AddModelError(
                nameof(Input.Nombre),
                "Ya existe una distribuidora con ese nombre.");

            return Page();
        }

        var distribuidora = new Distribuidora
        {
            Nombre = nombreLimpio,
            Contacto = Input.Contacto?.Trim(),
            Telefono = Input.Telefono?.Trim(),
            Correo = Input.Correo?.Trim(),
            Direccion = Input.Direccion?.Trim(),
            Activa = true
        };

        _context.Distribuidoras.Add(distribuidora);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(
                string.Empty,
                "No se pudo registrar la distribuidora.");

            return Page();
        }

        TempData["MensajeExito"] =
            $"La distribuidora {distribuidora.Nombre} fue registrada correctamente.";

        return RedirectToPage("./Index");
    }
}