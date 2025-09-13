using AutoMapper;
using KhadiStore.Application.DTOs;
using KhadiStore.Domain.Entities;
using KhadiStore.Domain.Interfaces;

namespace KhadiStore.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ProductService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
        {
            var products = await _unitOfWork.Products.GetAllAsync();
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public async Task<ProductDto?> GetProductByIdAsync(int id)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            return product != null ? _mapper.Map<ProductDto>(product) : null;
        }

        public async Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(int categoryId)
        {
            var products = await _unitOfWork.Products.GetByCategoryAsync(categoryId);
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public async Task<IEnumerable<ProductDto>> SearchProductsAsync(string searchTerm)
        {
            var products = await _unitOfWork.Products.SearchProductsAsync(searchTerm);
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public async Task<IEnumerable<ProductDto>> GetLowStockProductsAsync()
        {
            var products = await _unitOfWork.Products.GetLowStockProductsAsync();
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public async Task<IEnumerable<ProductDto>> GetActiveProductsAsync()
        {
            var products = await _unitOfWork.Products.GetActiveProductsAsync();
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public async Task<ProductDto> CreateProductAsync(CreateProductDto createProductDto)
        {
            var product = _mapper.Map<Product>(createProductDto);
            product.CreatedAt = DateTime.UtcNow;

            var createdProduct = await _unitOfWork.Products.AddAsync(product);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ProductDto>(createdProduct);
        }

        public async Task<ProductDto?> UpdateProductAsync(int id, UpdateProductDto updateProductDto)
        {
            var existingProduct = await _unitOfWork.Products.GetByIdAsync(id);
            if (existingProduct == null) return null;

            _mapper.Map(updateProductDto, existingProduct);
            existingProduct.ModifiedAt = DateTime.UtcNow;

            await _unitOfWork.Products.UpdateAsync(existingProduct);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ProductDto>(existingProduct);
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            var result = await _unitOfWork.Products.DeleteAsync(id);
            if (result)
            {
                await _unitOfWork.SaveChangesAsync();
            }
            return result;
        }

        public async Task<bool> UpdateStockAsync(int productId, int quantity)
        {
            var result = await _unitOfWork.Products.UpdateStockAsync(productId, quantity);
            if (result)
            {
                await _unitOfWork.SaveChangesAsync();
            }
            return result;
        }

        public async Task<decimal> GetInventoryValueAsync()
        {
            return await _unitOfWork.Products.GetTotalInventoryValueAsync();
        }

        public async Task<IEnumerable<ProductDto>> GetPagedProductsAsync(int pageIndex, int pageSize)
        {
            var products = await _unitOfWork.Products.GetPagedAsync(pageIndex, pageSize);
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }
    }
}