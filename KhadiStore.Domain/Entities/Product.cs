using System.ComponentModel.DataAnnotations;

namespace KhadiStore.Domain.Entities
{
    public class Product : BaseEntity
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }

        public int MinStockLevel { get; set; } = 10;

        [StringLength(50)]
        public string? SKU { get; set; }

        [StringLength(100)]
        public string? FabricType { get; set; } // Khadi, Cotton, Silk, etc.

        [StringLength(50)]
        public string? Color { get; set; }

        [StringLength(20)]
        public string? Size { get; set; } // S, M, L, XL, XXL or measurements

        [StringLength(100)]
        public string? Pattern { get; set; } // Solid, Striped, Printed, etc.

        public decimal? GST { get; set; } = 5.0m; // GST percentage

        public bool IsActive { get; set; } = true;

        [StringLength(500)]
        public string? ImagePath { get; set; }

        public decimal? Weight { get; set; } // in grams

        [StringLength(50)]
        public string? Origin { get; set; } // State/Region of origin

        // Navigation Properties
        public virtual Category Category { get; set; } = null!;
        public virtual ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
        public virtual ICollection<PurchaseItem> PurchaseItems { get; set; } = new List<PurchaseItem>();

        // Computed Properties
        public bool IsLowStock => StockQuantity <= MinStockLevel;
        public decimal PriceWithGST => Price + (Price * (GST ?? 0) / 100);
        public string DisplayName => $"{Name} - {FabricType} ({Color})".Trim();
    }
}