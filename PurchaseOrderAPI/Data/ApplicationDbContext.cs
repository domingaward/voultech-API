using Microsoft.EntityFrameworkCore;
using PurchaseOrderAPI.Models;

namespace PurchaseOrderAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<OrdenCompra> OrdenesCompra { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<OrdenProducto> OrdenProductos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure OrdenCompra
            modelBuilder.Entity<OrdenCompra>(entity =>
            {
                entity.ToTable("OrdenesCompra");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Cliente).IsRequired().HasMaxLength(100);
                entity.Property(e => e.FechaCreacion).IsRequired();
                entity.Property(e => e.Total).HasColumnType("decimal(18,2)");
            });

            // Configure Producto
            modelBuilder.Entity<Producto>(entity =>
            {
                entity.ToTable("Productos");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Precio).HasColumnType("decimal(18,2)").IsRequired();
            });

            // Configure OrdenProducto
            modelBuilder.Entity<OrdenProducto>(entity =>
            {
                entity.ToTable("OrdenProductos");
                entity.HasKey(e => e.Id);

                // Configure relationships
                entity.HasOne(e => e.OrdenCompra)
                    .WithMany(o => o.OrdenProductos)
                    .HasForeignKey(e => e.OrdenId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Producto)
                    .WithMany(p => p.OrdenProductos)
                    .HasForeignKey(e => e.ProductoId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ============ SEED DATA SIMPLIFICADA ============
            
            // 6 Productos básicos para la empresa
            modelBuilder.Entity<Producto>().HasData(
                new Producto { Id = 1, Nombre = "Laptop HP Pavilion", Precio = 15000.00m },
                new Producto { Id = 2, Nombre = "Mouse Logitech", Precio = 500.00m },
                new Producto { Id = 3, Nombre = "Teclado Mecánico", Precio = 1200.00m },
                new Producto { Id = 4, Nombre = "Monitor 24 pulgadas", Precio = 4500.00m },
                new Producto { Id = 5, Nombre = "Impresora Multifuncional", Precio = 8000.00m },
                new Producto { Id = 6, Nombre = "Silla Ergonómica", Precio = 3500.00m }
            );

            // 2 Órdenes de compra de ejemplo
            modelBuilder.Entity<OrdenCompra>().HasData(
                new OrdenCompra 
                { 
                    Id = 1, 
                    Cliente = "TechSolutions S.A.", 
                    FechaCreacion = new DateTime(2025, 10, 1, 10, 30, 0, DateTimeKind.Utc),
                    Total = 16700.00m
                },
                new OrdenCompra 
                { 
                    Id = 2, 
                    Cliente = "Oficinas Corporativas Voultech", 
                    FechaCreacion = new DateTime(2025, 10, 2, 14, 15, 0, DateTimeKind.Utc),
                    Total = 16000.00m
                }
            );

            // Asociaciones OrdenProducto
            modelBuilder.Entity<OrdenProducto>().HasData(
                // Orden 1: TechSolutions S.A. - Laptop + Mouse + Teclado
                new OrdenProducto { Id = 1, OrdenId = 1, ProductoId = 1 }, // Laptop HP Pavilion
                new OrdenProducto { Id = 2, OrdenId = 1, ProductoId = 2 }, // Mouse Logitech  
                new OrdenProducto { Id = 3, OrdenId = 1, ProductoId = 3 }, // Teclado Mecánico

                // Orden 2: Oficinas Corporativas Mendoza - Monitor + Impresora + Silla
                new OrdenProducto { Id = 4, OrdenId = 2, ProductoId = 4 }, // Monitor 24 pulgadas
                new OrdenProducto { Id = 5, OrdenId = 2, ProductoId = 5 }, // Impresora Multifuncional
                new OrdenProducto { Id = 6, OrdenId = 2, ProductoId = 6 }  // Silla Ergonómica
            );
        }
    }
}