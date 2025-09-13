using KhadiStore.Application.DTOs;

namespace KhadiStore.Application.Interfaces
{
    public interface IReturnService
    {
        // Core CRUD operations
        Task<IEnumerable<ReturnDto>> GetAllReturnsAsync();
        Task<ReturnDto?> GetReturnByIdAsync(int id);
        Task<IEnumerable<ReturnDto>> GetReturnsBySaleIdAsync(int saleId);
        Task<IEnumerable<ReturnDto>> GetReturnsByCustomerIdAsync(int customerId);

        // Return processing - simplified to immediate processing
        Task<int> CreateReturnAsync(CreateReturnDto createReturnDto);

        // Filtering and searching
        Task<IEnumerable<ReturnDto>> GetReturnsByDateRangeAsync(DateTime startDate, DateTime endDate);

        // Analytics and reporting
        Task<decimal> GetTotalReturnsAmountAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<ReturnSummaryDto> GetReturnSummaryAsync(DateTime? startDate = null, DateTime? endDate = null);

        // Validation
        Task<bool> CanCreateReturnAsync(int saleId);
        Task<bool> ValidateReturnItemsAsync(int saleId, List<CreateReturnItemDto> returnItems);

        // Get remaining returnable quantities for UI
        Task<Dictionary<int, int>> GetRemainingReturnableQuantitiesAsync(int saleId);
    }
}