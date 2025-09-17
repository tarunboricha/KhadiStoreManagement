using AutoMapper;
using KhadiStore.Application.DTOs;
using KhadiStore.Application.Interfaces;
using KhadiStore.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KhadiStore.Application.Services
{
    public class PurchaseItemService : IPurchaseItemService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public PurchaseItemService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<PurchaseItemDto>> GetPurchaseItemsAsync(int purchaseId)
        {
            try
            {
                var items = await _unitOfWork.PurchaseItems.GetByPurchaseIdAsync(purchaseId);
                return _mapper.Map<IEnumerable<PurchaseItemDto>>(items);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving purchase items: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<PurchaseItemDto>> GetPurchaseHistoryByProductAsync(int productId)
        {
            try
            {
                var items = await _unitOfWork.PurchaseItems.GetByProductIdAsync(productId);
                return _mapper.Map<IEnumerable<PurchaseItemDto>>(items);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving purchase history: {ex.Message}", ex);
            }
        }

        public async Task<decimal> GetTotalPurchaseAmountByProductAsync(int productId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                return await _unitOfWork.PurchaseItems.GetTotalPurchaseAmountByProductAsync(productId, startDate, endDate);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting total purchase amount by product: {ex.Message}", ex);
            }
        }

        public async Task<int> GetTotalQuantityPurchasedAsync(int productId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                return await _unitOfWork.PurchaseItems.GetTotalQuantityPurchasedAsync(productId, startDate, endDate);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting total quantity purchased: {ex.Message}", ex);
            }
        }
    }
}
