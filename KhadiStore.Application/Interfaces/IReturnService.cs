using KhadiStore.Application.DTOs;

namespace KhadiStore.Application.Interfaces
{
    public interface IReturnService
    {
        Task<IEnumerable<ReturnDto>> GetAllReturnsAsync();
        Task<ReturnDto?> GetReturnByIdAsync(int id);
        Task<IEnumerable<ReturnDto>> GetReturnsBySaleIdAsync(int saleId);
        Task<IEnumerable<ReturnDto>> GetReturnsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<int> CreateReturnAsync(CreateReturnDto createReturnDto);
        Task<bool> CanCreateReturnAsync(int saleId);
        Task<bool> ValidateReturnItemsAsync(int saleId, List<CreateReturnItemDto> returnItems);
        Task<Dictionary<int, int>> GetRemainingReturnableQuantitiesAsync(int saleId);
        Task<decimal> GetTotalReturnsAmountAsync(DateTime? startDate = null, DateTime? endDate = null);
    }
}