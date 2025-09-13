using Microsoft.EntityFrameworkCore;
using KhadiStore.Domain.Entities;
using KhadiStore.Domain.Interfaces;
using KhadiStore.Infrastructure.Data;

namespace KhadiStore.Infrastructure.Repositories
{
    public class SupplierRepository : Repository<Supplier>, ISupplierRepository
    {
        public SupplierRepository(KhadiStoreDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Supplier>> GetActiveSuppliersAsync()
        {
            return await _dbSet
                .Where(s => !s.IsDeleted && s.IsActive)
                .OrderBy(s => s.Name)
                .ToListAsync();
        }

        public async Task<Supplier?> GetByGSTNumberAsync(string gstNumber)
        {
            return await _dbSet
                .Where(s => !s.IsDeleted && s.GSTNumber == gstNumber)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Supplier>> GetByStateAsync(string state)
        {
            return await _dbSet
                .Where(s => !s.IsDeleted && s.State == state)
                .OrderBy(s => s.Name)
                .ToListAsync();
        }
    }
}