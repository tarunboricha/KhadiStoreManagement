using KhadiStore.Application.DTOs;

namespace KhadiStore.Application.Services
{
    public interface ICustomerService
    {
        Task<IEnumerable<CustomerDto>> GetAllCustomersAsync();
        Task<CustomerDto?> GetCustomerByIdAsync(int id);
        Task<CustomerDto?> GetCustomerByPhoneAsync(string phone);
        Task<IEnumerable<CustomerDto>> GetActiveCustomersAsync();
        Task<CustomerDto> CreateCustomerAsync(CreateCustomerDto createCustomerDto);
        Task<CustomerDto?> UpdateCustomerAsync(int id, CreateCustomerDto updateCustomerDto);
        Task<bool> DeleteCustomerAsync(int id);
        Task<IEnumerable<CustomerDto>> GetPagedCustomersAsync(int pageIndex, int pageSize);
        Task<CustomerDto> GetCustomerByMobileAsync(string mobile);
        Task UpdateCustomerStatisticsAsync(int customerId, decimal saleAmount);
        Task<CustomerStatisticsDto> GetCustomerStatisticsAsync(int customerId);
    }
}