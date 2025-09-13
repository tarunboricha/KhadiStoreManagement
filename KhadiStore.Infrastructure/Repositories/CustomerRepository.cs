using Microsoft.EntityFrameworkCore;
using KhadiStore.Domain.Entities;
using KhadiStore.Domain.Interfaces;
using KhadiStore.Infrastructure.Data;

namespace KhadiStore.Infrastructure.Repositories
{
    public class CustomerRepository : Repository<Customer>, ICustomerRepository
    {
        public CustomerRepository(KhadiStoreDbContext context) : base(context)
        {
        }

        public async Task<Customer?> GetByPhoneAsync(string phone)
        {
            return await _dbSet
                .Where(c => !c.IsDeleted && c.Phone == phone)
                .FirstOrDefaultAsync();
        }

        public async Task<Customer?> GetByEmailAsync(string email)
        {
            return await _dbSet
                .Where(c => !c.IsDeleted && c.Email == email)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Customer>> GetByTypeAsync(CustomerType customerType)
        {
            return await _dbSet
                .Where(c => !c.IsDeleted && c.CustomerType == customerType)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Customer>> GetActiveCustomersAsync()
        {
            return await _dbSet
                .Where(c => !c.IsDeleted && c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<decimal> GetCustomerTotalPurchasesAsync(int customerId)
        {
            var customer = await GetByIdAsync(customerId);
            return customer?.TotalPurchases ?? 0;
        }
    }
}