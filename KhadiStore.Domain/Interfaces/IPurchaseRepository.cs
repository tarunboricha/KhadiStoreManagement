using KhadiStore.Domain.Entities;

namespace KhadiStore.Domain.Interfaces
{
    public interface IPurchaseRepository : IRepository<Purchase>
    {
        Task<IEnumerable<Purchase>> GetBySupplierAsync(int supplierId);
        Task<IEnumerable<Purchase>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<Purchase?> GetByPurchaseOrderNumberAsync(string purchaseOrderNumber);
        Task<string> GeneratePurchaseOrderNumberAsync();
        Task<IEnumerable<Purchase>> GetRecentPurchasesAsync(int count = 10);
    }
}