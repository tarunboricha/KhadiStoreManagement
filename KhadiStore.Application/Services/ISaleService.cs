using KhadiStore.Application.DTOs;

namespace KhadiStore.Application.Services
{
    public interface ISaleService
    {
        Task<IEnumerable<SaleDto>> GetAllSalesAsync();
        Task<SaleDto?> GetSaleByIdAsync(int id);
        Task<IEnumerable<SaleDto>> GetSalesByCustomerAsync(int customerId);
        Task<IEnumerable<SaleDto>> GetSalesByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<SaleDto>> GetTodaysSalesAsync();
        Task<SaleDto> CreateSaleAsync(CreateSaleDto createSaleDto);
        Task<decimal> GetTotalSalesAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<string> GenerateInvoiceNumberAsync();
        Task<SalesChartDto> GetSalesChartDataAsync(int days = 7);
        Task<IEnumerable<SaleDto>> GetRecentSalesAsync(int count = 10);
    }
}