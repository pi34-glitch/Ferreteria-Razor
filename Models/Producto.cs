using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FerreteriaRazor.Models;

public class Producto
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Codigo { get; set; } = string.Empty;

    [Required]
    [StringLength(120)]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Descripcion { get; set; }

    [Column(TypeName = "numeric(12,2)")]
    public decimal Precio { get; set; }

    public string? ImagenUrl { get; set; }

    public bool Activo { get; set; } = true;

    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    public int CategoriaId { get; set; }
    public Categoria Categoria { get; set; } = null!;

    public int MarcaId { get; set; }
    public Marca Marca { get; set; } = null!;

    public int DistribuidoraId { get; set; }
    public Distribuidora Distribuidora { get; set; } = null!;

    public ICollection<InventarioSucursal> InventariosSucursales { get; set; }
        = new List<InventarioSucursal>();
}