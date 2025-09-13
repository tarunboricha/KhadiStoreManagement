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
    }
}