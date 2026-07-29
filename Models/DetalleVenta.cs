using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FerreteriaRazor.Models;

public class DetalleVenta
{
    public int Id { get; set; }

    [Required]
    public int VentaId { get; set; }

    public Venta Venta { get; set; } = null!;

    [Required]
    public int ProductoId { get; set; }

    public Producto Producto { get; set; } = null!;

    [Range(1, int.MaxValue)]
    public int Cantidad { get; set; }

    [Column(TypeName = "numeric(12,2)")]
    public decimal PrecioUnitario { get; set; }

    [Column(TypeName = "numeric(12,2)")]
    public decimal Subtotal { get; set; }
}