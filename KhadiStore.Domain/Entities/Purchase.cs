using System.ComponentModel.DataAnnotations;

namespace KhadiStore.Domain.Entities
{
    public class Purchase : BaseEntity
    {
        [Required]
        [StringLength(50)]
        public string PurchaseOrderNumber { get; set; } = string.Empty;

        [Required]
        public int SupplierId { get; set; }

        public DateTime PurchaseDate { get; set; } = DateTime.Now;

        [Required]
        public decimal SubTotal { get; set; }

        public decimal GSTAmount { get; set; } = 0;

        [Required]
        public decimal TotalAmount { get; set; }

        public PurchaseStatus Status { get; set; } = PurchaseStatus.Ordered;

        [StringLength(500)]
        public string? Notes { get; set; }

        // Navigation Properties
        public virtual Supplier Supplier { get; set; } = null!;
        public virtual ICollection<PurchaseItem> PurchaseItems { get; set; } = new List<PurchaseItem>();

        // Computed Properties
        public int TotalItems => PurchaseItems.Sum(pi => pi.Quantity);
    }

    public enum PurchaseStatus
    {
        Ordered = 1,
        Received = 2,
        Cancelled = 3
    }
}