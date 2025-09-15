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

        // EXISTING METHODS (keep all of these as they are)
        public override async Task<Sale?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Where(s => !s.IsDeleted && s.Id == id)
                .Include(s => s.SaleItems)
                    .ThenInclude(si => si.Product)
                .Include(s => s.Customer)
                .FirstOrDefaultAsync();
        }

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

        // NEW: Efficient pagination methods
        public async Task<IEnumerable<Sale>> GetPagedAsync(int page, int pageSize, DateTime? startDate = null, DateTime? endDate = null, PaymentMethod? paymentMethod = null, string status = "")
        {
            try
            {
                var query = BuildFilterQuery(startDate, endDate, paymentMethod, status);

                return await query
                    .Include(s => s.SaleItems)
                    .Include(s => s.Customer)
                    .OrderByDescending(s => s.SaleDate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error in GetPagedAsync: {ex.Message}", ex);
            }
        }

        public async Task<int> GetCountAsync(DateTime? startDate = null, DateTime? endDate = null, PaymentMethod? paymentMethod = null, string status = "")
        {
            try
            {
                var query = BuildFilterQuery(startDate, endDate, paymentMethod, status);
                return await query.CountAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error in GetCountAsync: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<Sale>> GetPagedWithFiltersAsync(int page, int pageSize, string? customerName = null, string? invoiceNumber = null, DateTime? startDate = null, DateTime? endDate = null, PaymentMethod? paymentMethod = null, string status = "")
        {
            try
            {
                var query = BuildAdvancedFilterQuery(customerName, invoiceNumber, startDate, endDate, paymentMethod, status);

                return await query
                    .Include(s => s.SaleItems)
                    .Include(s => s.Customer)
                    .OrderByDescending(s => s.SaleDate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error in GetPagedWithFiltersAsync: {ex.Message}", ex);
            }
        }

        public async Task<int> GetCountWithFiltersAsync(string? customerName = null, string? invoiceNumber = null, DateTime? startDate = null, DateTime? endDate = null, PaymentMethod? paymentMethod = null, string status = "")
        {
            try
            {
                var query = BuildAdvancedFilterQuery(customerName, invoiceNumber, startDate, endDate, paymentMethod, status);
                return await query.CountAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error in GetCountWithFiltersAsync: {ex.Message}", ex);
            }
        }

        // PRIVATE: Helper methods to build queries
        private IQueryable<Sale> BuildFilterQuery(DateTime? startDate, DateTime? endDate, PaymentMethod? paymentMethod, string status)
        {
            var query = _dbSet.Where(s => !s.IsDeleted);

            // Date filtering
            if (startDate.HasValue)
                query = query.Where(s => s.SaleDate.Date >= startDate.Value.Date);

            if (endDate.HasValue)
                query = query.Where(s => s.SaleDate.Date <= endDate.Value.Date);

            // Payment method filtering
            if (paymentMethod.HasValue)
                query = query.Where(s => s.PaymentMethod == paymentMethod.Value);

            // Status filtering
            if (!string.IsNullOrEmpty(status))
            {
                if (Enum.TryParse<SaleStatus>(status, true, out var saleStatus))
                {
                    query = query.Where(s => s.Status == saleStatus);
                }
            }

            return query;
        }

        private IQueryable<Sale> BuildAdvancedFilterQuery(string? customerName, string? invoiceNumber, DateTime? startDate, DateTime? endDate, PaymentMethod? paymentMethod, string status)
        {
            var query = BuildFilterQuery(startDate, endDate, paymentMethod, status);

            // Customer name filtering
            if (!string.IsNullOrEmpty(customerName))
            {
                query = query.Where(s => s.CustomerName != null && s.CustomerName.Contains(customerName));
            }

            // Invoice number filtering
            if (!string.IsNullOrEmpty(invoiceNumber))
            {
                query = query.Where(s => s.InvoiceNumber.Contains(invoiceNumber));
            }

            return query;
        }
    }
}