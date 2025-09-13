using Microsoft.EntityFrameworkCore;
using KhadiStore.Domain.Entities;
using KhadiStore.Domain.Interfaces;
using KhadiStore.Infrastructure.Data;

namespace KhadiStore.Infrastructure.Repositories
{
    public class PurchaseRepository : Repository<Purchase>, IPurchaseRepository
    {
        public PurchaseRepository(KhadiStoreDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Purchase>> GetBySupplierAsync(int supplierId)
        {
            return await _dbSet
                .Where(p => !p.IsDeleted && p.SupplierId == supplierId)
                .Include(p => p.PurchaseItems)
                .OrderByDescending(p => p.PurchaseDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Purchase>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(p => !p.IsDeleted && p.PurchaseDate >= startDate && p.PurchaseDate < endDate)
                .Include(p => p.PurchaseItems)
                .OrderByDescending(p => p.PurchaseDate)
                .ToListAsync();
        }

        public async Task<Purchase?> GetByPurchaseOrderNumberAsync(string purchaseOrderNumber)
        {
            return await _dbSet
                .Where(p => !p.IsDeleted && p.PurchaseOrderNumber == purchaseOrderNumber)
                .Include(p => p.PurchaseItems)
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

        public async Task<IEnumerable<Purchase>> GetRecentPurchasesAsync(int count = 10)
        {
            return await _dbSet
                .Where(p => !p.IsDeleted)
                .Include(p => p.PurchaseItems)
                .OrderByDescending(p => p.PurchaseDate)
                .Take(count)
                .ToListAsync();
        }
    }
}