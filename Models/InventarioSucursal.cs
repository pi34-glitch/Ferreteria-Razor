using System.ComponentModel.DataAnnotations;

namespace FerreteriaRazor.Models;

public class InventarioSucursal
{
    public int Id { get; set; }

    [Required]
    public int ProductoId { get; set; }

    public Producto Producto { get; set; } = null!;

    [Required]
    public int SucursalId { get; set; }

    public Sucursal Sucursal { get; set; } = null!;

    [Range(0, int.MaxValue)]
    public int Stock { get; set; }

    [Range(0, int.MaxValue)]
    public int StockMinimo { get; set; }

    public DateTime FechaActualizacion { get; set; }
        = DateTime.UtcNow;
}