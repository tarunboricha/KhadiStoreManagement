using Microsoft.EntityFrameworkCore.Storage;
using KhadiStore.Domain.Interfaces;
using KhadiStore.Infrastructure.Data;
using KhadiStore.Application.Interfaces;

namespace KhadiStore.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly KhadiStoreDbContext _context;
        private IDbContextTransaction? _transaction;

        public UnitOfWork(KhadiStoreDbContext context)
        {
            _context = context;
            Products = new ProductRepository(_context);
            Customers = new CustomerRepository(_context);
            Suppliers = new SupplierRepository(_context);
            Sales = new SaleRepository(_context);
            Categories = new CategoryRepository(_context);
            Purchases = new PurchaseRepository(_context);
            Returns = new ReturnRepository(_context);
            PurchaseItems = new PurchaseItemRepository(_context);
        }

        public IProductRepository Products { get; }
        public ICustomerRepository Customers { get; }
        public ISupplierRepository Suppliers { get; }
        public ISaleRepository Sales { get; }
        public ICategoryRepository Categories { get; }
        public IPurchaseRepository Purchases { get; }
        public IReturnRepository Returns { get; }
        public IPurchaseItemRepository PurchaseItems { get; }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}