using CloudinaryDotNet;
using FerreteriaRazor.Data;
using FerreteriaRazor.Data.Seed;
using FerreteriaRazor.Models;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "No se configuró la cadena de conexión DefaultConnection.");
}

// PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Cloudinary (almacenamiento de imágenes de productos).
// Render usa un filesystem efímero, así que las imágenes no pueden
// guardarse en disco local: se suben a Cloudinary y se guarda la URL.
var cloudinaryAccount = new Account(
    builder.Configuration["Cloudinary:CloudName"],
    builder.Configuration["Cloudinary:ApiKey"],
    builder.Configuration["Cloudinary:ApiSecret"]);

builder.Services.AddSingleton(new Cloudinary(cloudinaryAccount)
{
    Api = { Secure = true }
});

// Identity
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.User.RequireUniqueEmail = true;

        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = false;

        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan =
            TimeSpan.FromMinutes(10);
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.Configure<SecurityStampValidatorOptions>(options =>
{
    options.ValidationInterval = TimeSpan.FromMinutes(1);
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";

    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;

    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy =
        CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite =
        SameSiteMode.Lax;
});

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AllowAnonymousToPage("/Account/Login");
    options.Conventions.AllowAnonymousToPage("/Account/AccessDenied");
});

/*
 * Los modelos de EF Core (Producto, InventarioSucursal, etc.) tienen
 * propiedades de navegación no-nulables (ej. Producto.Categoria) que
 * los formularios nunca envían directamente (solo envían el Id).
 * Sin esto, ASP.NET Core las trata como [Required] implícito por ser
 * tipos de referencia no-nulables, y el ModelState queda inválido en
 * cada POST aunque el formulario esté correctamente completado.
 */
builder.Services.Configure<Microsoft.AspNetCore.Mvc.MvcOptions>(options =>
{
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
});

var app = builder.Build();

// Render (y otros PaaS) terminan TLS en un proxy inverso y
// reenvían HTTP internamente; sin esto, UseHttpsRedirection/HSTS
// no reconocen el esquema original y pueden causar loops de redirect.
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto
});

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

// Migraciones y datos iniciales
using (var scope = app.Services.CreateScope())
{
    var servicios = scope.ServiceProvider;
    var logger = servicios
        .GetRequiredService<ILogger<Program>>();

    try
    {
        var context = servicios
            .GetRequiredService<ApplicationDbContext>();

        await context.Database.MigrateAsync();

        var userManager = servicios
            .GetRequiredService<UserManager<ApplicationUser>>();

        var roleManager = servicios
            .GetRequiredService<RoleManager<IdentityRole>>();

        await DbInitializer.InicializarAsync(
            context,
            userManager,
            roleManager,
            builder.Configuration);
    }
    catch (Exception ex)
    {
        logger.LogCritical(
            ex,
            "No se pudo preparar la base de datos.");

        throw;
    }
}

app.Run();