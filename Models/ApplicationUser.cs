using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace FerreteriaRazor.Models;

public class ApplicationUser : IdentityUser
{
    [Required]
    [StringLength(100)]
    public string NombreCompleto { get; set; } = string.Empty;

    public bool Activo { get; set; } = true;

    public int? SucursalId { get; set; }

    public Sucursal? Sucursal { get; set; }
}