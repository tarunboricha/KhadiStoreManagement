using KhadiStore.Domain.Entities;

namespace KhadiStore.Domain.Interfaces
{
    public interface IProductRepository : IRepository<Product>
    {
        Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId);
        Task<IEnumerable<Product>> GetLowStockProductsAsync();
        Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm);
        Task<Product?> GetBySKUAsync(string sku);
        Task<bool> UpdateStockAsync(int productId, int quantity);
        Task<IEnumerable<Product>> GetByFabricTypeAsync(string fabricType);
        Task<decimal> GetTotalInventoryValueAsync();
        Task<IEnumerable<Product>> GetActiveProductsAsync();
        Task<bool> IncrementStockAsync(int productId, int quantityToAdd);
        Task<bool> UpdateInventoryAsync(int productId, int newStockQuantity);
        Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId);
    }
}