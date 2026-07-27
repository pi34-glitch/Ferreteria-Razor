using FerreteriaRazor.Data;
using FerreteriaRazor.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FerreteriaRazor.Pages.Categorias;
[Authorize(Roles = "Administrador")]
public class EditModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public EditModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Categoria Categoria { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var categoria = await _context.Categorias
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);

        if (categoria is null)
        {
            return NotFound();
        }

        Categoria = categoria;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Categoria.Nombre = Categoria.Nombre.Trim();

        var nombreExistente = await _context.Categorias
            .AnyAsync(c =>
                c.Id != Categoria.Id &&
                c.Nombre.ToLower() == Categoria.Nombre.ToLower());

        if (nombreExistente)
        {
            ModelState.AddModelError(
                "Categoria.Nombre",
                "Ya existe otra categoría con ese nombre.");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var categoriaExistente = await _context.Categorias
            .FirstOrDefaultAsync(c => c.Id == Categoria.Id);

        if (categoriaExistente is null)
        {
            return NotFound();
        }

        categoriaExistente.Nombre = Categoria.Nombre;
        categoriaExistente.Descripcion = Categoria.Descripcion;
        categoriaExistente.Activa = Categoria.Activa;

        await _context.SaveChangesAsync();

        TempData["Mensaje"] =
            "La categoría fue actualizada correctamente.";

        return RedirectToPage("./Index");
    }
}