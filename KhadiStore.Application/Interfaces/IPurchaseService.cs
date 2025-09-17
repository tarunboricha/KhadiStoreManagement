using KhadiStore.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KhadiStore.Application.Interfaces
{
    public interface IPurchaseService
    {
        // Basic CRUD operations
        Task<IEnumerable<PurchaseDto>> GetAllPurchasesAsync();
        Task<PurchaseDto?> GetPurchaseByIdAsync(int id);
        Task<PurchaseDto> CreatePurchaseAsync(CreatePurchaseDto createPurchaseDto);
        Task<bool> UpdatePurchaseStatusAsync(int purchaseId, string newStatus, string? reason = null);
        Task<bool> DeletePurchaseAsync(int id);

        // Purchase workflow
        Task<bool> ReceivePurchaseAsync(int purchaseId, string? notes = null);
        Task<bool> CancelPurchaseAsync(int purchaseId, string? reason = null);
        Task<string> GeneratePurchaseOrderNumberAsync();

        // Search and filtering
        Task<IEnumerable<PurchaseDto>> GetPurchasesBySupplierAsync(int supplierId);
        Task<IEnumerable<PurchaseDto>> GetPurchasesByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<PurchaseDto>> GetPurchasesByStatusAsync(string status);
        Task<PurchaseDto?> GetPurchaseByOrderNumberAsync(string orderNumber);
        Task<IEnumerable<PurchaseDto>> GetPendingReceivalsAsync();

        // Pagination
        Task<PagedResult<PurchaseDto>> GetPagedPurchasesAsync(PurchaseFilterDto filter);
        Task<int> GetPurchasesCountAsync(PurchaseFilterDto filter);

        // Statistics and reporting
        Task<decimal> GetTotalPurchaseAmountAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<int> GetTotalPurchasesCountAsync();
        Task<int> GetPendingPurchasesCountAsync();
        Task<decimal> GetMonthlyPurchaseAmountAsync(int year, int month);
        Task<IEnumerable<object>> GetPurchaseChartDataAsync(int days = 30);
        Task<IEnumerable<PurchaseDto>> GetRecentPurchasesAsync(int count = 10);

        // Validation
        Task<bool> CanChangeStatusAsync(int purchaseId, string newStatus);
        Task<bool> PurchaseOrderNumberExistsAsync(string orderNumber, int excludeId = 0);
    }
}
