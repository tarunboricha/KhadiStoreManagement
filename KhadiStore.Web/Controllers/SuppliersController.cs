using KhadiStore.Application.DTOs;
using KhadiStore.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KhadiStore.Web.Controllers
{
    [Authorize]
    public class SuppliersController : Controller
    {
        private readonly ISupplierService _supplierService;
        private readonly IPurchaseService _purchaseService;

        public SuppliersController(ISupplierService supplierService, IPurchaseService purchaseService)
        {
            _supplierService = supplierService;
            _purchaseService = purchaseService;
        }

        // GET: Suppliers
        public async Task<IActionResult> Index(string? name, string? city, string? state, bool? isActive, int page = 1, int pageSize = 20)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 20;

                var filter = new SupplierFilterDto
                {
                    Name = name,
                    City = city,
                    State = state,
                    IsActive = isActive,
                    Page = page,
                    PageSize = pageSize
                };

                var pagedResult = await _supplierService.GetPagedSuppliersAsync(filter);

                ViewBag.Name = name;
                ViewBag.City = city;
                ViewBag.State = state;
                ViewBag.IsActive = isActive;
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
                TempData["Error"] = "Error loading suppliers. Please try again.";
                return View(new List<SupplierDto>());
            }
        }

        // GET: Suppliers/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var supplier = await _supplierService.GetSupplierByIdAsync(id);
                if (supplier == null)
                {
                    TempData["Error"] = "Supplier not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Load related purchases
                var purchases = await _purchaseService.GetPurchasesBySupplierAsync(id);
                ViewBag.Purchases = purchases;

                return View(supplier);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error loading supplier details. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Suppliers/Create
        public IActionResult Create()
        {
            return View(new CreateSupplierDto());
        }

        // POST: Suppliers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateSupplierDto createSupplierDto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var supplier = await _supplierService.CreateSupplierAsync(createSupplierDto);
                    TempData["Success"] = "Supplier created successfully!";
                    return RedirectToAction(nameof(Details), new { id = supplier.Id });
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Error creating supplier: " + ex.Message;
                }
            }

            return View(createSupplierDto);
        }

        // GET: Suppliers/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var supplier = await _supplierService.GetSupplierByIdAsync(id);
                if (supplier == null)
                {
                    TempData["Error"] = "Supplier not found.";
                    return RedirectToAction(nameof(Index));
                }

                var updateDto = new UpdateSupplierDto
                {
                    Id = supplier.Id,
                    Name = supplier.Name,
                    ContactPerson = supplier.ContactPerson,
                    Phone = supplier.Phone,
                    Email = supplier.Email,
                    Address = supplier.Address,
                    City = supplier.City,
                    State = supplier.State,
                    PinCode = supplier.PinCode,
                    GSTNumber = supplier.GSTNumber,
                    IsActive = supplier.IsActive
                };

                return View(updateDto);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error loading supplier for editing. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Suppliers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UpdateSupplierDto updateSupplierDto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var success = await _supplierService.UpdateSupplierAsync(updateSupplierDto);
                    if (success)
                    {
                        TempData["Success"] = "Supplier updated successfully!";
                        return RedirectToAction(nameof(Details), new { id = updateSupplierDto.Id });
                    }
                    else
                    {
                        TempData["Error"] = "Supplier not found.";
                        return RedirectToAction(nameof(Index));
                    }
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Error updating supplier: " + ex.Message;
                }
            }

            return View(updateSupplierDto);
        }

        // POST: Suppliers/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var success = await _supplierService.DeleteSupplierAsync(id);
                if (success)
                {
                    TempData["Success"] = "Supplier deleted successfully!";
                }
                else
                {
                    TempData["Error"] = "Supplier not found.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error deleting supplier: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Suppliers/ToggleStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            try
            {
                var success = await _supplierService.ToggleSupplierStatusAsync(id);
                if (success)
                {
                    TempData["Success"] = "Supplier status updated successfully!";
                }
                else
                {
                    TempData["Error"] = "Supplier not found.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error updating supplier status: " + ex.Message;
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // AJAX: Search suppliers
        [HttpGet]
        public async Task<IActionResult> Search(string term)
        {
            try
            {
                var suppliers = await _supplierService.SearchSuppliersAsync(term ?? "");
                return Json(suppliers.Select(s => new
                {
                    id = s.Id,
                    name = s.Name,
                    contactPerson = s.ContactPerson,
                    phone = s.Phone,
                    email = s.Email,
                    city = s.City,
                    state = s.State
                }));
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        // AJAX: Get active suppliers for dropdown
        [HttpGet]
        public async Task<IActionResult> GetActiveSuppliers()
        {
            try
            {
                var suppliers = await _supplierService.GetActiveSuppliersAsync();
                return Json(suppliers.Select(s => new
                {
                    id = s.Id,
                    name = s.Name,
                    contactPerson = s.ContactPerson,
                    phone = s.Phone,
                    email = s.Email,
                    gstNumber = s.GSTNumber
                }));
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        // AJAX: Check if supplier name exists
        [HttpGet]
        public async Task<IActionResult> CheckSupplierExists(string name, int excludeId = 0)
        {
            try
            {
                var exists = await _supplierService.SupplierExistsAsync(name, excludeId);
                return Json(new { exists });
            }
            catch (Exception)
            {
                return Json(new { exists = false });
            }
        }

        // AJAX: Check if GST number exists
        [HttpGet]
        public async Task<IActionResult> CheckGSTExists(string gstNumber, int excludeId = 0)
        {
            try
            {
                var exists = await _supplierService.GSTNumberExistsAsync(gstNumber, excludeId);
                return Json(new { exists });
            }
            catch (Exception)
            {
                return Json(new { exists = false });
            }
        }
    }
}