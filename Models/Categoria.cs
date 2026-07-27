using System.ComponentModel.DataAnnotations;

namespace FerreteriaRazor.Models;

public class Categoria
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre de la categoría es obligatorio.")]
    [StringLength(100)]
    [Display(Name = "Categoría")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(300)]
    public string? Descripcion { get; set; }

    public bool Activa { get; set; } = true;

    // Una categoría puede tener muchos productos.
    public ICollection<Producto> Productos { get; set; }
        = new List<Producto>();
}