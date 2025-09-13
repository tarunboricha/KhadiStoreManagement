using Microsoft.EntityFrameworkCore;
using KhadiStore.Domain.Entities;
using KhadiStore.Domain.Interfaces;
using KhadiStore.Infrastructure.Data;

namespace KhadiStore.Infrastructure.Repositories
{
    public class SaleRepository : Repository<Sale>, ISaleRepository
    {
        public SaleRepository(KhadiStoreDbContext context) : base(context)
        {
        }

        // OVERRIDE GetByIdAsync to include SaleItems
        public override async Task<Sale?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Where(s => !s.IsDeleted && s.Id == id)
                .Include(s => s.SaleItems)
                    .ThenInclude(si => si.Product)
                .Include(s => s.Customer)
                .FirstOrDefaultAsync();
        }

        // OVERRIDE GetAllAsync to include SaleItems  
        public override async Task<IEnumerable<Sale>> GetAllAsync()
        {
            return await _dbSet
                .Where(s => !s.IsDeleted)
                .Include(s => s.SaleItems)
                .Include(s => s.Customer)
                .OrderByDescending(s => s.SaleDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Sale>> GetByCustomerAsync(int customerId)
        {
            return await _dbSet
                .Where(s => !s.IsDeleted && s.CustomerId == customerId)
                .Include(s => s.SaleItems)
                    .ThenInclude(si => si.Product)
                .OrderByDescending(s => s.SaleDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Sale>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(s => !s.IsDeleted && s.SaleDate >= startDate && s.SaleDate < endDate)
                .Include(s => s.SaleItems)
                    .ThenInclude(si => si.Product)
                .OrderByDescending(s => s.SaleDate)
                .ToListAsync();
        }

        public async Task<Sale?> GetByInvoiceNumberAsync(string invoiceNumber)
        {
            return await _dbSet
                .Where(s => !s.IsDeleted && s.InvoiceNumber == invoiceNumber)
                .Include(s => s.SaleItems)
                    .ThenInclude(si => si.Product)
                .Include(s => s.Customer)
                .FirstOrDefaultAsync();
        }

        public async Task<decimal> GetTotalSalesAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _dbSet.Where(s => !s.IsDeleted && s.Status == SaleStatus.Completed);

            if (startDate.HasValue)
                query = query.Where(s => s.SaleDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(s => s.SaleDate < endDate.Value);

            return await query.SumAsync(s => s.TotalAmount);
        }

        public async Task<IEnumerable<Sale>> GetTodaysSalesAsync()
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            return await _dbSet
                .Where(s => !s.IsDeleted && s.SaleDate >= today && s.SaleDate < tomorrow)
                .Include(s => s.SaleItems)
                    .ThenInclude(si => si.Product)
                .OrderByDescending(s => s.SaleDate)
                .ToListAsync();
        }

        public async Task<string> GenerateInvoiceNumberAsync()
        {
            var today = DateTime.Today;
            var prefix = $"INV{today:yyyyMMdd}";

            var lastInvoice = await _dbSet
                .Where(s => s.InvoiceNumber.StartsWith(prefix))
                .OrderByDescending(s => s.InvoiceNumber)
                .FirstOrDefaultAsync();

            if (lastInvoice == null)
            {
                return $"{prefix}001";
            }

            var lastNumber = int.Parse(lastInvoice.InvoiceNumber.Substring(prefix.Length));
            return $"{prefix}{(lastNumber + 1):D3}";
        }

        public async Task<IEnumerable<Sale>> GetRecentSalesAsync(int count = 10)
        {
            return await _dbSet
                .Where(s => !s.IsDeleted)
                .Include(s => s.SaleItems)
                    .ThenInclude(si => si.Product)
                .OrderByDescending(s => s.SaleDate)
                .Take(count)
                .ToListAsync();
        }
    }
}