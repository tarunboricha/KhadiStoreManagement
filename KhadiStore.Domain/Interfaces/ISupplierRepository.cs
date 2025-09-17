using KhadiStore.Domain.Entities;
using KhadiStore.Domain.Interfaces;

namespace KhadiStore.Application.Interfaces
{
    public interface ISupplierRepository : IRepository<Supplier>
    {
        Task<IEnumerable<Supplier>> GetActiveAsync();
        Task<Supplier?> GetByNameAsync(string name);
        Task<Supplier?> GetByGSTNumberAsync(string gstNumber);
        Task<IEnumerable<Supplier>> GetByLocationAsync(string city, string state);
        Task<bool> ExistsAsync(string name, int excludeId = 0);
        Task<IEnumerable<Supplier>> SearchAsync(string searchTerm);

        // Pagination methods
        Task<IEnumerable<Supplier>> GetPagedAsync(int page, int pageSize, string? name = null, string? city = null, string? state = null, bool? isActive = null);
        Task<int> GetCountAsync(string? name = null, string? city = null, string? state = null, bool? isActive = null);

        // Statistics
        Task<int> GetTotalSuppliersAsync();
        Task<int> GetActiveSuppliersAsync();
        Task<IEnumerable<Supplier>> GetTopSuppliersByPurchaseAmountAsync(int count = 10);
    }
}
