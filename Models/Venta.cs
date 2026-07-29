using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FerreteriaRazor.Models;

public class Venta
{
    public int Id { get; set; }

    public DateTime Fecha { get; set; } = DateTime.UtcNow;

    [Column(TypeName = "numeric(12,2)")]
    public decimal Total { get; set; }
    
    [Required]
    public MetodoPago MetodoPago { get; set; }

    [Required]
    public int SucursalId { get; set; }

    public Sucursal Sucursal { get; set; } = null!;

    [Required]
    public string UsuarioId { get; set; } = string.Empty;

    public ApplicationUser Usuario { get; set; } = null!;

    public ICollection<DetalleVenta> Detalles { get; set; }
        = new List<DetalleVenta>();
}