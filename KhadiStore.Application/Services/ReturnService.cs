using AutoMapper;
using KhadiStore.Application.DTOs;
using KhadiStore.Application.Interfaces;
using KhadiStore.Domain.Entities;
using KhadiStore.Domain.Interfaces;

namespace KhadiStore.Application.Services
{
    public class ReturnService : IReturnService
    {
        private readonly IReturnRepository _returnRepository;
        private readonly ISaleRepository _saleRepository;
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;

        public ReturnService(
            IReturnRepository returnRepository,
            ISaleRepository saleRepository,
            IProductRepository productRepository,
            IMapper mapper)
        {
            _returnRepository = returnRepository;
            _saleRepository = saleRepository;
            _productRepository = productRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ReturnDto>> GetAllReturnsAsync()
        {
            try
            {
                var returns = await _returnRepository.GetAllAsync();
                return _mapper.Map<IEnumerable<ReturnDto>>(returns);
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public async Task<ReturnDto?> GetReturnByIdAsync(int id)
        {
            try
            {
                var returnEntity = await _returnRepository.GetByIdAsync(id);
                return returnEntity == null ? null : _mapper.Map<ReturnDto>(returnEntity);
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Error retrieving return with id {ReturnId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<ReturnDto>> GetReturnsBySaleIdAsync(int saleId)
        {
            try
            {
                var returns = await _returnRepository.GetBySaleIdAsync(saleId);
                return _mapper.Map<IEnumerable<ReturnDto>>(returns);
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Error retrieving returns for sale {SaleId}", saleId);
                throw;
            }
        }

        public async Task<IEnumerable<ReturnDto>> GetReturnsByCustomerIdAsync(int customerId)
        {
            try
            {
                var returns = await _returnRepository.GetByCustomerIdAsync(customerId);
                return _mapper.Map<IEnumerable<ReturnDto>>(returns);
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Error retrieving returns for customer {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<int> CreateReturnAsync(CreateReturnDto createReturnDto)
        {
            try
            {
                // Validate the return request
                if (!await CanCreateReturnAsync(createReturnDto.SaleId))
                {
                    throw new InvalidOperationException("Return cannot be created for this sale");
                }

                if (!await ValidateReturnItemsAsync(createReturnDto.SaleId, createReturnDto.ReturnItems))
                {
                    throw new ArgumentException("Invalid return items");
                }

                // Get the original sale - use no-tracking query to avoid conflicts
                var sale = await _saleRepository.GetByIdAsync(createReturnDto.SaleId);
                if (sale == null)
                {
                    throw new ArgumentException("Sale not found");
                }

                // Generate return number
                var returnNumber = await _returnRepository.GenerateReturnNumberAsync();

                // Create return entity - immediately processed
                var returnEntity = new Return
                {
                    ReturnNumber = returnNumber,
                    SaleId = createReturnDto.SaleId,
                    CustomerId = sale.CustomerId,
                    ReturnDate = DateTime.Now,
                    ReturnReason = createReturnDto.ReturnReason,
                    RefundMethod = Enum.Parse<RefundMethod>(createReturnDto.RefundMethod, true),
                    RefundReference = createReturnDto.RefundReference ?? string.Empty,
                    Notes = createReturnDto.Notes ?? string.Empty,
                    IsProcessed = true, // Immediately processed
                    CreatedAt = DateTime.Now,
                    IsDeleted = false
                };

                // Process return items and calculate totals
                decimal subtotal = 0;
                decimal totalGST = 0;

                foreach (var itemDto in createReturnDto.ReturnItems)
                {
                    var originalSaleItem = sale.SaleItems.First(si => si.Id == itemDto.SaleItemId);

                    var returnItem = new ReturnItem
                    {
                        ProductId = itemDto.ProductId,
                        SaleItemId = itemDto.SaleItemId,
                        ProductName = originalSaleItem.ProductName,
                        ReturnQuantity = itemDto.ReturnQuantity,
                        UnitPrice = originalSaleItem.UnitPrice,
                        DiscountAmount = (originalSaleItem.DiscountAmount / originalSaleItem.Quantity) * itemDto.ReturnQuantity,
                        GSTRate = originalSaleItem.GSTRate,
                        CreatedAt = DateTime.Now,
                        IsDeleted = false
                    };

                    // Calculate line totals
                    var lineSubtotal = (returnItem.ReturnQuantity * returnItem.UnitPrice) - returnItem.DiscountAmount;
                    var lineGST = lineSubtotal * (returnItem.GSTRate / 100);
                    returnItem.GSTAmount = lineGST;
                    returnItem.LineTotal = lineSubtotal + lineGST;

                    subtotal += lineSubtotal;
                    totalGST += lineGST;

                    returnEntity.ReturnItems.Add(returnItem);
                }

                returnEntity.SubTotal = subtotal;
                returnEntity.GSTAmount = totalGST;
                returnEntity.TotalAmount = subtotal + totalGST;

                // Save return first
                var savedReturn = await _returnRepository.AddAsync(returnEntity);

                // Update inventory using atomic operations - SEQUENTIAL, not concurrent
                foreach (var returnItem in returnEntity.ReturnItems)
                {
                    try
                    {
                        // Use the atomic increment method to avoid DbContext conflicts
                        var success = await _productRepository.IncrementStockAsync(returnItem.ProductId, returnItem.ReturnQuantity);
                        
                    }
                    catch (Exception ex)
                    {
                        // Don't fail the entire return for inventory update issues
                        // The return is already saved, inventory can be manually corrected
                    }
                }

                return savedReturn.Id;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private async Task UpdateProductInventoryAsync(int productId, int quantityToAdd)
        {
            try
            {
                // Load product with explicit query to avoid context conflicts
                var product = await _productRepository.GetByIdAsync(productId);
                if (product != null)
                {
                    var oldQuantity = product.StockQuantity;
                    product.StockQuantity += quantityToAdd;

                    await _productRepository.UpdateAsync(product);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<IEnumerable<ReturnDto>> GetReturnsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var returns = await _returnRepository.GetByDateRangeAsync(startDate, endDate);
                return _mapper.Map<IEnumerable<ReturnDto>>(returns);
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Error retrieving returns for date range {StartDate} to {EndDate}", startDate, endDate);
                throw;
            }
        }

        public async Task<decimal> GetTotalReturnsAmountAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                return await _returnRepository.GetTotalReturnsAmountAsync(startDate, endDate);
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Error calculating total returns amount");
                throw;
            }
        }

        public async Task<ReturnSummaryDto> GetReturnSummaryAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var returns = startDate.HasValue && endDate.HasValue
                    ? await GetReturnsByDateRangeAsync(startDate.Value, endDate.Value)
                    : await GetAllReturnsAsync();

                // All returns are processed, so simplified summary
                var summary = new ReturnSummaryDto
                {
                    TotalReturns = returns.Count(),
                    TotalReturnAmount = returns.Sum(r => r.TotalAmount),
                    PendingReturns = 0, // No pending returns in simplified system
                    CompletedReturns = returns.Count(), // All returns are completed immediately
                    AverageReturnValue = returns.Any() ? returns.Average(r => r.TotalAmount) : 0,
                    ReturnReasonBreakdown = returns.GroupBy(r => r.ReturnReason)
                        .ToDictionary(g => g.Key, g => g.Count()),
                    RefundMethodBreakdown = returns.GroupBy(r => r.RefundMethod)
                        .ToDictionary(g => g.Key, g => g.Sum(r => r.TotalAmount))
                };

                return summary;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<bool> CanCreateReturnAsync(int saleId)
        {
            try
            {
                var sale = await _saleRepository.GetByIdAsync(saleId);
                if (sale == null)
                    return false;

                // Check if sale is completed
                if (sale.Status != SaleStatus.Completed)
                    return false;

                // Check if sale is within return window (30 days)
                var daysSinceSale = (DateTime.Now - sale.SaleDate).Days;
                if (daysSinceSale > 30)
                    return false;

                return true;
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Error validating return eligibility for sale {SaleId}", saleId);
                return false;
            }
        }

        public async Task<bool> ValidateReturnItemsAsync(int saleId, List<CreateReturnItemDto> returnItems)
        {
            try
            {
                var sale = await _saleRepository.GetByIdAsync(saleId);
                if (sale == null)
                {
                    //_logger.LogError("Sale {SaleId} not found during return validation", saleId);
                    return false;
                }

                // Get already returned quantities for this sale
                var returnedQuantities = await _returnRepository.GetReturnedQuantitiesForSaleAsync(saleId);

                //_logger.LogInformation("Validating return items for sale {SaleId}. Already returned quantities: {ReturnedQties}",saleId, string.Join(", ", returnedQuantities.Select(kv => $"SaleItem{kv.Key}:{kv.Value}")));

                foreach (var returnItem in returnItems)
                {
                    // Find the original sale item
                    var saleItem = sale.SaleItems.FirstOrDefault(si => si.Id == returnItem.SaleItemId);
                    if (saleItem == null)
                    {
                        //_logger.LogError("Sale item {SaleItemId} not found in sale {SaleId}", returnItem.SaleItemId, saleId);
                        return false;
                    }

                    // Calculate remaining returnable quantity
                    var originalQuantity = saleItem.Quantity;
                    var alreadyReturned = returnedQuantities.GetValueOrDefault(returnItem.SaleItemId, 0);
                    var remainingQuantity = originalQuantity - alreadyReturned;

                    //_logger.LogInformation("SaleItem {SaleItemId}: Original={Original}, AlreadyReturned={Returned}, Remaining={Remaining}, Requested={Requested}",returnItem.SaleItemId, originalQuantity, alreadyReturned, remainingQuantity, returnItem.ReturnQuantity);

                    // Validation checks
                    if (returnItem.ReturnQuantity <= 0)
                    {
                        //_logger.LogError("Invalid return quantity {Qty} for sale item {SaleItemId}", returnItem.ReturnQuantity, returnItem.SaleItemId);
                        return false;
                    }

                    if (remainingQuantity <= 0)
                    {
                        //_logger.LogError("Sale item {SaleItemId} is already fully returned (Original: {Original}, Returned: {Returned})", returnItem.SaleItemId, originalQuantity, alreadyReturned);
                        return false;
                    }

                    if (returnItem.ReturnQuantity > remainingQuantity)
                    {
                        //_logger.LogError("Requested return quantity {Requested} exceeds remaining quantity {Remaining} for sale item {SaleItemId}", returnItem.ReturnQuantity, remainingQuantity, returnItem.SaleItemId);
                        return false;
                    }

                    // Check if product exists
                    var product = await _productRepository.GetByIdAsync(returnItem.ProductId);
                    if (product == null)
                    {
                        //_logger.LogError("Product {ProductId} not found", returnItem.ProductId);
                        return false;
                    }
                }

                //_logger.LogInformation("All return items validation passed for sale {SaleId}", saleId);
                return true;
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Error validating return items for sale {SaleId}", saleId);
                return false;
            }
        }

        // ALSO ADD this new method to get remaining quantities for UI
        public async Task<Dictionary<int, int>> GetRemainingReturnableQuantitiesAsync(int saleId)
        {
            try
            {
                var sale = await _saleRepository.GetByIdAsync(saleId);
                if (sale == null)
                    return new Dictionary<int, int>();

                var returnedQuantities = await _returnRepository.GetReturnedQuantitiesForSaleAsync(saleId);
                var remainingQuantities = new Dictionary<int, int>();

                foreach (var saleItem in sale.SaleItems)
                {
                    var originalQuantity = saleItem.Quantity;
                    var alreadyReturned = returnedQuantities.GetValueOrDefault(saleItem.Id, 0);
                    var remaining = originalQuantity - alreadyReturned;

                    remainingQuantities[saleItem.Id] = Math.Max(0, remaining);
                }

                return remainingQuantities;
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Error getting remaining returnable quantities for sale {SaleId}", saleId);
                return new Dictionary<int, int>();
            }
        }
    }
}