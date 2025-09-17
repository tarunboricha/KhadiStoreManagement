using KhadiStore.Domain.Entities;
using KhadiStore.Domain.Interfaces;

namespace KhadiStore.Application.Interfaces
{
    public interface IPurchaseRepository : IRepository<Purchase>
    {
        Task<IEnumerable<Purchase>> GetBySupplierAsync(int supplierId);
        Task<IEnumerable<Purchase>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<Purchase>> GetByStatusAsync(PurchaseStatus status);
        Task<Purchase?> GetByPurchaseOrderNumberAsync(string purchaseOrderNumber);
        Task<string> GeneratePurchaseOrderNumberAsync();
        Task<decimal> GetTotalPurchaseAmountAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<Purchase>> GetRecentPurchasesAsync(int count = 10);
        Task<IEnumerable<Purchase>> GetPendingReceivalsAsync();

        // Pagination methods
        Task<IEnumerable<Purchase>> GetPagedAsync(int page, int pageSize, DateTime? startDate = null, DateTime? endDate = null, int? supplierId = null, string status = "", string? purchaseOrderNumber = null);
        Task<int> GetCountAsync(DateTime? startDate = null, DateTime? endDate = null, int? supplierId = null, string status = "", string? purchaseOrderNumber = null);

        // Statistics
        Task<int> GetTotalPurchasesAsync();
        Task<int> GetPendingPurchasesAsync();
        Task<decimal> GetMonthlyPurchaseAmountAsync(int year, int month);
        Task<IEnumerable<object>> GetPurchaseChartDataAsync(int days = 30);
    }
}
