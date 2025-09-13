using KhadiStore.Application.DTOs;
using KhadiStore.Application.Interfaces;
using KhadiStore.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KhadiStore.Web.Controllers
{
    [Authorize]
    public class ReturnsController : Controller
    {
        private readonly IReturnService _returnService;
        private readonly ISaleService _saleService;
        private readonly ILogger<ReturnsController> _logger;

        public ReturnsController(
            IReturnService returnService,
            ISaleService saleService,
            ILogger<ReturnsController> logger)
        {
            _returnService = returnService;
            _saleService = saleService;
            _logger = logger;
        }

        // GET: Returns
        public async Task<IActionResult> Index(DateTime? startDate = null, DateTime? endDate = null,
            string? refundMethod = null, int? saleId = null)
        {
            try
            {
                var returns = await _returnService.GetAllReturnsAsync();

                // Apply filters
                if (startDate.HasValue)
                    returns = returns.Where(r => r.ReturnDate.Date >= startDate.Value.Date);

                if (endDate.HasValue)
                    returns = returns.Where(r => r.ReturnDate.Date <= endDate.Value.Date);

                if (!string.IsNullOrEmpty(refundMethod))
                    returns = returns.Where(r => r.RefundMethod.Equals(refundMethod, StringComparison.OrdinalIgnoreCase));

                if (saleId.HasValue)
                    returns = returns.Where(r => r.SaleId == saleId.Value);

                // Set ViewBag for filters
                ViewBag.StartDate = startDate;
                ViewBag.EndDate = endDate;
                ViewBag.RefundMethod = refundMethod;
                ViewBag.SaleId = saleId;

                return View(returns.OrderByDescending(r => r.ReturnDate).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading returns index");
                TempData["Error"] = "Error loading returns. Please try again.";
                return View(new List<ReturnDto>());
            }
        }

        // GET: Returns/Create?saleId=9
        public async Task<IActionResult> Create(int saleId)
        {
            try
            {
                // Validate sale eligibility
                if (!await _returnService.CanCreateReturnAsync(saleId))
                {
                    TempData["Error"] = "This sale is not eligible for returns. Sales must be completed and within 30 days.";
                    return RedirectToAction("Details", "Sales", new { id = saleId });
                }

                // Get the original sale
                var sale = await _saleService.GetSaleByIdAsync(saleId);
                if (sale == null)
                {
                    TempData["Error"] = "Sale not found.";
                    return RedirectToAction("Index", "Sales");
                }

                // Get existing returns for this sale
                var existingReturns = await _returnService.GetReturnsBySaleIdAsync(saleId);
                ViewBag.ExistingReturns = existingReturns.ToList();

                // Get remaining returnable quantities for each sale item
                var remainingQuantities = await _returnService.GetRemainingReturnableQuantitiesAsync(saleId);
                ViewBag.RemainingQuantities = remainingQuantities ?? new Dictionary<int, int>();

                // Check if any items are still returnable - ENSURE this is always set
                var hasReturnableItems = remainingQuantities?.Values?.Any(qty => qty > 0) ?? false;
                ViewBag.HasReturnableItems = hasReturnableItems; // CRITICAL: Always set this

                ViewBag.TotalItemsReturned = remainingQuantities?.Keys?.Count - (remainingQuantities?.Values?.Count(qty => qty > 0) ?? 0);
                ViewBag.TotalItemsInSale = sale.SaleItems?.Count ?? 0;

                // Show info message for fully returned sales
                if (!hasReturnableItems)
                {
                    TempData["Info"] = "All items from this sale have been fully returned. You can view the return history below.";
                    ViewBag.ShowReturnHistory = true;
                }

                ViewBag.Sale = sale;

                var createReturnDto = new CreateReturnDto
                {
                    SaleId = saleId,
                    RefundMethod = sale.PaymentMethod ?? "Cash" // Default to Cash if null
                };

                return View(createReturnDto);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error loading return form. Please try again.";
                return RedirectToAction("Index", "Sales");
            }
        }

        // POST: Returns/Create - Simplified to immediate processing
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateReturnDto createReturnDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    foreach (var error in ModelState.Where(x => x.Value.Errors.Count > 0))
                    {
                        _logger.LogError("Validation Error - {Field}: {Errors}",
                            error.Key, string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage)));
                    }

                    await LoadCreateViewDataAsync(createReturnDto.SaleId);
                    return View(createReturnDto);
                }

                // Enhanced validation with specific error messages
                var remainingQuantities = await _returnService.GetRemainingReturnableQuantitiesAsync(createReturnDto.SaleId);
                var sale = await _saleService.GetSaleByIdAsync(createReturnDto.SaleId);

                bool hasValidationErrors = false;
                foreach (var returnItem in createReturnDto.ReturnItems)
                {
                    var remainingQty = remainingQuantities.GetValueOrDefault(returnItem.SaleItemId, 0);
                    var saleItem = sale?.SaleItems?.FirstOrDefault(si => si.Id == returnItem.SaleItemId);

                    if (remainingQty <= 0)
                    {
                        ModelState.AddModelError("", $"Product '{saleItem?.ProductName}' is already fully returned. No remaining quantity available.");
                        hasValidationErrors = true;
                    }
                    else if (returnItem.ReturnQuantity > remainingQty)
                    {
                        ModelState.AddModelError("", $"Product '{saleItem?.ProductName}': Requested quantity ({returnItem.ReturnQuantity}) exceeds remaining returnable quantity ({remainingQty}).");
                        hasValidationErrors = true;
                    }
                }

                if (hasValidationErrors)
                {
                    await LoadCreateViewDataAsync(createReturnDto.SaleId);
                    return View(createReturnDto);
                }

                // Validate return items using service
                if (!await _returnService.ValidateReturnItemsAsync(createReturnDto.SaleId, createReturnDto.ReturnItems))
                {
                    ModelState.AddModelError("", "Invalid return items selected. Please check the quantities and try again.");
                    await LoadCreateViewDataAsync(createReturnDto.SaleId);
                    return View(createReturnDto);
                }

                // Create and immediately process the return
                var returnId = await _returnService.CreateReturnAsync(createReturnDto);

                TempData["Success"] = "Return has been processed successfully! Refund completed and inventory updated.";

                return RedirectToAction("Details", new { id = returnId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating return for sale {SaleId}", createReturnDto.SaleId);
                ModelState.AddModelError("", "Error processing return: " + ex.Message);

                await LoadCreateViewDataAsync(createReturnDto.SaleId);
                return View(createReturnDto);
            }
        }

        // GET: Returns/Details/5 - Simplified for processed returns only
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var returnOrder = await _returnService.GetReturnByIdAsync(id);
                if (returnOrder == null)
                {
                    TempData["Error"] = "Return not found.";
                    return RedirectToAction("Index");
                }

                return View(returnOrder);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading return details for {ReturnId}", id);
                TempData["Error"] = "Error loading return details. Please try again.";
                return RedirectToAction("Index");
            }
        }

        // GET: Returns/ReturnReceipt/5
        public async Task<IActionResult> ReturnReceipt(int id)
        {
            try
            {
                var returnOrder = await _returnService.GetReturnByIdAsync(id);
                if (returnOrder == null)
                {
                    return NotFound();
                }

                return View(returnOrder);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading return receipt for {ReturnId}", id);
                TempData["Error"] = "Error loading return receipt. Please try again.";
                return RedirectToAction("Details", new { id });
            }
        }

        private async Task LoadCreateViewDataAsync(int saleId)
        {
            var sale = await _saleService.GetSaleByIdAsync(saleId);
            ViewBag.Sale = sale;

            var existingReturns = await _returnService.GetReturnsBySaleIdAsync(saleId);
            ViewBag.ExistingReturns = existingReturns.ToList();

            // Add remaining quantities
            var remainingQuantities = await _returnService.GetRemainingReturnableQuantitiesAsync(saleId);
            ViewBag.RemainingQuantities = remainingQuantities;
        }
    }
}