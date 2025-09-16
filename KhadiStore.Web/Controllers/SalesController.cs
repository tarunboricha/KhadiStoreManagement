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

        // UPDATED: Efficient pagination implementation
        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate, PaymentMethod? paymentMethod, string status = "", int page = 1, int pageSize = 20)
        {
            try
            {
                // Validate pagination parameters
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 20;

                // Get total count first (for pagination info)
                var totalCount = await _saleService.GetSalesCountAsync(startDate, endDate, paymentMethod, status);

                // Get paginated sales
                var sales = await _saleService.GetPagedSalesAsync(page, pageSize, startDate, endDate, paymentMethod, status);

                // Calculate pagination info
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                // Set ViewBag properties for the view
                ViewBag.StartDate = startDate;
                ViewBag.EndDate = endDate;
                ViewBag.PaymentMethod = paymentMethod;
                ViewBag.Status = status;
                ViewBag.CurrentPage = page;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalItems = totalCount;
                ViewBag.TotalPages = totalPages;
                ViewBag.HasPreviousPage = page > 1;
                ViewBag.HasNextPage = page < totalPages;

                return View(sales);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error loading sales data. Please try again.";
                return View(new List<SaleDto>());
            }
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
                    ViewBag.HasReturns = existingReturns.Any();
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
                // Load categories
                try
                {
                    var categories = await _categoryService.GetAllCategoriesAsync();
                    ViewBag.Categories = categories.Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.Name
                    }).ToList();
                }
                catch (Exception)
                {
                    ViewBag.Categories = new List<SelectListItem>();
                }

                // Load customers
                try
                {
                    var customers = await _customerService.GetAllCustomersAsync();
                    ViewBag.Customers = customers.Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.Name
                    }).ToList();
                }
                catch (Exception)
                {
                    ViewBag.Customers = new List<SelectListItem>();
                }

                var model = new CreateSaleDto
                {
                    SaleDate = DateTime.Now,
                    PaymentMethod = PaymentMethod.Cash,
                    SaleItems = new List<CreateSaleItemDto>(),
                    EnableRounding = true, // Default to enabled
                    RoundingMethod = RoundingMethod.RoundDown // Your specific requirement
                };

                return View(model);
            }
            catch (Exception)
            {
                TempData["Error"] = "Error loading sales form. Please try again.";
                return RedirectToAction("Index");
            }
        }

        // NEW: Update sale status action
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int saleId, string newStatus, string? reason = null)
        {
            try
            {
                var canChange = await _saleService.CanChangeStatusAsync(saleId, newStatus);
                if (!canChange)
                {
                    TempData["Error"] = "Cannot change sale status. Invalid status transition.";
                    return RedirectToAction("Details", new { id = saleId });
                }

                var success = await _saleService.UpdateSaleStatusAsync(saleId, newStatus, reason);
                if (success)
                {
                    TempData["Success"] = $"Sale status updated to {newStatus} successfully.";
                }
                else
                {
                    TempData["Error"] = "Failed to update sale status. Please try again.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error updating sale status: " + ex.Message;
            }

            return RedirectToAction("Details", new { id = saleId });
        }

        [HttpGet]
        public async Task<IActionResult> CalculateRoundedAmount(decimal amount, RoundingMethod method)
        {
            try
            {
                var roundedAmount = await _saleService.CalculateRoundedAmount(amount, method);
                var roundingDifference = roundedAmount - amount;

                return Json(new
                {
                    originalAmount = amount,
                    roundedAmount = roundedAmount,
                    roundingDifference = roundingDifference,
                    formattedOriginal = amount.ToString("N2"),
                    formattedRounded = roundedAmount.ToString("N2"),
                    formattedDifference = roundingDifference.ToString("N2")
                });
            }
            catch (Exception)
            {
                return Json(new
                {
                    originalAmount = amount,
                    roundedAmount = amount,
                    roundingDifference = 0,
                    formattedOriginal = amount.ToString("N2"),
                    formattedRounded = amount.ToString("N2"),
                    formattedDifference = "0.00"
                });
            }
        }

        // NEW: AJAX method to check if status can be changed
        [HttpGet]
        public async Task<IActionResult> CanChangeStatus(int saleId, string newStatus)
        {
            try
            {
                var canChange = await _saleService.CanChangeStatusAsync(saleId, newStatus);
                return Json(new { canChange = canChange });
            }
            catch (Exception)
            {
                return Json(new { canChange = false });
            }
        }


        [HttpGet]
        public async Task<IActionResult> GetProductsByCategory(int categoryId)
        {
            try
            {
                var products = await _productService.GetProductsByCategoryAsync(categoryId);
                var productList = products
                    .Where(p => p.StockQuantity > 0)
                    .Select(p => new
                    {
                        id = p.Id,
                        name = p.Name,
                        price = p.Price,
                        stock = p.StockQuantity,
                        gstRate = p.GST,
                        description = p.Description ?? ""
                    })
                    .ToList();

                return Json(productList);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

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
                    price = product.Price,
                    stock = product.StockQuantity,
                    gstRate = product.GST,
                    description = product.Description ?? "",
                    category = product.CategoryName ?? ""
                };

                return Json(productDetails);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
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
                    if (!string.IsNullOrWhiteSpace(createSaleDto.CustomerPhone))
                    {
                        createSaleDto.CustomerId = await CreateCustomerIfNeededAsync(createSaleDto.CustomerName, createSaleDto.CustomerPhone);
                    }

                    var sale = await _saleService.CreateSaleAsync(createSaleDto);

                    // Update customer statistics after successful sale creation
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

            // Reload ViewBag data if validation fails
            try
            {
                var categories = await _categoryService.GetAllCategoriesAsync();
                ViewBag.Categories = categories.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }).ToList();

                var customers = await _customerService.GetAllCustomersAsync();
                ViewBag.Customers = customers.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }).ToList();
            }
            catch
            {
                ViewBag.Categories = new List<SelectListItem>();
                ViewBag.Customers = new List<SelectListItem>();
            }

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
            catch (Exception)
            {
                // Log the error but don't fail the sale creation
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
            try
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
            catch (Exception)
            {
                return Json(new { price = 0, gst = 5, stock = 0, name = "" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCustomerByPhone(string phone)
        {
            try
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
            catch (Exception)
            {
                return Json(new { id = 0, name = "", email = "" });
            }
        }
    }
}