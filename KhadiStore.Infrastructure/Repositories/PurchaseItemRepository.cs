using KhadiStore.Application.Interfaces;
using KhadiStore.Domain.Entities;
using KhadiStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KhadiStore.Infrastructure.Repositories
{
    public class PurchaseItemRepository : Repository<PurchaseItem>, IPurchaseItemRepository
    {
        public PurchaseItemRepository(KhadiStoreDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<PurchaseItem>> GetByPurchaseIdAsync(int purchaseId)
        {
            return await _dbSet
                .Where(pi => !pi.IsDeleted && pi.PurchaseId == purchaseId)
                .Include(pi => pi.Product)
                .ToListAsync();
        }

        public async Task<IEnumerable<PurchaseItem>> GetByProductIdAsync(int productId)
        {
            return await _dbSet
                .Where(pi => !pi.IsDeleted && pi.ProductId == productId)
                .Include(pi => pi.Purchase)
                    .ThenInclude(p => p.Supplier)
                .OrderByDescending(pi => pi.Purchase.PurchaseDate)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalPurchaseAmountByProductAsync(int productId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _dbSet.Where(pi => !pi.IsDeleted && pi.ProductId == productId);

            if (startDate.HasValue || endDate.HasValue)
            {
                query = query.Include(pi => pi.Purchase);

                if (startDate.HasValue)
                    query = query.Where(pi => pi.Purchase.PurchaseDate >= startDate.Value);
                if (endDate.HasValue)
                    query = query.Where(pi => pi.Purchase.PurchaseDate < endDate.Value);
            }

            return await query.SumAsync(pi => pi.TotalAmount);
        }

        public async Task<int> GetTotalQuantityPurchasedAsync(int productId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _dbSet.Where(pi => !pi.IsDeleted && pi.ProductId == productId);

            if (startDate.HasValue || endDate.HasValue)
            {
                query = query.Include(pi => pi.Purchase);

                if (startDate.HasValue)
                    query = query.Where(pi => pi.Purchase.PurchaseDate >= startDate.Value);
                if (endDate.HasValue)
                    query = query.Where(pi => pi.Purchase.PurchaseDate < endDate.Value);
            }

            return await query.SumAsync(pi => pi.Quantity);
        }
    }
}
