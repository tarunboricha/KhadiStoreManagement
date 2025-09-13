using KhadiStore.Application.DTOs;

namespace KhadiStore.Application.Services
{
    public interface IDashboardService
    {
        Task<DashboardStatsDto> GetDashboardStatsAsync();
        Task<SalesChartDto> GetSalesChartDataAsync(int days = 7);
        Task<IEnumerable<TopProductDto>> GetTopProductsAsync(int count = 5);
        Task<IEnumerable<LowStockProductDto>> GetLowStockProductsAsync();
    }
}