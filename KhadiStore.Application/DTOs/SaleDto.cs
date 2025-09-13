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
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string? PaymentReference { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public int TotalItems { get; set; }
        public List<SaleItemDto> SaleItems { get; set; } = new List<SaleItemDto>();
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
    }

    public class CreateSaleItemDto
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int Quantity { get; set; }
    }
}