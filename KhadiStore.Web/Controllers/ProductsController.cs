using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using KhadiStore.Application.Services;
using KhadiStore.Application.DTOs;

namespace KhadiStore.Web.Controllers
{
    [Authorize]
    public class ProductsController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;

        public ProductsController(IProductService productService, ICategoryService categoryService)
        {
            _productService = productService;
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index(int? categoryId, string? search, int page = 1, int pageSize = 20)
        {
            IEnumerable<ProductDto> products;

            if (!string.IsNullOrEmpty(search))
            {
                products = await _productService.SearchProductsAsync(search);
            }
            else if (categoryId.HasValue)
            {
                products = await _productService.GetProductsByCategoryAsync(categoryId.Value);
            }
            else
            {
                products = await _productService.GetActiveProductsAsync();
            }

            var pagedProducts = products.Skip((page - 1) * pageSize).Take(pageSize);

            ViewBag.Categories = await _categoryService.GetActiveCategoriesAsync();
            ViewBag.CurrentCategory = categoryId;
            ViewBag.Search = search;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = products.Count();
            ViewBag.TotalPages = (int)Math.Ceiling((double)products.Count() / pageSize);

            return View(pagedProducts);
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _categoryService.GetActiveCategoriesAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateProductDto createProductDto)
        {
            if (ModelState.IsValid)
            {
                await _productService.CreateProductAsync(createProductDto);
                TempData["Success"] = "Product created successfully!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = await _categoryService.GetActiveCategoriesAsync();
            return View(createProductDto);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            var updateDto = new UpdateProductDto
            {
                Name = product.Name,
                Description = product.Description,
                CategoryId = product.CategoryId,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                MinStockLevel = product.MinStockLevel,
                SKU = product.SKU,
                FabricType = product.FabricType,
                Color = product.Color,
                Size = product.Size,
                Pattern = product.Pattern,
                GST = product.GST,
                ImagePath = product.ImagePath,
                Weight = product.Weight,
                Origin = product.Origin,
                IsActive = product.IsActive
            };

            ViewBag.Categories = await _categoryService.GetActiveCategoriesAsync();
            return View(updateDto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateProductDto updateProductDto)
        {
            if (ModelState.IsValid)
            {
                var result = await _productService.UpdateProductAsync(id, updateProductDto);
                if (result != null)
                {
                    TempData["Success"] = "Product updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["Error"] = "Product not found!";
                }
            }

            ViewBag.Categories = await _categoryService.GetActiveCategoriesAsync();
            return View(updateProductDto);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _productService.DeleteProductAsync(id);
            if (result)
            {
                TempData["Success"] = "Product deleted successfully!";
            }
            else
            {
                TempData["Error"] = "Product not found!";
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> LowStock()
        {
            var products = await _productService.GetLowStockProductsAsync();
            return View(products);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStock(int id, int quantity)
        {
            var result = await _productService.UpdateStockAsync(id, quantity);
            if (result)
            {
                return Json(new { success = true, message = "Stock updated successfully!" });
            }
            return Json(new { success = false, message = "Failed to update stock!" });
        }
    }
}