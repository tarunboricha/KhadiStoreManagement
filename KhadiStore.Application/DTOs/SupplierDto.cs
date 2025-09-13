using System.ComponentModel.DataAnnotations;

namespace KhadiStore.Application.DTOs
{
    public class SupplierDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ContactPerson { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string Address { get; set; } = string.Empty;
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PinCode { get; set; }
        public string? GSTNumber { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateSupplierDto
    {
        [Required(ErrorMessage = "Supplier name is required")]
        [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
        public string Name { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Contact person cannot exceed 100 characters")]
        public string? ContactPerson { get; set; }

        [StringLength(15, ErrorMessage = "Phone cannot exceed 15 characters")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        public string? Phone { get; set; }

        [StringLength(200, ErrorMessage = "Email cannot exceed 200 characters")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Address is required")]
        [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
        public string Address { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "City cannot exceed 100 characters")]
        public string? City { get; set; }

        [StringLength(100, ErrorMessage = "State cannot exceed 100 characters")]
        public string? State { get; set; }

        [StringLength(10, ErrorMessage = "Pin code cannot exceed 10 characters")]
        public string? PinCode { get; set; }

        [StringLength(15, ErrorMessage = "GST number cannot exceed 15 characters")]
        public string? GSTNumber { get; set; }
    }
}