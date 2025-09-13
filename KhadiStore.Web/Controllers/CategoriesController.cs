using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using KhadiStore.Application.Services;
using KhadiStore.Application.DTOs;

namespace KhadiStore.Web.Controllers
{
    [Authorize]
    public class CategoriesController : Controller
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _categoryService.GetActiveCategoriesAsync();
            return View(categories);
        }

        public async Task<IActionResult> Details(int id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateCategoryDto createCategoryDto)
        {
            if (ModelState.IsValid)
            {
                await _categoryService.CreateCategoryAsync(createCategoryDto);
                TempData["Success"] = "Category created successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(createCategoryDto);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            var updateDto = new CreateCategoryDto
            {
                Name = category.Name,
                Description = category.Description,
                ImagePath = category.ImagePath
            };

            return View(updateDto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CreateCategoryDto updateCategoryDto)
        {
            if (ModelState.IsValid)
            {
                var result = await _categoryService.UpdateCategoryAsync(id, updateCategoryDto);
                if (result != null)
                {
                    TempData["Success"] = "Category updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["Error"] = "Category not found!";
                }
            }

            return View(updateCategoryDto);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _categoryService.DeleteCategoryAsync(id);
            if (result)
            {
                TempData["Success"] = "Category deleted successfully!";
            }
            else
            {
                TempData["Error"] = "Category not found!";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}