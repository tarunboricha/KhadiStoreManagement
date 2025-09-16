using System.ComponentModel.DataAnnotations;
using KhadiStore.Domain.Entities;

namespace KhadiStore.Application.DTOs
{
    public class SaleDto
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public int? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public DateTime SaleDate { get; set; }
        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal GSTAmount { get; set; }

        // NEW: Rounding fields
        public decimal RoundingAmount { get; set; } = 0;
        public decimal CalculatedTotal { get; set; } // Total before rounding

        public decimal TotalAmount { get; set; } // Final amount after rounding
        public string PaymentMethod { get; set; } = string.Empty;
        public string? PaymentReference { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public int TotalItems { get; set; }
        public List<SaleItemDto> SaleItems { get; set; } = new List<SaleItemDto>();

        // Helper properties
        public bool HasRounding => RoundingAmount != 0;
        public bool CanChangeStatus => Status != "Returned";
        public List<string> AvailableStatuses => GetAvailableStatuses();

        private List<string> GetAvailableStatuses()
        {
            return Status switch
            {
                "Pending" => new List<string> { "Pending", "Completed", "Cancelled" },
                "Completed" => new List<string> { "Completed", "Cancelled" },
                "Cancelled" => new List<string> { "Cancelled", "Pending" },
                _ => new List<string> { "Pending", "Completed", "Cancelled" }
            };
        }
    }

    public class SaleItemDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal GSTRate { get; set; }
        public decimal GSTAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal LineTotal { get; set; }
    }

    public class SalesFilterDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }
        public string Status { get; set; } = string.Empty;
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? CustomerName { get; set; }
        public string? InvoiceNumber { get; set; }
    }
    public class CreateSaleDto
    {
        public int? CustomerId { get; set; }

        [StringLength(100)]
        public string? CustomerName { get; set; }

        [StringLength(15)]
        public string? CustomerPhone { get; set; }

        public DateTime SaleDate { get; set; } = DateTime.Now;

        public decimal DiscountAmount { get; set; } = 0;

        [Required]
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;

        [StringLength(100)]
        public string? PaymentReference { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "At least one item is required")]
        public List<CreateSaleItemDto> SaleItems { get; set; } = new List<CreateSaleItemDto>();

        public decimal BillDiscountPercentage { get; set; } = 0;

        // NEW: Rounding functionality
        public bool EnableRounding { get; set; } = true;
        public RoundingMethod RoundingMethod { get; set; } = RoundingMethod.NearestTen;
    }

    public class CreateSaleItemDto
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int Quantity { get; set; }
    }

    // NEW: Rounding method enum
    public enum RoundingMethod
    {
        None = 0,
        NearestFive = 5,
        NearestTen = 10,
        RoundDown = -1
    }
    public class UpdateSaleStatusDto
    {
        [Required]
        public int SaleId { get; set; }

        [Required]
        public string Status { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Reason { get; set; }
    }
}