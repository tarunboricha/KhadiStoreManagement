using KhadiStore.Application.DTOs;
using KhadiStore.Application.Interfaces;
using KhadiStore.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace KhadiStore.Web.Controllers
{
    [Authorize]
    public class PurchasesController : Controller
    {
        private readonly IPurchaseService _purchaseService;
        private readonly ISupplierService _supplierService;
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;

        public PurchasesController(
            IPurchaseService purchaseService,
            ISupplierService supplierService,
            IProductService productService,
            ICategoryService categoryService)
        {
            _purchaseService = purchaseService;
            _supplierService = supplierService;
            _productService = productService;
            _categoryService = categoryService;
        }

        // GET: Purchases
        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate, int? supplierId, string status = "", string? purchaseOrderNumber = "", int page = 1, int pageSize = 20)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 20;

                var filter = new PurchaseFilterDto
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    SupplierId = supplierId,
                    Status = status,
                    PurchaseOrderNumber = purchaseOrderNumber,
                    Page = page,
                    PageSize = pageSize
                };

                var pagedResult = await _purchaseService.GetPagedPurchasesAsync(filter);

                // Load suppliers for filter dropdown
                var suppliers = await _supplierService.GetActiveSuppliersAsync();
                ViewBag.Suppliers = suppliers.Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Name,
                    Selected = s.Id == supplierId
                }).ToList();

                ViewBag.StartDate = startDate;
                ViewBag.EndDate = endDate;
                ViewBag.SupplierId = supplierId;
                ViewBag.Status = status;
                ViewBag.PurchaseOrderNumber = purchaseOrderNumber;
                ViewBag.CurrentPage = page;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalItems = pagedResult.TotalCount;
                ViewBag.TotalPages = pagedResult.TotalPages;
                ViewBag.HasPreviousPage = pagedResult.HasPreviousPage;
                ViewBag.HasNextPage = pagedResult.HasNextPage;

                return View(pagedResult.Items);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error loading purchases. Please try again.";
                return View(new List<PurchaseDto>());
            }
        }

        // GET: Purchases/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var purchase = await _purchaseService.GetPurchaseByIdAsync(id);
                if (purchase == null)
                {
                    TempData["Error"] = "Purchase not found.";
                    return RedirectToAction(nameof(Index));
                }

                return View(purchase);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error loading purchase details. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Purchases/Create
        public async Task<IActionResult> Create()
        {
            try
            {
                // Load suppliers
                var suppliers = await _supplierService.GetActiveSuppliersAsync();
                ViewBag.Suppliers = suppliers.Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Name
                }).ToList();

                // Load categories for product selection
                var categories = await _categoryService.GetAllCategoriesAsync();
                ViewBag.Categories = categories.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }).ToList();

                var model = new CreatePurchaseDto
                {
                    PurchaseDate = DateTime.Now,
                    PurchaseItems = new List<CreatePurchaseItemDto>()
                };

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error loading purchase form. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Purchases/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePurchaseDto createPurchaseDto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var purchase = await _purchaseService.CreatePurchaseAsync(createPurchaseDto);
                    TempData["Success"] = "Purchase created successfully!";
                    return RedirectToAction(nameof(Details), new { id = purchase.Id });
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Error creating purchase: " + ex.Message;
                }
            }

            // Reload ViewBag data if validation fails
            try
            {
                var suppliers = await _supplierService.GetActiveSuppliersAsync();
                ViewBag.Suppliers = suppliers.Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Name,
                    Selected = s.Id == createPurchaseDto.SupplierId
                }).ToList();

                var categories = await _categoryService.GetAllCategoriesAsync();
                ViewBag.Categories = categories.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }).ToList();
            }
            catch
            {
                ViewBag.Suppliers = new List<SelectListItem>();
                ViewBag.Categories = new List<SelectListItem>();
            }

            return View(createPurchaseDto);
        }

        // POST: Purchases/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int purchaseId, string newStatus, string? reason = null)
        {
            try
            {
                var canChange = await _purchaseService.CanChangeStatusAsync(purchaseId, newStatus);
                if (!canChange)
                {
                    TempData["Error"] = "Cannot change purchase status. Invalid status transition.";
                    return RedirectToAction("Details", new { id = purchaseId });
                }

                var success = await _purchaseService.UpdatePurchaseStatusAsync(purchaseId, newStatus, reason);
                if (success)
                {
                    TempData["Success"] = $"Purchase status updated to {newStatus} successfully.";
                }
                else
                {
                    TempData["Error"] = "Failed to update purchase status. Please try again.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error updating purchase status: " + ex.Message;
            }

            return RedirectToAction("Details", new { id = purchaseId });
        }

        // POST: Purchases/Receive/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Receive(int id, string? notes = null)
        {
            try
            {
                var success = await _purchaseService.ReceivePurchaseAsync(id, notes);
                if (success)
                {
                    TempData["Success"] = "Purchase received successfully! Stock has been updated.";
                }
                else
                {
                    TempData["Error"] = "Failed to receive purchase. Please try again.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error receiving purchase: " + ex.Message;
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: Purchases/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id, string? reason = null)
        {
            try
            {
                var success = await _purchaseService.CancelPurchaseAsync(id, reason);
                if (success)
                {
                    TempData["Success"] = "Purchase cancelled successfully!";
                }
                else
                {
                    TempData["Error"] = "Failed to cancel purchase. Please try again.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error cancelling purchase: " + ex.Message;
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: Purchases/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var success = await _purchaseService.DeletePurchaseAsync(id);
                if (success)
                {
                    TempData["Success"] = "Purchase deleted successfully!";
                }
                else
                {
                    TempData["Error"] = "Purchase not found.";
                }
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error deleting purchase: " + ex.Message;
                return RedirectToAction(nameof(Details), new { id });
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Purchases/PendingReceivalsReport
        public async Task<IActionResult> PendingReceivalsReport()
        {
            try
            {
                var pendingPurchases = await _purchaseService.GetPendingReceivalsAsync();
                return View(pendingPurchases);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error loading pending receivals report. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // AJAX: Get supplier details
        [HttpGet]
        public async Task<IActionResult> GetSupplierDetails(int supplierId)
        {
            try
            {
                var supplier = await _supplierService.GetSupplierByIdAsync(supplierId);
                if (supplier != null)
                {
                    return Json(new
                    {
                        id = supplier.Id,
                        name = supplier.Name,
                        contactPerson = supplier.ContactPerson,
                        phone = supplier.Phone,
                        email = supplier.Email,
                        address = supplier.Address,
                        city = supplier.City,
                        state = supplier.State,
                        gstNumber = supplier.GSTNumber
                    });
                }
                return Json(null);
            }
            catch (Exception)
            {
                return Json(null);
            }
        }

        // AJAX: Get products by category for purchase
        [HttpGet]
        public async Task<IActionResult> GetProductsByCategory(int categoryId)
        {
            try
            {
                var products = await _productService.GetProductsByCategoryAsync(categoryId);
                var productList = products.Select(p => new
                {
                    id = p.Id,
                    name = p.Name,
                    price = p.Price,
                    stock = p.StockQuantity,
                    gstRate = p.GST ?? 5.0m,
                    description = p.Description ?? ""
                }).ToList();

                return Json(productList);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        // AJAX: Get product details
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
                    gstRate = product.GST ?? 5.0m,
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

        // AJAX: Check if purchase order number exists
        [HttpGet]
        public async Task<IActionResult> CheckOrderNumberExists(string orderNumber, int excludeId = 0)
        {
            try
            {
                var exists = await _purchaseService.PurchaseOrderNumberExistsAsync(orderNumber, excludeId);
                return Json(new { exists });
            }
            catch (Exception)
            {
                return Json(new { exists = false });
            }
        }

        // AJAX: Check if status can be changed
        [HttpGet]
        public async Task<IActionResult> CanChangeStatus(int purchaseId, string newStatus)
        {
            try
            {
                var canChange = await _purchaseService.CanChangeStatusAsync(purchaseId, newStatus);
                return Json(new { canChange });
            }
            catch (Exception)
            {
                return Json(new { canChange = false });
            }
        }
    }
}