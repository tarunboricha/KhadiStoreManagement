// Supplier DTOs
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
        public DateTime? UpdatedAt { get; set; }

        // Computed properties
        public string FullAddress => $"{Address}, {City}, {State} - {PinCode}".Trim(' ', ',', '-');
        public int TotalPurchases { get; set; }
        public decimal TotalPurchaseAmount { get; set; }
    }

    public class CreateSupplierDto
    {
        [Required(ErrorMessage = "Supplier name is required")]
        [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
        public string Name { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Contact person name cannot exceed 100 characters")]
        public string? ContactPerson { get; set; }

        [StringLength(15, ErrorMessage = "Phone number cannot exceed 15 characters")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        public string? Phone { get; set; }

        [StringLength(200, ErrorMessage = "Email cannot exceed 200 characters")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Address is required")]
        [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
        public string Address { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "City name cannot exceed 100 characters")]
        public string? City { get; set; }

        [StringLength(100, ErrorMessage = "State name cannot exceed 100 characters")]
        public string? State { get; set; }

        [StringLength(10, ErrorMessage = "Pin code cannot exceed 10 characters")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "Pin code must be 6 digits")]
        public string? PinCode { get; set; }

        [StringLength(15, ErrorMessage = "GST number cannot exceed 15 characters")]
        [RegularExpression(@"^[0-9]{2}[A-Z]{5}[0-9]{4}[A-Z]{1}[1-9A-Z]{1}Z[0-9A-Z]{1}$",
            ErrorMessage = "Invalid GST number format")]
        public string? GSTNumber { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateSupplierDto : CreateSupplierDto
    {
        public int Id { get; set; }
    }

    public class SupplierFilterDto
    {
        public string? Name { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public bool? IsActive { get; set; } = true;
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}