using KhadiStore.Domain.Entities;

namespace KhadiStore.Domain.Interfaces
{
    public interface ISupplierRepository : IRepository<Supplier>
    {
        Task<IEnumerable<Supplier>> GetActiveSuppliersAsync();
        Task<Supplier?> GetByGSTNumberAsync(string gstNumber);
        Task<IEnumerable<Supplier>> GetByStateAsync(string state);
    }
}