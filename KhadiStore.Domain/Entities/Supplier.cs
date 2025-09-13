using System.ComponentModel.DataAnnotations;

namespace KhadiStore.Domain.Entities
{
    public class Supplier : BaseEntity
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(100)]
        public string? ContactPerson { get; set; }

        [StringLength(15)]
        [Phone]
        public string? Phone { get; set; }

        [StringLength(200)]
        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        [StringLength(500)]
        public string Address { get; set; } = string.Empty;

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(100)]
        public string? State { get; set; }

        [StringLength(10)]
        public string? PinCode { get; set; }

        [StringLength(15)]
        public string? GSTNumber { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public virtual ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();
    }
}