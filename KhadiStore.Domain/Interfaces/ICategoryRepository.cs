using KhadiStore.Domain.Entities;

namespace KhadiStore.Domain.Interfaces
{
    public interface ICategoryRepository : IRepository<Category>
    {
        Task<IEnumerable<Category>> GetActiveCategoriesAsync();
        Task<Category?> GetByNameAsync(string name);
        Task<IEnumerable<Category>> GetCategoriesWithProductCountAsync();
    }
}