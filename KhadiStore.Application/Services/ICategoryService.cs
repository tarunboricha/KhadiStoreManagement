using KhadiStore.Application.DTOs;

namespace KhadiStore.Application.Services
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync();
        Task<CategoryDto?> GetCategoryByIdAsync(int id);
        Task<IEnumerable<CategoryDto>> GetActiveCategoriesAsync();
        Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto createCategoryDto);
        Task<CategoryDto?> UpdateCategoryAsync(int id, CreateCategoryDto updateCategoryDto);
        Task<bool> DeleteCategoryAsync(int id);
    }
}