using System.ComponentModel.DataAnnotations;

namespace KhadiStore.Domain.Entities
{
    public abstract class BaseEntity
    {
        [Key]
        public int Id { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(100)]
        public string CreatedBy { get; set; } = string.Empty;

        public DateTime? ModifiedAt { get; set; }

        [StringLength(100)]
        public string? ModifiedBy { get; set; }

        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }

        [StringLength(100)]
        public string? DeletedBy { get; set; }
    }
}