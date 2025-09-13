using KhadiStore.Domain.Entities;

namespace KhadiStore.Domain.Interfaces
{
    public interface IReturnRepository : IRepository<Return>
    {
        Task<IEnumerable<Return>> GetBySaleIdAsync(int saleId);
        Task<IEnumerable<Return>> GetByCustomerIdAsync(int customerId);
        Task<IEnumerable<Return>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<decimal> GetTotalReturnsAmountAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<string> GenerateReturnNumberAsync();
        Task<int> GetReturnedQuantityForSaleItemAsync(int saleItemId);
        Task<Dictionary<int, int>> GetReturnedQuantitiesForSaleAsync(int saleId);
    }
}