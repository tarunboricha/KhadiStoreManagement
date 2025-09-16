// Updated Sale.cs entity - Add rounding fields
using System.ComponentModel.DataAnnotations;

namespace KhadiStore.Domain.Entities
{
    public class Sale : BaseEntity
    {
        [Required]
        [StringLength(50)]
        public string InvoiceNumber { get; set; } = string.Empty;

        public int? CustomerId { get; set; }

        [StringLength(100)]
        public string? CustomerName { get; set; }

        [StringLength(15)]
        public string? CustomerPhone { get; set; }

        public DateTime SaleDate { get; set; } = DateTime.Now;

        [Required]
        public decimal SubTotal { get; set; }

        public decimal DiscountAmount { get; set; } = 0;

        public decimal GSTAmount { get; set; } = 0;

        // NEW: Rounding fields
        public decimal RoundingAmount { get; set; } = 0;
        public decimal CalculatedTotal { get; set; } = 0; // Total before rounding

        [Required]
        public decimal TotalAmount { get; set; } // Final amount after rounding

        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;

        [StringLength(100)]
        public string? PaymentReference { get; set; }

        public SaleStatus Status { get; set; } = SaleStatus.Completed;

        [StringLength(500)]
        public string? Notes { get; set; }

        // Navigation Properties
        public virtual Customer? Customer { get; set; }
        public virtual ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();

        // Computed Properties
        public int TotalItems => SaleItems.Sum(si => si.Quantity);
        public bool HasRounding => RoundingAmount != 0;
    }

    public enum PaymentMethod
    {
        Cash = 1,
        Card = 2,
        UPI = 3,
        NetBanking = 4,
        Cheque = 5
    }

    public enum SaleStatus
    {
        Pending = 1,
        Completed = 2,
        Cancelled = 3,
        Returned = 4
    }
}