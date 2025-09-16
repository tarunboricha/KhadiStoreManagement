using AutoMapper;
using KhadiStore.Application.DTOs;
using KhadiStore.Domain.Entities;
using KhadiStore.Domain.Interfaces;

namespace KhadiStore.Application.Services
{
    public class SaleService : ISaleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public SaleService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        // EXISTING METHODS (keep these as they are)
        public async Task<IEnumerable<SaleDto>> GetAllSalesAsync()
        {
            var sales = await _unitOfWork.Sales.GetAllAsync();
            return _mapper.Map<IEnumerable<SaleDto>>(sales);
        }

        public async Task<SaleDto?> GetSaleByIdAsync(int id)
        {
            var sale = await _unitOfWork.Sales.GetByIdAsync(id);
            return sale != null ? _mapper.Map<SaleDto>(sale) : null;
        }

        public async Task<IEnumerable<SaleDto>> GetSalesByCustomerAsync(int customerId)
        {
            var sales = await _unitOfWork.Sales.GetByCustomerAsync(customerId);
            return _mapper.Map<IEnumerable<SaleDto>>(sales);
        }

        public async Task<IEnumerable<SaleDto>> GetSalesByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var sales = await _unitOfWork.Sales.GetByDateRangeAsync(startDate, endDate);
            return _mapper.Map<IEnumerable<SaleDto>>(sales);
        }

        public async Task<IEnumerable<SaleDto>> GetTodaysSalesAsync()
        {
            var sales = await _unitOfWork.Sales.GetTodaysSalesAsync();
            return _mapper.Map<IEnumerable<SaleDto>>(sales);
        }

        // UPDATED: CreateSaleAsync with rounding functionality
        public async Task<SaleDto> CreateSaleAsync(CreateSaleDto createSaleDto)
        {
            var sale = _mapper.Map<Sale>(createSaleDto);
            sale.InvoiceNumber = await GenerateInvoiceNumberAsync();
            sale.CreatedAt = DateTime.UtcNow;

            // Calculate totals
            decimal subTotal = 0;
            decimal gstTotal = 0;

            foreach (var item in sale.SaleItems)
            {
                var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId);
                if (product != null)
                {
                    item.ProductName = product.Name;
                    item.UnitPrice = product.Price;
                    item.GSTRate = product.GST ?? 5.0m;
                    item.DiscountAmount = (item.UnitPrice * createSaleDto.BillDiscountPercentage / 100) * item.Quantity;

                    var itemTotal = item.UnitPrice * item.Quantity;
                    var gstAmount = itemTotal * item.GSTRate / 100;

                    item.GSTAmount = gstAmount;
                    item.TotalAmount = itemTotal + gstAmount - item.DiscountAmount;

                    subTotal += itemTotal;
                    gstTotal += gstAmount;

                    // Update product stock
                    await _unitOfWork.Products.UpdateStockAsync(product.Id, -item.Quantity);
                }
            }

            sale.SubTotal = subTotal;
            sale.GSTAmount = gstTotal;
            sale.DiscountAmount = (subTotal + gstTotal) * createSaleDto.BillDiscountPercentage / 100;

            // Calculate total before rounding
            var calculatedTotal = subTotal + gstTotal - sale.DiscountAmount;
            sale.CalculatedTotal = calculatedTotal;

            // Apply rounding if enabled
            if (createSaleDto.EnableRounding)
            {
                var roundedAmount = await CalculateRoundedAmount(calculatedTotal, createSaleDto.RoundingMethod);
                sale.TotalAmount = roundedAmount;
                sale.RoundingAmount = roundedAmount - calculatedTotal;
            }
            else
            {
                sale.TotalAmount = calculatedTotal;
                sale.RoundingAmount = 0;
            }

            var createdSale = await _unitOfWork.Sales.AddAsync(sale);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<SaleDto>(createdSale);
        }

        // NEW: Calculate rounded amount based on method
        public async Task<decimal> CalculateRoundedAmount(decimal amount, RoundingMethod method = RoundingMethod.NearestTen)
        {
            return method switch
            {
                RoundingMethod.None => amount,
                RoundingMethod.NearestFive => Math.Round(amount / 5, 0, MidpointRounding.AwayFromZero) * 5,
                RoundingMethod.NearestTen => Math.Round(amount / 10, 0, MidpointRounding.AwayFromZero) * 10,
                RoundingMethod.RoundDown => Math.Floor(amount / 10) * 10, // Your specific requirement: 1002->1000, 1017->1010
                _ => amount
            };
        }

        // NEW: Update sale status
        public async Task<bool> UpdateSaleStatusAsync(int saleId, string newStatus, string? reason = null)
        {
            try
            {
                var sale = await _unitOfWork.Sales.GetByIdAsync(saleId);
                if (sale == null)
                    return false;

                // Validate status change
                if (!await CanChangeStatusAsync(saleId, newStatus))
                    return false;

                // Parse and validate new status
                if (!Enum.TryParse<SaleStatus>(newStatus, true, out var saleStatus))
                    return false;

                // Update status
                sale.Status = saleStatus;

                // Add reason to notes if provided
                if (!string.IsNullOrWhiteSpace(reason))
                {
                    var statusChangeNote = $"\n[{DateTime.Now:yyyy-MM-dd HH:mm}] Status changed to {newStatus}. Reason: {reason}";
                    sale.Notes = (sale.Notes ?? "") + statusChangeNote;
                }

                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // NEW: Check if status can be changed
        public async Task<bool> CanChangeStatusAsync(int saleId, string newStatus)
        {
            var sale = await _unitOfWork.Sales.GetByIdAsync(saleId);
            if (sale == null)
                return false;

            // Cannot change status if already returned
            if (sale.Status == SaleStatus.Returned)
                return false;

            // Parse new status
            if (!Enum.TryParse<SaleStatus>(newStatus, true, out var targetStatus))
                return false;

            // Business rules for status changes
            return sale.Status switch
            {
                SaleStatus.Pending => targetStatus is SaleStatus.Completed or SaleStatus.Cancelled or SaleStatus.Pending,
                SaleStatus.Completed => targetStatus is SaleStatus.Cancelled or SaleStatus.Completed,
                SaleStatus.Cancelled => targetStatus is SaleStatus.Pending or SaleStatus.Cancelled,
                SaleStatus.Returned => false, // Cannot change returned status
                _ => false
            };
        }

        public async Task<decimal> GetTotalSalesAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            return await _unitOfWork.Sales.GetTotalSalesAsync(startDate, endDate);
        }

        public async Task<string> GenerateInvoiceNumberAsync()
        {
            return await _unitOfWork.Sales.GenerateInvoiceNumberAsync();
        }

        public async Task<SalesChartDto> GetSalesChartDataAsync(int days = 7)
        {
            var startDate = DateTime.Today.AddDays(-days);
            var endDate = DateTime.Today;

            var salesData = new SalesChartDto();

            for (int i = 0; i < days; i++)
            {
                var date = startDate.AddDays(i);
                var dayStart = date;
                var dayEnd = date.AddDays(1);

                var dayTotal = await _unitOfWork.Sales.GetTotalSalesAsync(dayStart, dayEnd);

                salesData.Labels.Add(date.ToString("MMM dd"));
                salesData.Values.Add(dayTotal);
            }

            return salesData;
        }

        public async Task<IEnumerable<SaleDto>> GetRecentSalesAsync(int count = 10)
        {
            var sales = await _unitOfWork.Sales.GetRecentSalesAsync(count);
            return _mapper.Map<IEnumerable<SaleDto>>(sales);
        }

        // NEW: Pagination methods implementation
        public async Task<IEnumerable<SaleDto>> GetPagedSalesAsync(int page, int pageSize, DateTime? startDate = null, DateTime? endDate = null, PaymentMethod? paymentMethod = null, string status = "")
        {
            try
            {
                // Validate pagination parameters
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 20;

                var sales = await _unitOfWork.Sales.GetPagedAsync(page, pageSize, startDate, endDate, paymentMethod, status);
                return _mapper.Map<IEnumerable<SaleDto>>(sales);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving paged sales: {ex.Message}", ex);
            }
        }

        public async Task<int> GetSalesCountAsync(DateTime? startDate = null, DateTime? endDate = null, PaymentMethod? paymentMethod = null, string status = "")
        {
            try
            {
                return await _unitOfWork.Sales.GetCountAsync(startDate, endDate, paymentMethod, status);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting sales count: {ex.Message}", ex);
            }
        }

        public async Task<PagedResult<SaleDto>> GetPagedSalesWithFiltersAsync(SalesFilterDto filters)
        {
            try
            {
                // Validate pagination parameters
                if (filters.Page < 1) filters.Page = 1;
                if (filters.PageSize < 1 || filters.PageSize > 100) filters.PageSize = 20;

                var totalCount = await GetSalesCountAsync(filters.StartDate, filters.EndDate, filters.PaymentMethod, filters.Status);
                var sales = await GetPagedSalesAsync(filters.Page, filters.PageSize, filters.StartDate, filters.EndDate, filters.PaymentMethod, filters.Status);

                return new PagedResult<SaleDto>
                {
                    Items = sales,
                    TotalCount = totalCount,
                    PageNumber = filters.Page,
                    PageSize = filters.PageSize
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving filtered sales: {ex.Message}", ex);
            }
        }
    }
}