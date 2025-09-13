using KhadiStore.Domain.Entities;

namespace KhadiStore.Domain.Interfaces
{
    public interface ICustomerRepository : IRepository<Customer>
    {
        Task<Customer?> GetByPhoneAsync(string phone);
        Task<Customer?> GetByEmailAsync(string email);
        Task<IEnumerable<Customer>> GetByTypeAsync(CustomerType customerType);
        Task<IEnumerable<Customer>> GetActiveCustomersAsync();
        Task<decimal> GetCustomerTotalPurchasesAsync(int customerId);
    }
}