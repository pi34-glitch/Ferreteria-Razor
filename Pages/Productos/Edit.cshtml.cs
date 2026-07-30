using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using FerreteriaRazor.Data;
using FerreteriaRazor.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace FerreteriaRazor.Pages.Productos;

[Authorize(Roles = "Administrador")]
public class EditModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly Cloudinary _cloudinary;

    public EditModel(
        ApplicationDbContext context,
        Cloudinary cloudinary)
    {
        _context = context;
        _cloudinary = cloudinary;
    }

    [BindProperty]
    public Producto Producto { get; set; } = null!;

    [BindProperty]
    public IFormFile? NuevaImagen { get; set; }

    public SelectList Categorias { get; set; } = null!;
    public SelectList Marcas { get; set; } = null!;
    public SelectList Distribuidoras { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var producto = await _context.Productos
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (producto is null)
        {
            return NotFound();
        }

        Producto = producto;

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

        var productoExistente = await _context.Productos
            .FirstOrDefaultAsync(p => p.Id == Producto.Id);

        if (productoExistente is null)
        {
            return NotFound();
        }

        if (NuevaImagen is not null && NuevaImagen.Length > 0)
        {
            var errorImagen = ValidarImagen(NuevaImagen);

            if (errorImagen is not null)
            {
                ModelState.AddModelError(
                    nameof(NuevaImagen),
                    errorImagen);

                return Page();
            }

            productoExistente.ImagenUrl =
                await GuardarImagenAsync(NuevaImagen);
        }

        productoExistente.Codigo = Producto.Codigo;
        productoExistente.Nombre = Producto.Nombre;
        productoExistente.Descripcion = Producto.Descripcion;
        productoExistente.Precio = Producto.Precio;
        productoExistente.CategoriaId = Producto.CategoriaId;
        productoExistente.MarcaId = Producto.MarcaId;
        productoExistente.DistribuidoraId = Producto.DistribuidoraId;
        productoExistente.Activo = Producto.Activo;

        await _context.SaveChangesAsync();

        TempData["Mensaje"] = "El producto fue actualizado correctamente.";

        return RedirectToPage("./Index");
    }

    private async Task CargarListasAsync()
    {
        Categorias = new SelectList(
            await _context.Categorias
                .AsNoTracking()
                .Where(c => c.Activa)
                .OrderBy(c => c.Nombre)
                .ToListAsync(),
            "Id",
            "Nombre",
            Producto?.CategoriaId);

        Marcas = new SelectList(
            await _context.Marcas
                .AsNoTracking()
                .Where(m => m.Activa)
                .OrderBy(m => m.Nombre)
                .ToListAsync(),
            "Id",
            "Nombre",
            Producto?.MarcaId);

        Distribuidoras = new SelectList(
            await _context.Distribuidoras
                .AsNoTracking()
                .Where(d => d.Activa)
                .OrderBy(d => d.Nombre)
                .ToListAsync(),
            "Id",
            "Nombre",
            Producto?.DistribuidoraId);
    }

    private static string? ValidarImagen(IFormFile imagen)
    {
        const long tamanoMaximo = 5 * 1024 * 1024;

        string[] tiposPermitidos =
        {
            "image/jpeg",
            "image/png",
            "image/webp"
        };

        if (imagen.Length > tamanoMaximo)
        {
            return "La imagen no puede superar los 5 MB.";
        }

        if (!tiposPermitidos.Contains(imagen.ContentType))
        {
            return "Solo se permiten imágenes JPG, PNG o WEBP.";
        }

        return null;
    }

    private async Task<string> GuardarImagenAsync(IFormFile imagen)
    {
        await using var stream = imagen.OpenReadStream();

        var parametrosSubida = new ImageUploadParams
        {
            File = new FileDescription(imagen.FileName, stream),
            Folder = "ferreteria/productos"
        };

        var resultado = await _cloudinary.UploadAsync(parametrosSubida);

        if (resultado.Error is not null)
        {
            throw new InvalidOperationException(
                $"No se pudo subir la imagen a Cloudinary: {resultado.Error.Message}");
        }

        return resultado.SecureUrl.ToString();
    }
}