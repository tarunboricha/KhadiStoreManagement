using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using KhadiStore.Application.Services;
using KhadiStore.Application.DTOs;

namespace KhadiStore.Web.Controllers
{
    [Authorize]
    public class CustomersController : Controller
    {
        private readonly ICustomerService _customerService;
        private readonly ISaleService _saleService;

        public CustomersController(ICustomerService customerService, ISaleService saleService)
        {
            _customerService = customerService;
            _saleService = saleService;
        }

        public async Task<IActionResult> Index(int page = 1, int pageSize = 20)
        {
            var customers = await _customerService.GetActiveCustomersAsync();
            var pagedCustomers = customers.Skip((page - 1) * pageSize).Take(pageSize);

            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = customers.Count();
            ViewBag.TotalPages = (int)Math.Ceiling((double)customers.Count() / pageSize);

            return View(pagedCustomers);
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var customer = await _customerService.GetCustomerByIdAsync(id);
                if (customer == null)
                {
                    return NotFound();
                }   

                // NEW: Get customer statistics
                var statistics = await _customerService.GetCustomerStatisticsAsync(id);
                ViewBag.CustomerStatistics = statistics;

                return View(customer);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error loading customer details: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateCustomerDto createCustomerDto)
        {
            if (ModelState.IsValid)
            {
                await _customerService.CreateCustomerAsync(createCustomerDto);
                TempData["Success"] = "Customer created successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(createCustomerDto);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var customer = await _customerService.GetCustomerByIdAsync(id);
            if (customer == null)
            {
                return NotFound();
            }

            var updateDto = new CreateCustomerDto
            {
                Name = customer.Name,
                Phone = customer.Phone,
                Email = customer.Email,
                Address = customer.Address,
                City = customer.City,
                State = customer.State,
                PinCode = customer.PinCode,
                GSTNumber = customer.GSTNumber,
                CustomerType = Enum.Parse<Domain.Entities.CustomerType>(customer.CustomerType)
            };

            return View(updateDto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CreateCustomerDto updateCustomerDto)
        {
            if (ModelState.IsValid)
            {
                var result = await _customerService.UpdateCustomerAsync(id, updateCustomerDto);
                if (result != null)
                {
                    TempData["Success"] = "Customer updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["Error"] = "Customer not found!";
                }
            }

            return View(updateCustomerDto);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _customerService.DeleteCustomerAsync(id);
            if (result)
            {
                TempData["Success"] = "Customer deleted successfully!";
            }
            else
            {
                TempData["Error"] = "Customer not found!";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}