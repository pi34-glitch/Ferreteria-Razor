using System.ComponentModel.DataAnnotations;

namespace FerreteriaRazor.Models;

public class Marca
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre de la marca es obligatorio.")]
    [StringLength(100)]
    [Display(Name = "Marca")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(300)]
    public string? Descripcion { get; set; }

    public bool Activa { get; set; } = true;

    // Una marca puede tener muchos productos.
    public ICollection<Producto> Productos { get; set; } 
        = new List<Producto>();
}