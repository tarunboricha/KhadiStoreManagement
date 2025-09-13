using System.ComponentModel.DataAnnotations;

namespace KhadiStore.Domain.Entities
{
    public class Customer : BaseEntity
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(15)]
        [Phone]
        public string? Phone { get; set; }

        [StringLength(200)]
        [EmailAddress]
        public string? Email { get; set; }

        [StringLength(500)]
        public string? Address { get; set; }

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(100)]
        public string? State { get; set; }

        [StringLength(10)]
        public string? PinCode { get; set; }

        [StringLength(15)]
        public string? GSTNumber { get; set; }

        public CustomerType CustomerType { get; set; } = CustomerType.Retail;

        public decimal TotalPurchases { get; set; } = 0;

        public int TotalOrders { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();

        // Computed Properties
        public bool IsWholesale => CustomerType == CustomerType.Wholesale;
    }

    public enum CustomerType
    {
        Retail = 1,
        Wholesale = 2,
        Corporate = 3
    }
}