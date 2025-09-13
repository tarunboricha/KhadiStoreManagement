using Microsoft.EntityFrameworkCore;
using KhadiStore.Domain.Entities;
using KhadiStore.Domain.Interfaces;
using KhadiStore.Infrastructure.Data;

namespace KhadiStore.Infrastructure.Repositories
{
    public class ReturnRepository : Repository<Return>, IReturnRepository
    {
        public ReturnRepository(KhadiStoreDbContext context) : base(context)
        {
        }

        public override async Task<Return?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Where(r => !r.IsDeleted && r.Id == id)
                .Include(r => r.ReturnItems)
                    .ThenInclude(ri => ri.Product)
                .Include(r => r.Sale)
                    .ThenInclude(s => s.SaleItems)
                .Include(r => r.Customer)
                .FirstOrDefaultAsync();
        }

        public override async Task<IEnumerable<Return>> GetAllAsync()
        {
            return await _dbSet
                .Where(r => !r.IsDeleted)
                .Include(r => r.ReturnItems)
                .Include(r => r.Sale)
                .Include(r => r.Customer)
                .OrderByDescending(r => r.ReturnDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Return>> GetBySaleIdAsync(int saleId)
        {
            return await _dbSet
                .Where(r => !r.IsDeleted && r.SaleId == saleId)
                .Include(r => r.ReturnItems)
                    .ThenInclude(ri => ri.Product)
                .Include(r => r.Sale)
                .OrderByDescending(r => r.ReturnDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Return>> GetByCustomerIdAsync(int customerId)
        {
            return await _dbSet
                .Where(r => !r.IsDeleted && r.CustomerId == customerId)
                .Include(r => r.ReturnItems)
                    .ThenInclude(ri => ri.Product)
                .Include(r => r.Sale)
                .Include(r => r.Customer)
                .OrderByDescending(r => r.ReturnDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Return>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(r => !r.IsDeleted &&
                           r.ReturnDate >= startDate &&
                           r.ReturnDate <= endDate.AddDays(1).AddTicks(-1))
                .Include(r => r.ReturnItems)
                    .ThenInclude(ri => ri.Product)
                .Include(r => r.Sale)
                .Include(r => r.Customer)
                .OrderByDescending(r => r.ReturnDate)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalReturnsAmountAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var query = _dbSet.Where(r => !r.IsDeleted && r.IsProcessed);

                if (startDate.HasValue)
                    query = query.Where(r => r.ReturnDate >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(r => r.ReturnDate <= endDate.Value.AddDays(1).AddTicks(-1));

                var result = await query.SumAsync(r => r.TotalAmount);
                return result;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public async Task<string> GenerateReturnNumberAsync()
        {
            var today = DateTime.Today;
            var prefix = $"RET{today:yyyyMMdd}";

            var lastReturn = await _dbSet
                .Where(r => r.ReturnNumber.StartsWith(prefix))
                .OrderByDescending(r => r.ReturnNumber)
                .FirstOrDefaultAsync();

            if (lastReturn == null)
            {
                return $"{prefix}001";
            }

            var lastNumberStr = lastReturn.ReturnNumber.Substring(prefix.Length);
            if (int.TryParse(lastNumberStr, out int lastNumber))
            {
                return $"{prefix}{(lastNumber + 1):D3}";
            }

            // Fallback if parsing fails
            var todaysReturns = await _dbSet
                .CountAsync(r => r.ReturnNumber.StartsWith(prefix));
            return $"{prefix}{(todaysReturns + 1):D3}";
        }

        public override async Task<Return> AddAsync(Return entity)
        {
            try
            {
                entity.CreatedAt = DateTime.Now;
                entity.IsDeleted = false;

                // Add to context
                var result = await _dbSet.AddAsync(entity);

                // Save changes immediately for this entity
                var saveResult = await _context.SaveChangesAsync();

                // Verify the entity was saved
                if (result.Entity.Id == 0)
                {
                    throw new InvalidOperationException("Return was not saved - ID is still 0");
                }

                return result.Entity;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        // Also add this method to get sale with items explicitly
        public async Task<Sale> GetSaleByIdWithItemsAsync(int saleId)
        {
            try
            {
                var sale = await _context.Sales
                    .Include(s => s.SaleItems)
                    .Include(s => s.Customer)
                    .FirstOrDefaultAsync(s => s.Id == saleId && !s.IsDeleted);

                return sale;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<int> GetReturnedQuantityForSaleItemAsync(int saleItemId)
        {
            try
            {
                // Since all returns are automatically processed, count all non-deleted returns
                var returnedQty = await _dbSet
                    .Where(r => !r.IsDeleted && r.IsProcessed) // All returns are processed
                    .SelectMany(r => r.ReturnItems)
                    .Where(ri => !ri.IsDeleted && ri.SaleItemId == saleItemId)
                    .SumAsync(ri => ri.ReturnQuantity);

                return returnedQty;
            }
            catch (Exception ex)
            {
                // Log error and return 0 as safe fallback
                return 0;
            }
        }

        public async Task<Dictionary<int, int>> GetReturnedQuantitiesForSaleAsync(int saleId)
        {
            try
            {
                // Use a fresh query with no tracking to avoid context conflicts
                var returnedItems = await _context.Returns
                    .AsNoTracking() // Critical: Don't track these entities
                    .Where(r => !r.IsDeleted && r.SaleId == saleId && r.IsProcessed)
                    .SelectMany(r => r.ReturnItems)
                    .Where(ri => !ri.IsDeleted)
                    .GroupBy(ri => ri.SaleItemId)
                    .Select(g => new { SaleItemId = g.Key, TotalReturned = g.Sum(ri => ri.ReturnQuantity) })
                    .ToListAsync();

                return returnedItems.ToDictionary(x => x.SaleItemId, x => x.TotalReturned);
            }
            catch (Exception ex)
            {
                return new Dictionary<int, int>();
            }
        }
    }
}