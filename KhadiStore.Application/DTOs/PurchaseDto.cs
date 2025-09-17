using System.ComponentModel.DataAnnotations;

namespace KhadiStore.Application.DTOs
{
    // Purchase DTOs
    public class PurchaseDto
    {
        public int Id { get; set; }
        public string PurchaseOrderNumber { get; set; } = string.Empty;
        public int SupplierId { get; set; }
        public string? SupplierName { get; set; }
        public string? SupplierPhone { get; set; }
        public string? SupplierEmail { get; set; }
        public DateTime PurchaseDate { get; set; }
        public decimal SubTotal { get; set; }
        public decimal GSTAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int TotalItems { get; set; }
        public List<PurchaseItemDto> PurchaseItems { get; set; } = new List<PurchaseItemDto>();

        // Helper properties
        public bool CanReceive => Status == "Ordered";
        public bool CanCancel => Status == "Ordered";
        public bool IsReceived => Status == "Received";
        public List<string> AvailableStatuses => GetAvailableStatuses();

        private List<string> GetAvailableStatuses()
        {
            return Status switch
            {
                "Ordered" => new List<string> { "Ordered", "Received", "Cancelled" },
                "Received" => new List<string> { "Received" },
                "Cancelled" => new List<string> { "Cancelled", "Ordered" },
                _ => new List<string> { "Ordered", "Received", "Cancelled" }
            };
        }
    }

    public class CreatePurchaseDto
    {
        [Required(ErrorMessage = "Supplier is required")]
        public int SupplierId { get; set; }

        public DateTime PurchaseDate { get; set; } = DateTime.Now;

        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string? Notes { get; set; }

        [Required(ErrorMessage = "At least one item is required")]
        [MinLength(1, ErrorMessage = "At least one item is required")]
        public List<CreatePurchaseItemDto> PurchaseItems { get; set; } = new List<CreatePurchaseItemDto>();
    }

    public class UpdatePurchaseStatusDto
    {
        [Required]
        public int PurchaseId { get; set; }

        [Required]
        public string Status { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Reason { get; set; }
    }

    // Purchase Item DTOs
    public class PurchaseItemDto
    {
        public int Id { get; set; }
        public int PurchaseId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal GSTRate { get; set; }
        public decimal GSTAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal LineTotal { get; set; }
    }

    public class CreatePurchaseItemDto
    {
        [Required(ErrorMessage = "Product is required")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Unit price is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Unit price must be greater than 0")]
        public decimal UnitPrice { get; set; }
    }

    // Pagination and Filtering DTOs
    public class PurchaseFilterDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? SupplierId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? PurchaseOrderNumber { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
