using System.ComponentModel.DataAnnotations;
using KhadiStore.Domain.Entities;

namespace KhadiStore.Application.DTOs
{
    public class CustomerDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PinCode { get; set; }
        public string? GSTNumber { get; set; }
        public string CustomerType { get; set; } = string.Empty;
        public decimal TotalPurchases { get; set; }
        public int TotalOrders { get; set; }
        public bool IsActive { get; set; }
        public bool IsWholesale { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateCustomerDto
    {
        [Required(ErrorMessage = "Customer name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [StringLength(15, ErrorMessage = "Phone cannot exceed 15 characters")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        public string? Phone { get; set; }

        [StringLength(200, ErrorMessage = "Email cannot exceed 200 characters")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string? Email { get; set; }

        [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
        public string? Address { get; set; }

        [StringLength(100, ErrorMessage = "City cannot exceed 100 characters")]
        public string? City { get; set; }

        [StringLength(100, ErrorMessage = "State cannot exceed 100 characters")]
        public string? State { get; set; }

        [StringLength(10, ErrorMessage = "Pin code cannot exceed 10 characters")]
        public string? PinCode { get; set; }

        [StringLength(15, ErrorMessage = "GST number cannot exceed 15 characters")]
        public string? GSTNumber { get; set; }

        public CustomerType CustomerType { get; set; } = CustomerType.Retail;
        public decimal TotalPurchases { get; set; }
        public int TotalOrders { get; set; }
    }

    public class CustomerStatisticsDto
    {
        public int CustomerId { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalPurchases { get; set; }
        public DateTime? LastPurchaseDate { get; set; }
        public DateTime? FirstPurchaseDate { get; set; }
        public decimal AverageOrderValue { get; set; }
    }
}