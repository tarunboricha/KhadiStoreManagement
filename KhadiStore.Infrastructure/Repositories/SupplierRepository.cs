using Microsoft.EntityFrameworkCore;
using KhadiStore.Domain.Entities;
using KhadiStore.Domain.Interfaces;
using KhadiStore.Infrastructure.Data;
using KhadiStore.Application.Interfaces;

namespace KhadiStore.Infrastructure.Repositories
{
    public class SupplierRepository : Repository<Supplier>, ISupplierRepository
    {
        public SupplierRepository(KhadiStoreDbContext context) : base(context)
        {
        }

        public override async Task<Supplier?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Where(s => !s.IsDeleted && s.Id == id)
                .Include(s => s.Purchases)
                .FirstOrDefaultAsync();
        }

        public override async Task<IEnumerable<Supplier>> GetAllAsync()
        {
            return await _dbSet
                .Where(s => !s.IsDeleted)
                .OrderBy(s => s.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Supplier>> GetActiveAsync()
        {
            return await _dbSet
                .Where(s => !s.IsDeleted && s.IsActive)
                .OrderBy(s => s.Name)
                .ToListAsync();
        }

        public async Task<Supplier?> GetByNameAsync(string name)
        {
            return await _dbSet
                .Where(s => !s.IsDeleted && s.Name.ToLower() == name.ToLower())
                .FirstOrDefaultAsync();
        }

        public async Task<Supplier?> GetByGSTNumberAsync(string gstNumber)
        {
            if (string.IsNullOrWhiteSpace(gstNumber))
                return null;

            return await _dbSet
                .Where(s => !s.IsDeleted && s.GSTNumber == gstNumber)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Supplier>> GetByLocationAsync(string city, string state)
        {
            var query = _dbSet.Where(s => !s.IsDeleted);

            if (!string.IsNullOrWhiteSpace(city))
                query = query.Where(s => s.City != null && s.City.ToLower().Contains(city.ToLower()));

            if (!string.IsNullOrWhiteSpace(state))
                query = query.Where(s => s.State != null && s.State.ToLower().Contains(state.ToLower()));

            return await query.OrderBy(s => s.Name).ToListAsync();
        }

        public async Task<bool> ExistsAsync(string name, int excludeId = 0)
        {
            return await _dbSet
                .Where(s => !s.IsDeleted && s.Name.ToLower() == name.ToLower() && s.Id != excludeId)
                .AnyAsync();
        }

        public async Task<IEnumerable<Supplier>> SearchAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetActiveAsync();

            var term = searchTerm.ToLower();

            return await _dbSet
                .Where(s => !s.IsDeleted && (
                    s.Name.ToLower().Contains(term) ||
                    (s.ContactPerson != null && s.ContactPerson.ToLower().Contains(term)) ||
                    (s.Phone != null && s.Phone.Contains(term)) ||
                    (s.Email != null && s.Email.ToLower().Contains(term)) ||
                    (s.City != null && s.City.ToLower().Contains(term)) ||
                    (s.GSTNumber != null && s.GSTNumber.ToLower().Contains(term))
                ))
                .OrderBy(s => s.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Supplier>> GetPagedAsync(int page, int pageSize, string? name = null, string? city = null, string? state = null, bool? isActive = null)
        {
            try
            {
                var query = BuildFilterQuery(name, city, state, isActive);

                return await query
                    .OrderBy(s => s.Name)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error in GetPagedAsync: {ex.Message}", ex);
            }
        }

        public async Task<int> GetCountAsync(string? name = null, string? city = null, string? state = null, bool? isActive = null)
        {
            try
            {
                var query = BuildFilterQuery(name, city, state, isActive);
                return await query.CountAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error in GetCountAsync: {ex.Message}", ex);
            }
        }

        public async Task<int> GetTotalSuppliersAsync()
        {
            return await _dbSet.Where(s => !s.IsDeleted).CountAsync();
        }

        public async Task<int> GetActiveSuppliersAsync()
        {
            return await _dbSet.Where(s => !s.IsDeleted && s.IsActive).CountAsync();
        }

        public async Task<IEnumerable<Supplier>> GetTopSuppliersByPurchaseAmountAsync(int count = 10)
        {
            return await _dbSet
                .Where(s => !s.IsDeleted && s.IsActive)
                .Include(s => s.Purchases.Where(p => !p.IsDeleted && p.Status == PurchaseStatus.Received))
                .OrderByDescending(s => s.Purchases.Sum(p => p.TotalAmount))
                .Take(count)
                .ToListAsync();
        }

        private IQueryable<Supplier> BuildFilterQuery(string? name, string? city, string? state, bool? isActive)
        {
            var query = _dbSet.Where(s => !s.IsDeleted);

            if (!string.IsNullOrWhiteSpace(name))
                query = query.Where(s => s.Name.ToLower().Contains(name.ToLower()));

            if (!string.IsNullOrWhiteSpace(city))
                query = query.Where(s => s.City != null && s.City.ToLower().Contains(city.ToLower()));

            if (!string.IsNullOrWhiteSpace(state))
                query = query.Where(s => s.State != null && s.State.ToLower().Contains(state.ToLower()));

            if (isActive.HasValue)
                query = query.Where(s => s.IsActive == isActive.Value);

            return query;
        }
    }
}