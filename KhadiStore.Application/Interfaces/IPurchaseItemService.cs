using KhadiStore.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KhadiStore.Application.Interfaces
{
    public interface IPurchaseItemService
    {
        Task<IEnumerable<PurchaseItemDto>> GetPurchaseItemsAsync(int purchaseId);
        Task<IEnumerable<PurchaseItemDto>> GetPurchaseHistoryByProductAsync(int productId);
        Task<decimal> GetTotalPurchaseAmountByProductAsync(int productId, DateTime? startDate = null, DateTime? endDate = null);
        Task<int> GetTotalQuantityPurchasedAsync(int productId, DateTime? startDate = null, DateTime? endDate = null);
    }
}
