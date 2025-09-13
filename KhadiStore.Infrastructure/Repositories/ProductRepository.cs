using Microsoft.EntityFrameworkCore;
using KhadiStore.Domain.Entities;
using KhadiStore.Domain.Interfaces;
using KhadiStore.Infrastructure.Data;

namespace KhadiStore.Infrastructure.Repositories
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        public ProductRepository(KhadiStoreDbContext context) : base(context)
        {
        }

        public async Task<bool> IncrementStockAsync(int productId, int quantityToAdd)
        {
            try
            {
                // Use atomic SQL update to avoid race conditions
                var rowsAffected = await _context.Database.ExecuteSqlRawAsync(
                    "UPDATE Products SET StockQuantity = StockQuantity + {0}, ModifiedAt = {1} WHERE Id = {2} AND IsDeleted = 0",
                    quantityToAdd, DateTime.Now, productId);

                if (rowsAffected == 0)
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId)
        {
            return await _dbSet
                .Where(p => !p.IsDeleted && p.CategoryId == categoryId && p.IsActive)
                .Include(p => p.Category)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetLowStockProductsAsync()
        {
            return await _dbSet
                .Where(p => !p.IsDeleted && p.IsActive && p.StockQuantity <= p.MinStockLevel)
                .Include(p => p.Category)
                .OrderBy(p => p.StockQuantity)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm)
        {
            var term = searchTerm.ToLower();
            return await _dbSet
                .Where(p => !p.IsDeleted && p.IsActive &&
                           (p.Name.ToLower().Contains(term) ||
                            (p.Description != null && p.Description.ToLower().Contains(term)) ||
                            (p.FabricType != null && p.FabricType.ToLower().Contains(term)) ||
                            (p.Color != null && p.Color.ToLower().Contains(term)) ||
                            (p.SKU != null && p.SKU.ToLower().Contains(term))))
                .Include(p => p.Category)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<Product?> GetBySKUAsync(string sku)
        {
            return await _dbSet
                .Where(p => !p.IsDeleted && p.IsActive && p.SKU == sku)
                .Include(p => p.Category)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> UpdateStockAsync(int productId, int quantity)
        {
            var product = await GetByIdAsync(productId);
            if (product == null) return false;

            product.StockQuantity = Math.Max(0, product.StockQuantity + quantity);
            product.ModifiedAt = DateTime.UtcNow;

            return true;
        }

        public async Task<IEnumerable<Product>> GetByFabricTypeAsync(string fabricType)
        {
            return await _dbSet
                .Where(p => !p.IsDeleted && p.IsActive && p.FabricType == fabricType)
                .Include(p => p.Category)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalInventoryValueAsync()
        {
            return await _dbSet
                .Where(p => !p.IsDeleted && p.IsActive)
                .SumAsync(p => p.StockQuantity * p.Price);
        }

        public override async Task<Product> UpdateAsync(Product entity)
        {
            try
            {
                // Ensure we're working with a fresh context state
                var existingEntity = await _dbSet.FindAsync(entity.Id);
                if (existingEntity == null)
                {
                    throw new InvalidOperationException($"Product {entity.Id} not found for update");
                }

                // Update only the necessary fields to avoid conflicts
                existingEntity.StockQuantity = entity.StockQuantity;

                // Mark as modified
                _context.Entry(existingEntity).State = EntityState.Modified;

                // Save changes for this specific update
                var saveResult = await _context.SaveChangesAsync();

                return existingEntity;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId)
        {
            try
            {
                return await _dbSet
                    .Include(p => p.Category)
                    .Where(p => p.CategoryId == categoryId && !p.IsDeleted && p.StockQuantity > 0)
                    .OrderBy(p => p.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                return new List<Product>();
            }
        }

        public async Task<bool> UpdateInventoryAsync(int productId, int newStockQuantity)
        {
            try
            {
                // Use raw SQL for atomic inventory update - faster and avoids context conflicts
                var rowsAffected = await _context.Database.ExecuteSqlRawAsync(
                    "UPDATE Products SET StockQuantity = {0}, UpdatedAt = {1} WHERE Id = {2} AND IsDeleted = 0",
                    newStockQuantity, DateTime.Now, productId);

                if (rowsAffected == 0)
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<IEnumerable<Product>> GetActiveProductsAsync()
        {
            return await _dbSet
                .Where(p => !p.IsDeleted && p.IsActive)
                .Include(p => p.Category)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }
    }
}