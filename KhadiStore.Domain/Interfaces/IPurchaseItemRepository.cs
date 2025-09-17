using KhadiStore.Domain.Entities;
using KhadiStore.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KhadiStore.Application.Interfaces
{
    public interface IPurchaseItemRepository
    {
        Task<IEnumerable<PurchaseItem>> GetByPurchaseIdAsync(int purchaseId);
        Task<IEnumerable<PurchaseItem>> GetByProductIdAsync(int productId);
        Task<decimal> GetTotalPurchaseAmountByProductAsync(int productId, DateTime? startDate = null, DateTime? endDate = null);
        Task<int> GetTotalQuantityPurchasedAsync(int productId, DateTime? startDate = null, DateTime? endDate = null);
    }
}
