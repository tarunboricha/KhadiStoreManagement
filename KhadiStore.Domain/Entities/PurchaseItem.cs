using System.ComponentModel.DataAnnotations;

namespace KhadiStore.Domain.Entities
{
    public class PurchaseItem : BaseEntity
    {
        public int PurchaseId { get; set; }

        public int ProductId { get; set; }

        [Required]
        [StringLength(200)]
        public string ProductName { get; set; } = string.Empty;

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        public decimal UnitPrice { get; set; }

        public decimal GSTRate { get; set; } = 5.0m;

        public decimal GSTAmount { get; set; } = 0;

        [Required]
        public decimal TotalAmount { get; set; }

        // Navigation Properties
        public virtual Purchase Purchase { get; set; } = null!;
        public virtual Product Product { get; set; } = null!;

        // Computed Properties
        public decimal LineTotal => (UnitPrice * Quantity) + GSTAmount;
    }
}