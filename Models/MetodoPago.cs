using System.ComponentModel.DataAnnotations;

namespace FerreteriaRazor.Models;

public enum MetodoPago
{
    [Display(Name = "Efectivo")]
    Efectivo = 1,

    [Display(Name = "QR")]
    QR = 2,

    [Display(Name = "Tarjeta")]
    Tarjeta = 3
}