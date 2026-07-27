using FerreteriaRazor.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FerreteriaRazor.Data.Seed;

public static class DbInitializer
{
    public static async Task InicializarAsync(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        await context.Database.MigrateAsync();

        await CrearRolesAsync(roleManager);
        await CrearSucursalesAsync(context);
        await CrearAdministradorAsync(userManager);
    }

    private static async Task CrearRolesAsync(
        RoleManager<IdentityRole> roleManager)
    {
        string[] roles =
        {
            "Administrador",
            "Gerente",
            "Empleado"
        };

        foreach (var nombreRol in roles)
        {
            if (!await roleManager.RoleExistsAsync(nombreRol))
            {
                var resultado = await roleManager.CreateAsync(
                    new IdentityRole(nombreRol));

                if (!resultado.Succeeded)
                {
                    var errores = string.Join(
                        ", ",
                        resultado.Errors.Select(e => e.Description));

                    throw new InvalidOperationException(
                        $"No se pudo crear el rol {nombreRol}: {errores}");
                }
            }
        }
    }

    private static async Task CrearSucursalesAsync(
        ApplicationDbContext context)
    {
        if (await context.Sucursales.AnyAsync())
        {
            return;
        }

        var sucursales = new List<Sucursal>
        {
            new()
            {
                Nombre = "Sucursal Central",
                Direccion = "Avenida principal",
                Telefono = "00000001",
                Activa = true
            },
            new()
            {
                Nombre = "Sucursal Norte",
                Direccion = "Zona norte",
                Telefono = "00000002",
                Activa = true
            },
            new()
            {
                Nombre = "Sucursal Sur",
                Direccion = "Zona sur",
                Telefono = "00000003",
                Activa = true
            }
        };

        context.Sucursales.AddRange(sucursales);

        await context.SaveChangesAsync();
    }

    private static async Task CrearAdministradorAsync(
        UserManager<ApplicationUser> userManager)
    {
        const string correoAdministrador =
            "admin@ferreteria.com";

        var administrador = await userManager
            .FindByEmailAsync(correoAdministrador);

        if (administrador is null)
        {
            administrador = new ApplicationUser
            {
                UserName = "administrador",
                Email = correoAdministrador,
                NombreCompleto = "Administrador General",
                EmailConfirmed = true,
                Activo = true,

                // El administrador no pertenece a una sucursal fija.
                SucursalId = null
            };

            var resultado = await userManager.CreateAsync(
                administrador,
                "Admin1234");

            if (!resultado.Succeeded)
            {
                var errores = string.Join(
                    ", ",
                    resultado.Errors.Select(e => e.Description));

                throw new InvalidOperationException(
                    $"No se pudo crear el administrador: {errores}");
            }
        }

        if (!await userManager.IsInRoleAsync(
            administrador,
            "Administrador"))
        {
            await userManager.AddToRoleAsync(
                administrador,
                "Administrador");
        }
    }
}