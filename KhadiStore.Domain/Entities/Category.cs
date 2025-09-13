using System.ComponentModel.DataAnnotations;

namespace KhadiStore.Domain.Entities
{
    public class Category : BaseEntity
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        [StringLength(50)]
        public string? ImagePath { get; set; }

        // Navigation Properties
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}