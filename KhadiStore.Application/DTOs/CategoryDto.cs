using System.ComponentModel.DataAnnotations;

namespace KhadiStore.Application.DTOs
{
    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public string? ImagePath { get; set; }
        public int ProductCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateCategoryDto
    {
        [Required(ErrorMessage = "Category name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        [StringLength(50, ErrorMessage = "Image path cannot exceed 50 characters")]
        public string? ImagePath { get; set; }
    }
}