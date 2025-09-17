using KhadiStore.Application.Interfaces;

namespace KhadiStore.Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        // Existing properties (keep these as they are)
        ICategoryRepository Categories { get; }
        ICustomerRepository Customers { get; }
        IProductRepository Products { get; }
        IPurchaseRepository Purchases { get; }
        ISaleRepository Sales { get; }
        ISupplierRepository Suppliers { get; }
        IReturnRepository Returns { get; }
        // NEW: Purchase-related repositories
        IPurchaseItemRepository PurchaseItems { get; }

        // Existing methods (keep these as they are)
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
        Task<int> SaveChangesAsync();
    }
}