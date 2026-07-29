using FerreteriaRazor.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FerreteriaRazor.Data;

public class ApplicationDbContext
    : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Producto> Productos { get; set; } = null!;
    public DbSet<Categoria> Categorias { get; set; } = null!;
    public DbSet<Marca> Marcas { get; set; } = null!;
    public DbSet<Distribuidora> Distribuidoras { get; set; } = null!;
    public DbSet<Sucursal> Sucursales { get; set; } = null!; 
    public DbSet<InventarioSucursal> InventariosSucursales { get; set; } = null!;
    public DbSet<Venta> Ventas { get; set; } = null!;
    public DbSet<DetalleVenta> DetallesVenta { get; set; } = null!;
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Producto>()
            .HasOne(p => p.Marca)
            .WithMany(m => m.Productos)
            .HasForeignKey(p => p.MarcaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Producto>()
            .HasOne(p => p.Distribuidora)
            .WithMany(d => d.Productos)
            .HasForeignKey(p => p.DistribuidoraId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Producto>()
            .HasOne(p => p.Categoria)
            .WithMany(c => c.Productos)
            .HasForeignKey(p => p.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ApplicationUser>()
            .HasOne(u => u.Sucursal)
            .WithMany(s => s.Usuarios)
            .HasForeignKey(u => u.SucursalId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<InventarioSucursal>()
            .HasIndex(i => new
            {
                i.ProductoId,
                i.SucursalId
            })
            .IsUnique();

        modelBuilder.Entity<InventarioSucursal>()
            .HasOne(i => i.Producto)
            .WithMany(p => p.InventariosSucursales)
            .HasForeignKey(i => i.ProductoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<InventarioSucursal>()
            .HasOne(i => i.Sucursal)
            .WithMany(s => s.InventariosSucursales)
            .HasForeignKey(i => i.SucursalId)
            .OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder.Entity<Venta>()
            .HasOne(v => v.Sucursal)
            .WithMany()
            .HasForeignKey(v => v.SucursalId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Venta>()
            .HasOne(v => v.Usuario)
            .WithMany()
            .HasForeignKey(v => v.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DetalleVenta>()
            .HasOne(d => d.Venta)
            .WithMany(v => v.Detalles)
            .HasForeignKey(d => d.VentaId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DetalleVenta>()
            .HasOne(d => d.Producto)
            .WithMany()
            .HasForeignKey(d => d.ProductoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}