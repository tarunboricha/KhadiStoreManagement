using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KhadiStore.Domain.Entities
{
    public class Return : BaseEntity
    {
        [Required]
        [StringLength(50)]
        public string ReturnNumber { get; set; } = string.Empty;

        [ForeignKey("Sale")]
        public int SaleId { get; set; }
        public virtual Sale Sale { get; set; } = null!;

        public int? CustomerId { get; set; }
        public virtual Customer? Customer { get; set; }

        [Required]
        public DateTime ReturnDate { get; set; }

        [Required]
        [StringLength(500)]
        public string ReturnReason { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal SubTotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal GSTAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; } // Bill-level discount portion

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; } // Total refund amount

        [Required]
        [StringLength(50)]
        public string RefundMethod { get; set; } = "Cash";

        [StringLength(100)]
        public string RefundReference { get; set; } = string.Empty;

        [StringLength(1000)]
        public string AdditionalNotes { get; set; } = string.Empty;

        [StringLength(20)]
        public string Status { get; set; } = "Completed";

        public virtual ICollection<ReturnItem> ReturnItems { get; set; } = new List<ReturnItem>();
    }

    public class ReturnItem : BaseEntity
    {
        [ForeignKey("Return")]
        public int ReturnId { get; set; }
        public virtual Return Return { get; set; } = null!;

        [ForeignKey("Product")]
        public int ProductId { get; set; }
        public virtual Product Product { get; set; } = null!;

        [ForeignKey("SaleItem")]
        public int SaleItemId { get; set; }
        public virtual SaleItem SaleItem { get; set; } = null!;

        [Required]
        [StringLength(200)]
        public string ProductName { get; set; } = string.Empty;

        [Required]
        public int ReturnQuantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; } // Proportional bill-level discount

        [Column(TypeName = "decimal(5,2)")]
        public decimal GSTRate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal GSTAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal LineTotal { get; set; }
    }

    // Simplified RefundMethod enum (keep as is)
    public enum RefundMethod
    {
        Cash = 1,
        Card = 2,
        UPI = 3,
        BankTransfer = 4,
        StoreCredit = 5,
        Original = 6
    }
}