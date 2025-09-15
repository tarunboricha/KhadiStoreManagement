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
        private readonly ICustomerRepository _customerRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public ReturnService(
            IReturnRepository returnRepository,
            ISaleRepository saleRepository,
            IProductRepository productRepository,
            ICustomerRepository customerRepository,
            IMapper mapper,
            IUnitOfWork unitOfWork)
        {
            _returnRepository = returnRepository;
            _saleRepository = saleRepository;
            _productRepository = productRepository;
            _customerRepository = customerRepository;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
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
                //_logger.LogError(ex, "Error retrieving all returns");
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

                // Get the original sale
                var sale = await _saleRepository.GetByIdAsync(createReturnDto.SaleId);
                if (sale == null)
                {
                    throw new ArgumentException("Sale not found");
                }

                // Generate return number
                var returnNumber = await _returnRepository.GenerateReturnNumberAsync();

                // Calculate bill-level discount percentage from original sale
                var originalBillDiscountPercentage = 0m;
                if (sale.SubTotal + sale.GSTAmount > 0)
                {
                    originalBillDiscountPercentage = (sale.DiscountAmount / (sale.SubTotal + sale.GSTAmount)) * 100;
                }

                // Create return entity
                var returnEntity = new Return
                {
                    ReturnNumber = returnNumber,
                    SaleId = createReturnDto.SaleId,
                    CustomerId = sale.CustomerId,
                    ReturnDate = DateTime.Now,
                    ReturnReason = createReturnDto.ReturnReason,
                    RefundMethod = createReturnDto.RefundMethod,
                    RefundReference = createReturnDto.RefundReference ?? string.Empty,
                    AdditionalNotes = createReturnDto.AdditionalNotes ?? string.Empty,
                    Status = "Completed",
                    CreatedAt = DateTime.Now,
                    IsDeleted = false
                };

                // Process return items and calculate totals using proper bill-level discount logic
                decimal subtotal = 0;
                decimal totalGST = 0;

                foreach (var itemDto in createReturnDto.ReturnItems)
                {
                    var originalSaleItem = sale.SaleItems.First(si => si.Id == itemDto.SaleItemId);

                    // Calculate line subtotal (quantity × unit price)
                    var lineSubtotal = itemDto.ReturnQuantity * originalSaleItem.UnitPrice;

                    // Calculate GST on the line subtotal
                    var lineGST = lineSubtotal * (originalSaleItem.GSTRate / 100);

                    // Calculate total before discount for this line
                    var totalBeforeDiscount = lineSubtotal + lineGST;

                    // Apply proportional bill-level discount
                    var lineDiscountAmount = totalBeforeDiscount * (originalBillDiscountPercentage / 100);

                    var returnItem = new ReturnItem
                    {
                        ProductId = itemDto.ProductId,
                        SaleItemId = itemDto.SaleItemId,
                        ProductName = originalSaleItem.ProductName,
                        ReturnQuantity = itemDto.ReturnQuantity,
                        UnitPrice = originalSaleItem.UnitPrice,
                        DiscountAmount = lineDiscountAmount,
                        GSTRate = originalSaleItem.GSTRate,
                        GSTAmount = lineGST,
                        LineTotal = totalBeforeDiscount - lineDiscountAmount,
                        CreatedAt = DateTime.Now,
                        IsDeleted = false
                    };

                    subtotal += lineSubtotal;
                    totalGST += lineGST;

                    returnEntity.ReturnItems.Add(returnItem);
                }

                // Set return totals
                returnEntity.SubTotal = subtotal;
                returnEntity.GSTAmount = totalGST;
                var totalBeforeDiscountForReturn = subtotal + totalGST;
                returnEntity.DiscountAmount = totalBeforeDiscountForReturn * (originalBillDiscountPercentage / 100);
                returnEntity.TotalAmount = totalBeforeDiscountForReturn - returnEntity.DiscountAmount;

                // Save return
                var savedReturn = await _returnRepository.AddAsync(returnEntity);

                // Update inventory for each return item
                foreach (var returnItem in returnEntity.ReturnItems)
                {
                    try
                    {
                        await _productRepository.IncrementStockAsync(returnItem.ProductId, returnItem.ReturnQuantity);
                    }
                    catch (Exception ex)
                    {
                        // Don't fail the entire return for inventory issues
                    }
                }

                // Update customer statistics if customer exists
                if (sale.CustomerId.HasValue)
                {
                    try
                    {
                        await UpdateCustomerStatisticsForReturnAsync(sale.CustomerId.Value, returnEntity.TotalAmount);
                    }
                    catch (Exception ex)
                    {
                        //_logger.LogError(ex, "Failed to update customer statistics for customer {CustomerId}", sale.CustomerId.Value);
                    }
                }

                //_logger.LogInformation("Return {ReturnId} created successfully for sale {SaleId}", savedReturn.Id, createReturnDto.SaleId);

                return savedReturn.Id;
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Error creating return for sale {SaleId}", createReturnDto.SaleId);
                throw;
            }
        }

        public async Task<bool> CanCreateReturnAsync(int saleId)
        {
            try
            {
                var sale = await _saleRepository.GetByIdAsync(saleId);
                if (sale == null) return false;

                // Check if sale is completed
                if (sale.Status != SaleStatus.Completed) return false;

                // Check if sale is within return window (30 days)
                var daysSinceSale = (DateTime.Now - sale.SaleDate).Days;
                if (daysSinceSale > 30) return false;

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

                    // Validation checks
                    if (returnItem.ReturnQuantity <= 0)
                    {
                        //_logger.LogError("Invalid return quantity {Qty} for sale item {SaleItemId}", returnItem.ReturnQuantity, returnItem.SaleItemId);
                        return false;
                    }

                    if (remainingQuantity <= 0)
                    {
                        //_logger.LogError("Sale item {SaleItemId} is already fully returned", returnItem.SaleItemId);
                        return false;
                    }

                    if (returnItem.ReturnQuantity > remainingQuantity)
                    {
                        //_logger.LogError("Requested return quantity {Requested} exceeds remaining quantity {Remaining} for sale item {SaleItemId}",returnItem.ReturnQuantity, remainingQuantity, returnItem.SaleItemId);
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Error validating return items for sale {SaleId}", saleId);
                return false;
            }
        }

        public async Task<Dictionary<int, int>> GetRemainingReturnableQuantitiesAsync(int saleId)
        {
            try
            {
                var sale = await _saleRepository.GetByIdAsync(saleId);
                if (sale == null) return new Dictionary<int, int>();

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

        private async Task UpdateCustomerStatisticsForReturnAsync(int customerId, decimal returnAmount)
        {
            try
            {
                var customer = await _customerRepository.GetByIdAsync(customerId);
                if (customer != null)
                {
                    customer.TotalOrders = Math.Max(0, customer.TotalOrders - 1);
                    customer.TotalPurchases = Math.Max(0, customer.TotalPurchases - returnAmount);

                    await _customerRepository.UpdateAsync(customer);
                }
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Error updating customer statistics for return");
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
    }
}