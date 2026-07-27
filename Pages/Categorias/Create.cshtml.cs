using FerreteriaRazor.Data;
using FerreteriaRazor.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FerreteriaRazor.Pages.Categorias;

[Authorize(Roles = "Administrador")]
public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public CreateModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Categoria Categoria { get; set; } = new();

    public void OnGet()
    {
        Categoria.Activa = true;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Categoria.Nombre = Categoria.Nombre.Trim();

        var nombreExistente = await _context.Categorias
            .AnyAsync(c =>
                c.Nombre.ToLower() == Categoria.Nombre.ToLower());

        if (nombreExistente)
        {
            ModelState.AddModelError(
                "Categoria.Nombre",
                "Ya existe una categoría con ese nombre.");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        _context.Categorias.Add(Categoria);
        await _context.SaveChangesAsync();

        TempData["Mensaje"] =
            "La categoría fue registrada correctamente.";

        return RedirectToPage("./Index");
    }
}