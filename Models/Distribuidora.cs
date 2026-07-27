using System.ComponentModel.DataAnnotations;

namespace FerreteriaRazor.Models;

public class Distribuidora
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre de la distribuidora es obligatorio.")]
    [StringLength(150)]
    [Display(Name = "Distribuidora")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Contacto { get; set; }

    [Phone(ErrorMessage = "El número de teléfono no es válido.")]
    [StringLength(30)]
    public string? Telefono { get; set; }

    [EmailAddress(ErrorMessage = "El correo electrónico no es válido.")]
    [StringLength(150)]
    public string? Correo { get; set; }

    [StringLength(250)]
    public string? Direccion { get; set; }

    public bool Activa { get; set; } = true;

    // Una distribuidora puede proporcionar muchos productos.
    public ICollection<Producto> Productos { get; set; } 
        = new List<Producto>();
}