using KhadiStore.Application.DTOs;

namespace KhadiStore.Application.Services
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetAllProductsAsync();
        Task<ProductDto?> GetProductByIdAsync(int id);
        Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(int categoryId);
        Task<IEnumerable<ProductDto>> SearchProductsAsync(string searchTerm);
        Task<IEnumerable<ProductDto>> GetLowStockProductsAsync();
        Task<IEnumerable<ProductDto>> GetActiveProductsAsync();
        Task<ProductDto> CreateProductAsync(CreateProductDto createProductDto);
        Task<ProductDto?> UpdateProductAsync(int id, UpdateProductDto updateProductDto);
        Task<bool> DeleteProductAsync(int id);
        Task<bool> UpdateStockAsync(int productId, int quantity);
        Task<decimal> GetInventoryValueAsync();
        Task<IEnumerable<ProductDto>> GetPagedProductsAsync(int pageIndex, int pageSize);
    }
}