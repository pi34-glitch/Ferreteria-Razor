using System.ComponentModel.DataAnnotations;
using FerreteriaRazor.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FerreteriaRazor.Pages.Distribuidoras;

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
        [Display(Name = "Dirección")]
        public string? Direccion { get; set; }

        [Display(Name = "Distribuidora activa")]
        public bool Activa { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var distribuidora = await _context.Distribuidoras
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id);

        if (distribuidora is null)
        {
            return NotFound();
        }

        Input = new InputModel
        {
            Id = distribuidora.Id,
            Nombre = distribuidora.Nombre,
            Contacto = distribuidora.Contacto,
            Telefono = distribuidora.Telefono,
            Correo = distribuidora.Correo,
            Direccion = distribuidora.Direccion,
            Activa = distribuidora.Activa
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var distribuidora = await _context.Distribuidoras
            .FirstOrDefaultAsync(d => d.Id == Input.Id);

        if (distribuidora is null)
        {
            return NotFound();
        }

        string nombreLimpio = Input.Nombre.Trim();

        bool nombreDuplicado = await _context.Distribuidoras
            .AsNoTracking()
            .AnyAsync(d =>
                d.Id != Input.Id &&
                d.Nombre.ToUpper() == nombreLimpio.ToUpper());

        if (nombreDuplicado)
        {
            ModelState.AddModelError(
                nameof(Input.Nombre),
                "Ya existe otra distribuidora con ese nombre.");

            return Page();
        }

        distribuidora.Nombre = nombreLimpio;
        distribuidora.Contacto = Input.Contacto?.Trim();
        distribuidora.Telefono = Input.Telefono?.Trim();
        distribuidora.Correo = Input.Correo?.Trim();
        distribuidora.Direccion = Input.Direccion?.Trim();
        distribuidora.Activa = Input.Activa;

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
            $"La distribuidora {distribuidora.Nombre} fue actualizada.";

        return RedirectToPage("./Index");
    }
}