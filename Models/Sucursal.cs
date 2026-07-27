using System.ComponentModel.DataAnnotations;

namespace FerreteriaRazor.Models;

public class Sucursal
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Direccion { get; set; } = string.Empty;

    [StringLength(30)]
    public string? Telefono { get; set; }

    public bool Activa { get; set; } = true;

    public ICollection<ApplicationUser> Usuarios { get; set; }
        = new List<ApplicationUser>();
    public ICollection<InventarioSucursal> InventariosSucursales { get; set; }
        = new List<InventarioSucursal>();
}