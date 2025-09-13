using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using KhadiStore.Application.Services;
using KhadiStore.Web.Models;
using System.Diagnostics;

namespace KhadiStore.Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IDashboardService _dashboardService;

        public HomeController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        public async Task<IActionResult> Index()
        {
            var stats = await _dashboardService.GetDashboardStatsAsync();
            var chartData = await _dashboardService.GetSalesChartDataAsync(7);
            var lowStockProducts = await _dashboardService.GetLowStockProductsAsync();

            ViewBag.Stats = stats;
            ViewBag.ChartData = chartData;
            ViewBag.LowStockProducts = lowStockProducts;

            return View();
        }

        [AllowAnonymous]
        public IActionResult Privacy()
        {
            return View();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}