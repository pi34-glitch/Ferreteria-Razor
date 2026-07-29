using System.ComponentModel.DataAnnotations;
using FerreteriaRazor.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FerreteriaRazor.Pages.Marcas;

[Authorize(Roles = "Administrador")]
public class EditModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public EditModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre de la marca es obligatorio.")]
        [StringLength(100)]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(300)]
        [Display(Name = "Descripción")]
        public string? Descripcion { get; set; }

        [Display(Name = "Marca activa")]
        public bool Activa { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var marca = await _context.Marcas
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);

        if (marca is null)
        {
            return NotFound();
        }

        Input = new InputModel
        {
            Id = marca.Id,
            Nombre = marca.Nombre,
            Descripcion = marca.Descripcion,
            Activa = marca.Activa
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var marca = await _context.Marcas
            .FirstOrDefaultAsync(m => m.Id == Input.Id);

        if (marca is null)
        {
            return NotFound();
        }

        string nombreLimpio = Input.Nombre.Trim();

        bool nombreDuplicado = await _context.Marcas
            .AsNoTracking()
            .AnyAsync(m =>
                m.Id != Input.Id &&
                m.Nombre.ToUpper() == nombreLimpio.ToUpper());

        if (nombreDuplicado)
        {
            ModelState.AddModelError(
                nameof(Input.Nombre),
                "Ya existe otra marca con ese nombre.");

            return Page();
        }

        marca.Nombre = nombreLimpio;
        marca.Descripcion = Input.Descripcion?.Trim();
        marca.Activa = Input.Activa;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(
                string.Empty,
                "No se pudieron guardar los cambios.");

            return Page();
        }

        TempData["MensajeExito"] =
            $"La marca {marca.Nombre} fue actualizada correctamente.";

        return RedirectToPage("./Index");
    }
}