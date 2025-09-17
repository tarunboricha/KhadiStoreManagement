using AutoMapper;
using KhadiStore.Application.DTOs;
using KhadiStore.Application.Interfaces;
using KhadiStore.Domain.Entities;
using KhadiStore.Domain.Interfaces;


namespace KhadiStore.Application.Services
{
    public class PurchaseService : IPurchaseService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public PurchaseService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<PurchaseDto>> GetAllPurchasesAsync()
        {
            try
            {
                var purchases = await _unitOfWork.Purchases.GetAllAsync();
                return _mapper.Map<IEnumerable<PurchaseDto>>(purchases);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving purchases: {ex.Message}", ex);
            }
        }

        public async Task<PurchaseDto?> GetPurchaseByIdAsync(int id)
        {
            try
            {
                var purchase = await _unitOfWork.Purchases.GetByIdAsync(id);
                return purchase != null ? _mapper.Map<PurchaseDto>(purchase) : null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving purchase: {ex.Message}", ex);
            }
        }

        public async Task<PurchaseDto> CreatePurchaseAsync(CreatePurchaseDto createPurchaseDto)
        {
            try
            {
                var purchase = _mapper.Map<Purchase>(createPurchaseDto);
                purchase.PurchaseOrderNumber = await GeneratePurchaseOrderNumberAsync();
                purchase.CreatedAt = DateTime.UtcNow;
                purchase.Status = PurchaseStatus.Ordered;

                // Calculate totals
                decimal subTotal = 0;
                decimal gstTotal = 0;

                foreach (var item in purchase.PurchaseItems)
                {
                    var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId);
                    if (product != null)
                    {
                        item.ProductName = product.Name;
                        item.GSTRate = product.GST ?? 5.0m;

                        var itemSubtotal = item.UnitPrice * item.Quantity;
                        var gstAmount = itemSubtotal * (item.GSTRate / 100);

                        item.GSTAmount = gstAmount;
                        item.TotalAmount = itemSubtotal + gstAmount;

                        subTotal += itemSubtotal;
                        gstTotal += gstAmount;
                    }
                }

                purchase.SubTotal = subTotal;
                purchase.GSTAmount = gstTotal;
                purchase.TotalAmount = subTotal + gstTotal;

                var createdPurchase = await _unitOfWork.Purchases.AddAsync(purchase);
                await _unitOfWork.SaveChangesAsync();

                return _mapper.Map<PurchaseDto>(createdPurchase);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating purchase: {ex.Message}", ex);
            }
        }

        public async Task<bool> UpdatePurchaseStatusAsync(int purchaseId, string newStatus, string? reason = null)
        {
            try
            {
                if (!await CanChangeStatusAsync(purchaseId, newStatus))
                    return false;

                var purchase = await _unitOfWork.Purchases.GetByIdAsync(purchaseId);
                if (purchase == null)
                    return false;

                if (!Enum.TryParse<PurchaseStatus>(newStatus, true, out var status))
                    return false;

                await _unitOfWork.BeginTransactionAsync();

                var oldStatus = purchase.Status;
                purchase.Status = status;

                // Add status change note
                if (!string.IsNullOrWhiteSpace(reason))
                {
                    var statusChangeNote = $"\n[{DateTime.Now:yyyy-MM-dd HH:mm}] Status changed from {oldStatus} to {newStatus}. Reason: {reason}";
                    purchase.Notes = (purchase.Notes ?? "") + statusChangeNote;
                }

                // Handle status-specific logic
                if (status == PurchaseStatus.Received)
                {
                    // Update product stock
                    foreach (var item in purchase.PurchaseItems)
                    {
                        await _unitOfWork.Products.UpdateStockAsync(item.ProductId, item.Quantity);
                    }
                }
                else if (oldStatus == PurchaseStatus.Received && status != PurchaseStatus.Received)
                {
                    // Reverse stock update if changing from Received to other status
                    foreach (var item in purchase.PurchaseItems)
                    {
                        await _unitOfWork.Products.UpdateStockAsync(item.ProductId, -item.Quantity);
                    }
                }

                await _unitOfWork.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating purchase status: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeletePurchaseAsync(int id)
        {
            try
            {
                var purchase = await _unitOfWork.Purchases.GetByIdAsync(id);
                if (purchase == null)
                    return false;

                // Can only delete if not received
                if (purchase.Status == PurchaseStatus.Received)
                {
                    throw new InvalidOperationException("Cannot delete a received purchase. Please change status first.");
                }

                await _unitOfWork.BeginTransactionAsync();
                await _unitOfWork.Purchases.DeleteAsync(purchase.Id);
                await _unitOfWork.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting purchase: {ex.Message}", ex);
            }
        }

        public async Task<bool> ReceivePurchaseAsync(int purchaseId, string? notes = null)
        {
            return await UpdatePurchaseStatusAsync(purchaseId, "Received", notes ?? "Purchase received and stock updated");
        }

        public async Task<bool> CancelPurchaseAsync(int purchaseId, string? reason = null)
        {
            return await UpdatePurchaseStatusAsync(purchaseId, "Cancelled", reason ?? "Purchase cancelled");
        }

        public async Task<string> GeneratePurchaseOrderNumberAsync()
        {
            try
            {
                return await _unitOfWork.Purchases.GeneratePurchaseOrderNumberAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error generating purchase order number: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<PurchaseDto>> GetPurchasesBySupplierAsync(int supplierId)
        {
            try
            {
                var purchases = await _unitOfWork.Purchases.GetBySupplierAsync(supplierId);
                return _mapper.Map<IEnumerable<PurchaseDto>>(purchases);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving purchases by supplier: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<PurchaseDto>> GetPurchasesByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var purchases = await _unitOfWork.Purchases.GetByDateRangeAsync(startDate, endDate);
                return _mapper.Map<IEnumerable<PurchaseDto>>(purchases);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving purchases by date range: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<PurchaseDto>> GetPurchasesByStatusAsync(string status)
        {
            try
            {
                if (!Enum.TryParse<PurchaseStatus>(status, true, out var purchaseStatus))
                    return new List<PurchaseDto>();

                var purchases = await _unitOfWork.Purchases.GetByStatusAsync(purchaseStatus);
                return _mapper.Map<IEnumerable<PurchaseDto>>(purchases);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving purchases by status: {ex.Message}", ex);
            }
        }

        public async Task<PurchaseDto?> GetPurchaseByOrderNumberAsync(string orderNumber)
        {
            try
            {
                var purchase = await _unitOfWork.Purchases.GetByPurchaseOrderNumberAsync(orderNumber);
                return purchase != null ? _mapper.Map<PurchaseDto>(purchase) : null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving purchase by order number: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<PurchaseDto>> GetPendingReceivalsAsync()
        {
            try
            {
                var purchases = await _unitOfWork.Purchases.GetPendingReceivalsAsync();
                return _mapper.Map<IEnumerable<PurchaseDto>>(purchases);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving pending receivals: {ex.Message}", ex);
            }
        }

        public async Task<PagedResult<PurchaseDto>> GetPagedPurchasesAsync(PurchaseFilterDto filter)
        {
            try
            {
                var totalCount = await _unitOfWork.Purchases.GetCountAsync(filter.StartDate, filter.EndDate, filter.SupplierId, filter.Status, filter.PurchaseOrderNumber);
                var purchases = await _unitOfWork.Purchases.GetPagedAsync(filter.Page, filter.PageSize, filter.StartDate, filter.EndDate, filter.SupplierId, filter.Status, filter.PurchaseOrderNumber);
                var purchaseDtos = _mapper.Map<IEnumerable<PurchaseDto>>(purchases);

                return new PagedResult<PurchaseDto>
                {
                    Items = purchaseDtos,
                    TotalCount = totalCount,
                    PageNumber = filter.Page,
                    PageSize = filter.PageSize
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving paged purchases: {ex.Message}", ex);
            }
        }

        public async Task<int> GetPurchasesCountAsync(PurchaseFilterDto filter)
        {
            try
            {
                return await _unitOfWork.Purchases.GetCountAsync(filter.StartDate, filter.EndDate, filter.SupplierId, filter.Status, filter.PurchaseOrderNumber);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting purchases count: {ex.Message}", ex);
            }
        }

        public async Task<decimal> GetTotalPurchaseAmountAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                return await _unitOfWork.Purchases.GetTotalPurchaseAmountAsync(startDate, endDate);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting total purchase amount: {ex.Message}", ex);
            }
        }

        public async Task<int> GetTotalPurchasesCountAsync()
        {
            try
            {
                return await _unitOfWork.Purchases.GetTotalPurchasesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting total purchases count: {ex.Message}", ex);
            }
        }

        public async Task<int> GetPendingPurchasesCountAsync()
        {
            try
            {
                return await _unitOfWork.Purchases.GetPendingPurchasesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting pending purchases count: {ex.Message}", ex);
            }
        }

        public async Task<decimal> GetMonthlyPurchaseAmountAsync(int year, int month)
        {
            try
            {
                return await _unitOfWork.Purchases.GetMonthlyPurchaseAmountAsync(year, month);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting monthly purchase amount: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<object>> GetPurchaseChartDataAsync(int days = 30)
        {
            try
            {
                return await _unitOfWork.Purchases.GetPurchaseChartDataAsync(days);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting purchase chart data: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<PurchaseDto>> GetRecentPurchasesAsync(int count = 10)
        {
            try
            {
                var purchases = await _unitOfWork.Purchases.GetRecentPurchasesAsync(count);
                return _mapper.Map<IEnumerable<PurchaseDto>>(purchases);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving recent purchases: {ex.Message}", ex);
            }
        }

        public async Task<bool> CanChangeStatusAsync(int purchaseId, string newStatus)
        {
            try
            {
                var purchase = await _unitOfWork.Purchases.GetByIdAsync(purchaseId);
                if (purchase == null)
                    return false;

                if (!Enum.TryParse<PurchaseStatus>(newStatus, true, out var targetStatus))
                    return false;

                return purchase.Status switch
                {
                    PurchaseStatus.Ordered => targetStatus is PurchaseStatus.Received or PurchaseStatus.Cancelled or PurchaseStatus.Ordered,
                    PurchaseStatus.Received => targetStatus is PurchaseStatus.Received,
                    PurchaseStatus.Cancelled => targetStatus is PurchaseStatus.Ordered or PurchaseStatus.Cancelled,
                    _ => false
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error checking status change capability: {ex.Message}", ex);
            }
        }

        public async Task<bool> PurchaseOrderNumberExistsAsync(string orderNumber, int excludeId = 0)
        {
            try
            {
                var purchase = await _unitOfWork.Purchases.GetByPurchaseOrderNumberAsync(orderNumber);
                return purchase != null && purchase.Id != excludeId;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error checking purchase order number existence: {ex.Message}", ex);
            }
        }
    }
}
