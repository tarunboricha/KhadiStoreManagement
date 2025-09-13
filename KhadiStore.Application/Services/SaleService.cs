using AutoMapper;
using KhadiStore.Application.DTOs;
using KhadiStore.Domain.Entities;
using KhadiStore.Domain.Interfaces;
using System.Numerics;
using System.Xml.Linq;

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
            sale.DiscountAmount =  (subTotal + gstTotal) * createSaleDto.BillDiscountPercentage / 100;
            sale.TotalAmount = subTotal + gstTotal - sale.DiscountAmount;

            var createdSale = await _unitOfWork.Sales.AddAsync(sale);

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<SaleDto>(createdSale);
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
    }
}