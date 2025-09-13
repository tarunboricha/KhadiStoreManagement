using System.ComponentModel.DataAnnotations;

namespace KhadiStore.Application.DTOs
{
    public class ReturnDto
    {
        public int Id { get; set; }
        public string ReturnNumber { get; set; } = string.Empty;
        public int SaleId { get; set; }
        public string SaleInvoiceNumber { get; set; } = string.Empty;
        public int? CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public DateTime ReturnDate { get; set; }
        public string ReturnReason { get; set; } = string.Empty;
        public decimal SubTotal { get; set; }
        public decimal GSTAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string RefundMethod { get; set; } = string.Empty;
        public string RefundReference { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public int TotalItems { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsProcessed { get; set; } = true; // Always true in simplified system
        public List<ReturnItemDto> ReturnItems { get; set; } = new();
    }

    // Keep CreateReturnDto as is - no changes needed
    public class CreateReturnDto
    {
        [Required]
        public int SaleId { get; set; }

        [Required]
        [StringLength(500, ErrorMessage = "Return reason cannot exceed 500 characters")]
        public string ReturnReason { get; set; } = string.Empty;

        [Required]
        public string RefundMethod { get; set; } = "Cash";

        [StringLength(100)]
        public string RefundReference { get; set; } = string.Empty;

        [StringLength(1000)]
        public string Notes { get; set; } = string.Empty;

        [Required]
        [MinLength(1, ErrorMessage = "At least one item must be selected for return")]
        public List<CreateReturnItemDto> ReturnItems { get; set; } = new();
    }

    // Keep ReturnItemDto as is - no changes needed
    public class ReturnItemDto
    {
        public int Id { get; set; }
        public int ReturnId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int SaleItemId { get; set; }
        public int OriginalQuantity { get; set; }
        public int ReturnQuantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal GSTRate { get; set; }
        public decimal GSTAmount { get; set; }
        public decimal LineTotal { get; set; }
    }

    // Keep CreateReturnItemDto as is - no changes needed
    public class CreateReturnItemDto
    {
        [Required]
        public int SaleItemId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Return quantity must be at least 1")]
        public int ReturnQuantity { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Discount amount cannot be negative")]
        public decimal DiscountAmount { get; set; }
    }

    // Updated summary DTO - simplified for immediate processing
    public class ReturnSummaryDto
    {
        public int TotalReturns { get; set; }
        public decimal TotalReturnAmount { get; set; }
        public int PendingReturns { get; set; } = 0; // Always 0 in simplified system
        public int CompletedReturns { get; set; } // Same as TotalReturns
        public decimal AverageReturnValue { get; set; }
        public Dictionary<string, int> ReturnReasonBreakdown { get; set; } = new();
        public Dictionary<string, decimal> RefundMethodBreakdown { get; set; } = new();
    }
}