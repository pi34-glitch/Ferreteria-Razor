using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FerreteriaRazor.Pages.Productos;

[Authorize(Roles = "Administrador")]
public class CreateModel : PageModel
{
    
    public IActionResult OnGet()
    {
        TempData["MensajeInfo"] =
            "Los productos nuevos deben registrarse desde Inventario por sucursal.";

        return RedirectToPage("/Inventario/NuevoProducto");
    }
}