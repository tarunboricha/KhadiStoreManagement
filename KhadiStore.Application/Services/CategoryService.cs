using AutoMapper;
using KhadiStore.Application.DTOs;
using KhadiStore.Domain.Entities;
using KhadiStore.Domain.Interfaces;

namespace KhadiStore.Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CategoryService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
        {
            var categories = await _unitOfWork.Categories.GetAllAsync();
            return _mapper.Map<IEnumerable<CategoryDto>>(categories);
        }

        public async Task<CategoryDto?> GetCategoryByIdAsync(int id)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(id);
            return category != null ? _mapper.Map<CategoryDto>(category) : null;
        }

        public async Task<IEnumerable<CategoryDto>> GetActiveCategoriesAsync()
        {
            var categories = await _unitOfWork.Categories.GetActiveCategoriesAsync();
            return _mapper.Map<IEnumerable<CategoryDto>>(categories);
        }

        public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto createCategoryDto)
        {
            var category = _mapper.Map<Category>(createCategoryDto);
            category.CreatedAt = DateTime.UtcNow;

            var createdCategory = await _unitOfWork.Categories.AddAsync(category);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<CategoryDto>(createdCategory);
        }

        public async Task<CategoryDto?> UpdateCategoryAsync(int id, CreateCategoryDto updateCategoryDto)
        {
            var existingCategory = await _unitOfWork.Categories.GetByIdAsync(id);
            if (existingCategory == null) return null;

            _mapper.Map(updateCategoryDto, existingCategory);
            existingCategory.ModifiedAt = DateTime.UtcNow;

            await _unitOfWork.Categories.UpdateAsync(existingCategory);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<CategoryDto>(existingCategory);
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var result = await _unitOfWork.Categories.DeleteAsync(id);
            if (result)
            {
                await _unitOfWork.SaveChangesAsync();
            }
            return result;
        }
    }
}