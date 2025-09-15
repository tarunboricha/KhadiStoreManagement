using KhadiStore.Domain.Entities;

namespace KhadiStore.Domain.Interfaces
{
    public interface ISaleRepository : IRepository<Sale>
    {
        Task<IEnumerable<Sale>> GetByCustomerAsync(int customerId);
        Task<IEnumerable<Sale>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<Sale?> GetByInvoiceNumberAsync(string invoiceNumber);
        Task<decimal> GetTotalSalesAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<Sale>> GetTodaysSalesAsync();
        Task<string> GenerateInvoiceNumberAsync();
        Task<IEnumerable<Sale>> GetRecentSalesAsync(int count = 10);

        // NEW: Pagination methods
        Task<IEnumerable<Sale>> GetPagedAsync(int page, int pageSize, DateTime? startDate = null, DateTime? endDate = null, PaymentMethod? paymentMethod = null, string status = "");
        Task<int> GetCountAsync(DateTime? startDate = null, DateTime? endDate = null, PaymentMethod? paymentMethod = null, string status = "");
        Task<IEnumerable<Sale>> GetPagedWithFiltersAsync(int page, int pageSize, string? customerName = null, string? invoiceNumber = null, DateTime? startDate = null, DateTime? endDate = null, PaymentMethod? paymentMethod = null, string status = "");
        Task<int> GetCountWithFiltersAsync(string? customerName = null, string? invoiceNumber = null, DateTime? startDate = null, DateTime? endDate = null, PaymentMethod? paymentMethod = null, string status = "");
    }
}