using System.ComponentModel.DataAnnotations;
using FerreteriaRazor.Data;
using FerreteriaRazor.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FerreteriaRazor.Pages.Marcas;

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
        [Required(ErrorMessage = "El nombre de la marca es obligatorio.")]
        [StringLength(100)]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(300)]
        [Display(Name = "Descripción")]
        public string? Descripcion { get; set; }
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

        bool nombreExistente = await _context.Marcas
            .AsNoTracking()
            .AnyAsync(m =>
                m.Nombre.ToUpper() == nombreLimpio.ToUpper());

        if (nombreExistente)
        {
            ModelState.AddModelError(
                nameof(Input.Nombre),
                "Ya existe una marca con ese nombre.");

            return Page();
        }

        var marca = new Marca
        {
            Nombre = nombreLimpio,
            Descripcion = Input.Descripcion?.Trim(),
            Activa = true
        };

        _context.Marcas.Add(marca);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(
                string.Empty,
                "No se pudo registrar la marca.");

            return Page();
        }

        TempData["MensajeExito"] =
            $"La marca {marca.Nombre} fue registrada correctamente.";

        return RedirectToPage("./Index");
    }
}