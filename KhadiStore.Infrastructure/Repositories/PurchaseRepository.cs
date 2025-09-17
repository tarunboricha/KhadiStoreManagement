using Microsoft.EntityFrameworkCore;
using KhadiStore.Domain.Entities;
using KhadiStore.Infrastructure.Data;
using KhadiStore.Application.Interfaces;

namespace KhadiStore.Infrastructure.Repositories
{
    public class PurchaseRepository : Repository<Purchase>, IPurchaseRepository
    {
        public PurchaseRepository(KhadiStoreDbContext context) : base(context)
        {
        }

        public override async Task<Purchase?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Where(p => !p.IsDeleted && p.Id == id)
                .Include(p => p.PurchaseItems)
                    .ThenInclude(pi => pi.Product)
                .Include(p => p.Supplier)
                .FirstOrDefaultAsync();
        }

        public override async Task<IEnumerable<Purchase>> GetAllAsync()
        {
            return await _dbSet
                .Where(p => !p.IsDeleted)
                .Include(p => p.PurchaseItems)
                .Include(p => p.Supplier)
                .OrderByDescending(p => p.PurchaseDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Purchase>> GetBySupplierAsync(int supplierId)
        {
            return await _dbSet
                .Where(p => !p.IsDeleted && p.SupplierId == supplierId)
                .Include(p => p.PurchaseItems)
                    .ThenInclude(pi => pi.Product)
                .OrderByDescending(p => p.PurchaseDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Purchase>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(p => !p.IsDeleted && p.PurchaseDate >= startDate && p.PurchaseDate < endDate)
                .Include(p => p.PurchaseItems)
                    .ThenInclude(pi => pi.Product)
                .Include(p => p.Supplier)
                .OrderByDescending(p => p.PurchaseDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Purchase>> GetByStatusAsync(PurchaseStatus status)
        {
            return await _dbSet
                .Where(p => !p.IsDeleted && p.Status == status)
                .Include(p => p.PurchaseItems)
                .Include(p => p.Supplier)
                .OrderByDescending(p => p.PurchaseDate)
                .ToListAsync();
        }

        public async Task<Purchase?> GetByPurchaseOrderNumberAsync(string purchaseOrderNumber)
        {
            return await _dbSet
                .Where(p => !p.IsDeleted && p.PurchaseOrderNumber == purchaseOrderNumber)
                .Include(p => p.PurchaseItems)
                    .ThenInclude(pi => pi.Product)
                .Include(p => p.Supplier)
                .FirstOrDefaultAsync();
        }

        public async Task<string> GeneratePurchaseOrderNumberAsync()
        {
            var today = DateTime.Today;
            var prefix = $"PO{today:yyyyMMdd}";
            
            var lastPurchase = await _dbSet
                .Where(p => p.PurchaseOrderNumber.StartsWith(prefix))
                .OrderByDescending(p => p.PurchaseOrderNumber)
                .FirstOrDefaultAsync();

            if (lastPurchase == null)
            {
                return $"{prefix}001";
            }

            var lastNumber = int.Parse(lastPurchase.PurchaseOrderNumber.Substring(prefix.Length));
            return $"{prefix}{(lastNumber + 1):D3}";
        }

        public async Task<decimal> GetTotalPurchaseAmountAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _dbSet.Where(p => !p.IsDeleted && p.Status == PurchaseStatus.Received);
            
            if (startDate.HasValue)
                query = query.Where(p => p.PurchaseDate >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(p => p.PurchaseDate < endDate.Value);
                
            return await query.SumAsync(p => p.TotalAmount);
        }

        public async Task<IEnumerable<Purchase>> GetRecentPurchasesAsync(int count = 10)
        {
            return await _dbSet
                .Where(p => !p.IsDeleted)
                .Include(p => p.PurchaseItems)
                    .ThenInclude(pi => pi.Product)
                .Include(p => p.Supplier)
                .OrderByDescending(p => p.PurchaseDate)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Purchase>> GetPendingReceivalsAsync()
        {
            return await _dbSet
                .Where(p => !p.IsDeleted && p.Status == PurchaseStatus.Ordered)
                .Include(p => p.PurchaseItems)
                .Include(p => p.Supplier)
                .OrderBy(p => p.PurchaseDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Purchase>> GetPagedAsync(int page, int pageSize, DateTime? startDate = null, DateTime? endDate = null, int? supplierId = null, string status = "", string? purchaseOrderNumber = null)
        {
            try
            {
                var query = BuildFilterQuery(startDate, endDate, supplierId, status, purchaseOrderNumber);
                
                return await query
                    .Include(p => p.PurchaseItems)
                    .Include(p => p.Supplier)
                    .OrderByDescending(p => p.PurchaseDate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error in GetPagedAsync: {ex.Message}", ex);
            }
        }

        public async Task<int> GetCountAsync(DateTime? startDate = null, DateTime? endDate = null, int? supplierId = null, string status = "", string? purchaseOrderNumber = null)
        {
            try
            {
                var query = BuildFilterQuery(startDate, endDate, supplierId, status, purchaseOrderNumber);
                return await query.CountAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error in GetCountAsync: {ex.Message}", ex);
            }
        }

        public async Task<int> GetTotalPurchasesAsync()
        {
            return await _dbSet.Where(p => !p.IsDeleted).CountAsync();
        }

        public async Task<int> GetPendingPurchasesAsync()
        {
            return await _dbSet.Where(p => !p.IsDeleted && p.Status == PurchaseStatus.Ordered).CountAsync();
        }

        public async Task<decimal> GetMonthlyPurchaseAmountAsync(int year, int month)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1);
            
            return await _dbSet
                .Where(p => !p.IsDeleted && p.Status == PurchaseStatus.Received && 
                           p.PurchaseDate >= startDate && p.PurchaseDate < endDate)
                .SumAsync(p => p.TotalAmount);
        }

        public async Task<IEnumerable<object>> GetPurchaseChartDataAsync(int days = 30)
        {
            var startDate = DateTime.Today.AddDays(-days);
            var endDate = DateTime.Today.AddDays(1);
            
            var purchases = await _dbSet
                .Where(p => !p.IsDeleted && p.PurchaseDate >= startDate && p.PurchaseDate < endDate)
                .GroupBy(p => p.PurchaseDate.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    TotalAmount = g.Sum(p => p.TotalAmount),
                    Count = g.Count()
                })
                .OrderBy(x => x.Date)
                .ToListAsync();
                
            return purchases.Cast<object>();
        }

        private IQueryable<Purchase> BuildFilterQuery(DateTime? startDate, DateTime? endDate, int? supplierId, string status, string? purchaseOrderNumber)
        {
            var query = _dbSet.Where(p => !p.IsDeleted);

            if (startDate.HasValue)
                query = query.Where(p => p.PurchaseDate.Date >= startDate.Value.Date);

            if (endDate.HasValue)
                query = query.Where(p => p.PurchaseDate.Date <= endDate.Value.Date);

            if (supplierId.HasValue)
                query = query.Where(p => p.SupplierId == supplierId.Value);

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<PurchaseStatus>(status, true, out var purchaseStatus))
                query = query.Where(p => p.Status == purchaseStatus);

            if (!string.IsNullOrEmpty(purchaseOrderNumber))
                query = query.Where(p => p.PurchaseOrderNumber.Contains(purchaseOrderNumber));

            return query;
        }
    }
}