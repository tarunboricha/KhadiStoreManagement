using System.ComponentModel.DataAnnotations;

namespace KhadiStore.Application.DTOs
{
    public class ReturnDto
    {
        public int Id { get; set; }
        public string ReturnNumber { get; set; } = string.Empty;
        public int SaleId { get; set; }
        public SaleDto? Sale { get; set; }
        public int? CustomerId { get; set; }
        public CustomerDto? Customer { get; set; }
        public DateTime ReturnDate { get; set; }
        public string ReturnReason { get; set; } = string.Empty;
        public decimal SubTotal { get; set; }
        public decimal GSTAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string RefundMethod { get; set; } = string.Empty;
        public string RefundReference { get; set; } = string.Empty;
        public string AdditionalNotes { get; set; } = string.Empty;
        public string Status { get; set; } = "Completed";
        public DateTime CreatedAt { get; set; }
        public List<ReturnItemDto> ReturnItems { get; set; } = new List<ReturnItemDto>();
    }

    // Keep CreateReturnDto as is - no changes needed
    public class CreateReturnDto
    {
        public int SaleId { get; set; }

        [Required(ErrorMessage = "Return reason is required")]
        [StringLength(500, ErrorMessage = "Return reason cannot exceed 500 characters")]
        public string ReturnReason { get; set; } = string.Empty;

        [Required(ErrorMessage = "Refund method is required")]
        public string RefundMethod { get; set; } = "Cash";

        [StringLength(100, ErrorMessage = "Refund reference cannot exceed 100 characters")]
        public string? RefundReference { get; set; }

        [StringLength(1000, ErrorMessage = "Additional notes cannot exceed 1000 characters")]
        public string? AdditionalNotes { get; set; }

        public List<CreateReturnItemDto> ReturnItems { get; set; } = new List<CreateReturnItemDto>();
    }

    // Keep ReturnItemDto as is - no changes needed
    public class ReturnItemDto
    {
        public int Id { get; set; }
        public int ReturnId { get; set; }
        public int ProductId { get; set; }
        public int SaleItemId { get; set; }
        public string ProductName { get; set; } = string.Empty;
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
    }

    // Updated summary DTO - simplified for immediate processing
    public class ReturnSummaryDto
    {
        public int TotalReturns { get; set; }
        public decimal TotalReturnAmount { get; set; }
        public int PendingReturns { get; set; }
        public int CompletedReturns { get; set; }
        public decimal AverageReturnValue { get; set; }
        public Dictionary<string, int> ReturnReasonBreakdown { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, decimal> RefundMethodBreakdown { get; set; } = new Dictionary<string, decimal>();
    }
}