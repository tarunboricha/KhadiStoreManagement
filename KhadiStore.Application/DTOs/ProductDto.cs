using System.ComponentModel.DataAnnotations;

namespace KhadiStore.Application.DTOs
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public int MinStockLevel { get; set; }
        public string? SKU { get; set; }
        public string? FabricType { get; set; }
        public string? Color { get; set; }
        public string? Size { get; set; }
        public string? Pattern { get; set; }
        public decimal? GST { get; set; }
        public bool IsActive { get; set; }
        public string? ImagePath { get; set; }
        public decimal? Weight { get; set; }
        public string? Origin { get; set; }
        public bool IsLowStock { get; set; }
        public decimal PriceWithGST { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class CreateProductDto
    {
        [Required(ErrorMessage = "Product name is required")]
        [StringLength(200, ErrorMessage = "Product name cannot exceed 200 characters")]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Category is required")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Stock quantity is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Stock quantity cannot be negative")]
        public int StockQuantity { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Minimum stock level cannot be negative")]
        public int MinStockLevel { get; set; } = 10;

        [StringLength(50, ErrorMessage = "SKU cannot exceed 50 characters")]
        public string? SKU { get; set; }

        [StringLength(100, ErrorMessage = "Fabric type cannot exceed 100 characters")]
        public string? FabricType { get; set; }

        [StringLength(50, ErrorMessage = "Color cannot exceed 50 characters")]
        public string? Color { get; set; }

        [StringLength(20, ErrorMessage = "Size cannot exceed 20 characters")]
        public string? Size { get; set; }

        [StringLength(100, ErrorMessage = "Pattern cannot exceed 100 characters")]
        public string? Pattern { get; set; }

        [Range(0, 100, ErrorMessage = "GST must be between 0 and 100")]
        public decimal? GST { get; set; } = 5.0m;

        [StringLength(500, ErrorMessage = "Image path cannot exceed 500 characters")]
        public string? ImagePath { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Weight cannot be negative")]
        public decimal? Weight { get; set; }

        [StringLength(50, ErrorMessage = "Origin cannot exceed 50 characters")]
        public string? Origin { get; set; }
    }

    public class UpdateProductDto : CreateProductDto
    {
        public bool IsActive { get; set; } = true;
    }
}