using Microsoft.EntityFrameworkCore;
using KhadiStore.Domain.Entities;
using KhadiStore.Domain.Interfaces;
using KhadiStore.Infrastructure.Data;

namespace KhadiStore.Infrastructure.Repositories
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        public CategoryRepository(KhadiStoreDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Category>> GetActiveCategoriesAsync()
        {
            return await _dbSet
                .Where(c => !c.IsDeleted && c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<Category?> GetByNameAsync(string name)
        {
            return await _dbSet
                .Where(c => !c.IsDeleted && c.Name == name)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Category>> GetCategoriesWithProductCountAsync()
        {
            return await _dbSet
                .Where(c => !c.IsDeleted && c.IsActive)
                .Include(c => c.Products)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }
    }
}