using KhadiStore.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace KhadiStore.Infrastructure.Data
{
    public class KhadiStoreDbContext : IdentityDbContext<IdentityUser>
    {
        public KhadiStoreDbContext(DbContextOptions<KhadiStoreDbContext> options) : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<SaleItem> SaleItems { get; set; }
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<PurchaseItem> PurchaseItems { get; set; }
        public DbSet<Return> Returns { get; set; }
        public DbSet<ReturnItem> ReturnItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Category Configuration
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.ImagePath).HasMaxLength(50);
                entity.HasIndex(e => e.Name).IsUnique();
            });

            // Product Configuration
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
                entity.Property(e => e.GST).HasColumnType("decimal(5,2)");
                entity.Property(e => e.Weight).HasColumnType("decimal(10,3)");
                entity.Property(e => e.SKU).HasMaxLength(50);
                entity.Property(e => e.FabricType).HasMaxLength(100);
                entity.Property(e => e.Color).HasMaxLength(50);
                entity.Property(e => e.Size).HasMaxLength(20);
                entity.Property(e => e.Pattern).HasMaxLength(100);
                entity.Property(e => e.ImagePath).HasMaxLength(500);
                entity.Property(e => e.Origin).HasMaxLength(50);

                entity.HasOne(d => d.Category)
                    .WithMany(p => p.Products)
                    .HasForeignKey(d => d.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.SKU).IsUnique();
            });

            // Customer Configuration
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Phone).HasMaxLength(15);
                entity.Property(e => e.Email).HasMaxLength(200);
                entity.Property(e => e.Address).HasMaxLength(500);
                entity.Property(e => e.City).HasMaxLength(100);
                entity.Property(e => e.State).HasMaxLength(100);
                entity.Property(e => e.PinCode).HasMaxLength(10);
                entity.Property(e => e.GSTNumber).HasMaxLength(15);
                entity.Property(e => e.TotalPurchases).HasColumnType("decimal(18,2)");
            });

            // Supplier Configuration
            modelBuilder.Entity<Supplier>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.ContactPerson).HasMaxLength(100);
                entity.Property(e => e.Phone).HasMaxLength(15);
                entity.Property(e => e.Email).HasMaxLength(200);
                entity.Property(e => e.Address).IsRequired().HasMaxLength(500);
                entity.Property(e => e.City).HasMaxLength(100);
                entity.Property(e => e.State).HasMaxLength(100);
                entity.Property(e => e.PinCode).HasMaxLength(10);
                entity.Property(e => e.GSTNumber).HasMaxLength(15);
            });

            // Sale Configuration
            modelBuilder.Entity<Sale>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.InvoiceNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.CustomerName).HasMaxLength(100);
                entity.Property(e => e.CustomerPhone).HasMaxLength(15);
                entity.Property(e => e.SubTotal).HasColumnType("decimal(18,2)");
                entity.Property(e => e.DiscountAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.GSTAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.PaymentReference).HasMaxLength(100);
                entity.Property(e => e.Notes).HasMaxLength(500);

                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.Sales)
                    .HasForeignKey(d => d.CustomerId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(e => e.InvoiceNumber).IsUnique();
            });

            // SaleItem Configuration
            modelBuilder.Entity<SaleItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ProductName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.DiscountAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.GSTRate).HasColumnType("decimal(5,2)");
                entity.Property(e => e.GSTAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");

                entity.HasOne(d => d.Sale)
                    .WithMany(p => p.SaleItems)
                    .HasForeignKey(d => d.SaleId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.SaleItems)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Purchase Configuration
            modelBuilder.Entity<Purchase>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PurchaseOrderNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.SubTotal).HasColumnType("decimal(18,2)");
                entity.Property(e => e.GSTAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Notes).HasMaxLength(500);

                entity.HasOne(d => d.Supplier)
                    .WithMany(p => p.Purchases)
                    .HasForeignKey(d => d.SupplierId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.PurchaseOrderNumber).IsUnique();
            });

            // PurchaseItem Configuration
            modelBuilder.Entity<PurchaseItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ProductName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.GSTRate).HasColumnType("decimal(5,2)");
                entity.Property(e => e.GSTAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");

                entity.HasOne(d => d.Purchase)
                    .WithMany(p => p.PurchaseItems)
                    .HasForeignKey(d => d.PurchaseId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.PurchaseItems)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Return entity configuration
            modelBuilder.Entity<Return>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.Property(r => r.ReturnNumber).IsRequired().HasMaxLength(50);
                entity.HasIndex(r => r.ReturnNumber).IsUnique();

                entity.HasOne(r => r.Sale)
                    .WithMany()
                    .HasForeignKey(r => r.SaleId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.Customer)
                    .WithMany()
                    .HasForeignKey(r => r.CustomerId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // ReturnItem entity configuration
            modelBuilder.Entity<ReturnItem>(entity =>
            {
                entity.HasKey(ri => ri.Id);

                entity.HasOne(ri => ri.Return)
                    .WithMany(r => r.ReturnItems)
                    .HasForeignKey(ri => ri.ReturnId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ri => ri.Product)
                    .WithMany()
                    .HasForeignKey(ri => ri.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(ri => ri.SaleItem)
                    .WithMany()
                    .HasForeignKey(ri => ri.SaleItemId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Seed Data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Categories
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Khadi Cotton", Description = "Pure Khadi cotton fabrics", IsActive = true, CreatedAt = DateTime.UtcNow, CreatedBy = "System" },
                new Category { Id = 2, Name = "Silk Sarees", Description = "Traditional silk sarees", IsActive = true, CreatedAt = DateTime.UtcNow, CreatedBy = "System" },
                new Category { Id = 3, Name = "Men's Kurtas", Description = "Traditional men's kurtas", IsActive = true, CreatedAt = DateTime.UtcNow, CreatedBy = "System" },
                new Category { Id = 4, Name = "Women's Kurtis", Description = "Designer kurtis for women", IsActive = true, CreatedAt = DateTime.UtcNow, CreatedBy = "System" },
                new Category { Id = 5, Name = "Handloom", Description = "Handwoven traditional fabrics", IsActive = true, CreatedAt = DateTime.UtcNow, CreatedBy = "System" }
            );

            // Seed Products
            modelBuilder.Entity<Product>().HasData(
                new Product { Id = 1, Name = "White Khadi Kurta", Description = "Pure white khadi cotton kurta for men", CategoryId = 3, Price = 1500, StockQuantity = 50, MinStockLevel = 10, SKU = "KH001", FabricType = "Khadi Cotton", Color = "White", Size = "L", Pattern = "Solid", GST = 5.0m, IsActive = true, Origin = "Gujarat", CreatedAt = DateTime.UtcNow, CreatedBy = "System" },
                new Product { Id = 2, Name = "Red Silk Saree", Description = "Traditional red silk saree with gold border", CategoryId = 2, Price = 8500, StockQuantity = 25, MinStockLevel = 5, SKU = "SL001", FabricType = "Silk", Color = "Red", Pattern = "Border", GST = 5.0m, IsActive = true, Origin = "Karnataka", CreatedAt = DateTime.UtcNow, CreatedBy = "System" },
                new Product { Id = 3, Name = "Blue Cotton Kurti", Description = "Casual blue cotton kurti with prints", CategoryId = 4, Price = 850, StockQuantity = 75, MinStockLevel = 15, SKU = "CT001", FabricType = "Cotton", Color = "Blue", Size = "M", Pattern = "Printed", GST = 5.0m, IsActive = true, Origin = "Rajasthan", CreatedAt = DateTime.UtcNow, CreatedBy = "System" },
                new Product { Id = 4, Name = "Handloom Dhoti", Description = "Traditional handloom white dhoti", CategoryId = 5, Price = 1200, StockQuantity = 30, MinStockLevel = 8, SKU = "HL001", FabricType = "Handloom Cotton", Color = "White", Pattern = "Solid", GST = 0.0m, IsActive = true, Origin = "Tamil Nadu", CreatedAt = DateTime.UtcNow, CreatedBy = "System" },
                new Product { Id = 5, Name = "Green Khadi Dupatta", Description = "Light green khadi dupatta with tassels", CategoryId = 1, Price = 650, StockQuantity = 40, MinStockLevel = 10, SKU = "KH002", FabricType = "Khadi Cotton", Color = "Green", Pattern = "Plain", GST = 5.0m, IsActive = true, Origin = "West Bengal", CreatedAt = DateTime.UtcNow, CreatedBy = "System" }
            );

            // Seed Customers
            modelBuilder.Entity<Customer>().HasData(
                new Customer { Id = 1, Name = "Rajesh Kumar", Phone = "9876543210", Email = "rajesh@example.com", Address = "123, MG Road", City = "Mumbai", State = "Maharashtra", PinCode = "400001", CustomerType = CustomerType.Retail, IsActive = true, CreatedAt = DateTime.UtcNow, CreatedBy = "System" },
                new Customer { Id = 2, Name = "Priya Sharma", Phone = "9876543211", Email = "priya@example.com", Address = "456, Park Street", City = "Kolkata", State = "West Bengal", PinCode = "700001", CustomerType = CustomerType.Wholesale, IsActive = true, CreatedAt = DateTime.UtcNow, CreatedBy = "System" }
            );

            // Seed Suppliers
            modelBuilder.Entity<Supplier>().HasData(
                new Supplier { Id = 1, Name = "Gujarat Khadi Bhandar", ContactPerson = "Mohanlal Patel", Phone = "9876543220", Email = "gujarat@khadi.com", Address = "Khadi Gram, Sabarmati", City = "Ahmedabad", State = "Gujarat", PinCode = "380005", GSTNumber = "24ABCDE1234F1Z5", IsActive = true, CreatedAt = DateTime.UtcNow, CreatedBy = "System" },
                new Supplier { Id = 2, Name = "Karnataka Silk House", ContactPerson = "Ravi Kumar", Phone = "9876543221", Email = "karnataka@silk.com", Address = "Silk Market, Commercial Street", City = "Bangalore", State = "Karnataka", PinCode = "560001", GSTNumber = "29FGHIJ5678K2A6", IsActive = true, CreatedAt = DateTime.UtcNow, CreatedBy = "System" }
            );
        }
    }
}