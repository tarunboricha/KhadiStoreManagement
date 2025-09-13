using AutoMapper;
using KhadiStore.Application.DTOs;
using KhadiStore.Domain.Interfaces;

namespace KhadiStore.Application.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public DashboardService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<DashboardStatsDto> GetDashboardStatsAsync()
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            var stats = new DashboardStatsDto
            {
                TodaySales = await _unitOfWork.Sales.GetTotalSalesAsync(today, tomorrow),
                TodayOrders = (await _unitOfWork.Sales.GetTodaysSalesAsync()).Count(),
                LowStockItems = (await _unitOfWork.Products.GetLowStockProductsAsync()).Count(),
                TotalCustomers = await _unitOfWork.Customers.CountAsync(),
                TotalInventoryValue = await _unitOfWork.Products.GetTotalInventoryValueAsync(),
                TotalProducts = await _unitOfWork.Products.CountAsync()
            };

            return stats;
        }

        public async Task<SalesChartDto> GetSalesChartDataAsync(int days = 7)
        {
            var startDate = DateTime.Today.AddDays(-days);
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

        public async Task<IEnumerable<TopProductDto>> GetTopProductsAsync(int count = 5)
        {
            // This would require a more complex query to get actual top-selling products
            // For now, return empty list - can be implemented with raw SQL or stored procedure
            return new List<TopProductDto>();
        }

        public async Task<IEnumerable<LowStockProductDto>> GetLowStockProductsAsync()
        {
            var products = await _unitOfWork.Products.GetLowStockProductsAsync();
            return _mapper.Map<IEnumerable<LowStockProductDto>>(products);
        }
    }
}