using KhadiStore.Application.DTOs;
using KhadiStore.Application.Interfaces;
using KhadiStore.Application.Services;
using KhadiStore.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace KhadiStore.Web.Controllers
{
    [Authorize]
    public class SalesController : Controller
    {
        private readonly ISaleService _saleService;
        private readonly IProductService _productService;
        private readonly ICustomerService _customerService;
        private readonly IReturnService _returnService;
        private readonly ICategoryService _categoryService;

        public SalesController(ISaleService saleService, IProductService productService, ICustomerService customerService, IReturnService returnService, ICategoryService categoryService)
        {
            _saleService = saleService;
            _productService = productService;
            _customerService = customerService;
            _returnService = returnService;
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate, int page = 1, int pageSize = 20)
        {
            IEnumerable<SaleDto> sales;

            if (startDate.HasValue && endDate.HasValue)
            {
                sales = await _saleService.GetSalesByDateRangeAsync(startDate.Value, endDate.Value);
            }
            else
            {
                sales = await _saleService.GetAllSalesAsync();
            }

            var pagedSales = sales.Skip((page - 1) * pageSize).Take(pageSize);

            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = sales.Count();
            ViewBag.TotalPages = (int)Math.Ceiling((double)sales.Count() / pageSize);

            return View(pagedSales);
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var sale = await _saleService.GetSaleByIdAsync(id);
                if (sale == null)
                {
                    TempData["Error"] = "Sale not found.";
                    return RedirectToAction("Index");
                }

                // Check return status for this sale
                if (sale.Status == "Completed")
                {
                    var remainingQuantities = await _returnService.GetRemainingReturnableQuantitiesAsync(id);
                    var hasReturnableItems = remainingQuantities.Values.Any(qty => qty > 0);
                    var existingReturns = await _returnService.GetReturnsBySaleIdAsync(id);

                    ViewBag.HasReturnableItems = hasReturnableItems;
                    ViewBag.ExistingReturnsCount = existingReturns.Count();
                    ViewBag.TotalReturnedAmount = existingReturns.Sum(r => r.TotalAmount);
                }

                return View(sale);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error loading sale details. Please try again.";
                return RedirectToAction("Index");
            }
        }

        // GET: Sales/Create - WORKING VERSION
        public async Task<IActionResult> Create()
        {
            try
            {
                // Load categories - with proper error handling
                try
                {
                    var categories = await _categoryService.GetAllCategoriesAsync();
                    ViewBag.Categories = categories.Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.Name
                    }).ToList();
                }
                catch (Exception ex)
                {
                    ViewBag.Categories = new List<SelectListItem>();
                }

                // Load customers - with proper error handling  
                try
                {
                    var customers = await _customerService.GetAllCustomersAsync();
                    ViewBag.Customers = customers.Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.Name
                    }).ToList();
                }
                catch (Exception ex)
                {
                    ViewBag.Customers = new List<SelectListItem>();
                }

                var model = new CreateSaleDto
                {
                    SaleDate = DateTime.Now,
                    PaymentMethod = KhadiStore.Domain.Entities.PaymentMethod.Cash,
                    SaleItems = new List<CreateSaleItemDto>()
                };

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error loading sales form. Please try again.";
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetProductsByCategory(int categoryId)
        {
            try
            {
                // Try to get products using your service/repository
                var products = await _productService.GetProductsByCategoryAsync(categoryId);

                var productList = products
                    .Where(p => p.StockQuantity > 0)
                    .Select(p => new
                    {
                        id = p.Id,
                        name = p.Name,
                        price = p.Price, // Ensure it's a number
                        stock = p.StockQuantity,
                        gstRate = p.GST, // Ensure it's a number
                        description = p.Description ?? ""
                    })
                    .ToList();

                return Json(productList);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        // AJAX: Get product details - WORKING VERSION
        [HttpGet]
        public async Task<IActionResult> GetProductDetails(int productId)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(productId);
                if (product == null)
                {
                    return Json(null);
                }

                var productDetails = new
                {
                    id = product.Id,
                    name = product.Name,
                    price = product.Price, // Ensure it's a number
                    stock = product.StockQuantity,
                    gstRate = product.GST, // Ensure it's a number
                    description = product.Description ?? "",
                    category = product.CategoryName ?? ""
                };

                return Json(productDetails);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateSaleDto createSaleDto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if(!string.IsNullOrWhiteSpace(createSaleDto.CustomerPhone))
                    {
                        createSaleDto.CustomerId = await CreateCustomerIfNeededAsync(createSaleDto.CustomerName, createSaleDto.CustomerPhone);
                    }

                    var sale = await _saleService.CreateSaleAsync(createSaleDto);

                    // NEW: Update customer statistics after successful sale creation
                    if (sale.CustomerId.HasValue)
                    {
                        await UpdateCustomerStatisticsAsync(sale.CustomerId, sale.TotalAmount);
                    }

                    TempData["Success"] = "Sale completed successfully!";
                    return RedirectToAction(nameof(Details), new { id = sale.Id });
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Error creating sale: " + ex.Message;
                }
            }

            ViewBag.Products = await _productService.GetActiveProductsAsync();
            ViewBag.Customers = await _customerService.GetActiveCustomersAsync();
            return View(createSaleDto);
        }

        private async Task UpdateCustomerStatisticsAsync(int? customerId, decimal saleAmount)
        {
            if (!customerId.HasValue)
                return; // Skip for walk-in customers

            try
            {
                // Get customer
                var customer = await _customerService.GetCustomerByIdAsync(customerId.Value);
                if (customer == null)
                    return;

                // Update customer statistics
                await _customerService.UpdateCustomerStatisticsAsync(customerId.Value, saleAmount);
            }
            catch (Exception ex)
            {
                // Log the error but don't fail the sale creation
                // You can add logging here if needed
                // Just continue - customer stats update failure shouldn't break the sale
            }
        }

        private async Task<int?> CreateCustomerIfNeededAsync(string name, string phone)
        {
            // Skip if both are empty
            if (string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(phone))
                return null;

            try
            {
                // Check if customer already exists by phone
                if (!string.IsNullOrWhiteSpace(phone))
                {
                    var existingCustomer = await _customerService.GetCustomerByMobileAsync(phone);
                    if (existingCustomer != null)
                        return existingCustomer.Id;
                }

                // Create new customer
                var customer = new CreateCustomerDto
                {
                    Name = !string.IsNullOrWhiteSpace(name) ? name.Trim() : "Walk-in Customer",
                    Phone = !string.IsNullOrWhiteSpace(phone) ? phone.Trim() : "",
                    Email = "",
                    Address = "",
                };

                var newCustomer = await _customerService.CreateCustomerAsync(customer);

                return newCustomer.Id;
            }
            catch
            {
                // If customer creation fails, continue with sale as walk-in
                return null;
            }
        }

        public async Task<IActionResult> Invoice(int id)
        {
            var sale = await _saleService.GetSaleByIdAsync(id);
            if (sale == null)
            {
                return NotFound();
            }

            return View(sale);
        }

        [HttpGet]
        public async Task<IActionResult> GetProductPrice(int productId)
        {
            var product = await _productService.GetProductByIdAsync(productId);
            if (product != null)
            {
                return Json(new
                {
                    price = product.Price,
                    gst = product.GST,
                    stock = product.StockQuantity,
                    name = product.Name
                });
            }
            return Json(new { price = 0, gst = 5, stock = 0, name = "" });
        }

        [HttpGet]
        public async Task<IActionResult> GetCustomerByPhone(string phone)
        {
            var customer = await _customerService.GetCustomerByPhoneAsync(phone);
            if (customer != null)
            {
                return Json(new
                {
                    id = customer.Id,
                    name = customer.Name,
                    email = customer.Email
                });
            }
            return Json(new { id = 0, name = "", email = "" });
        }
    }
}