namespace KhadiStore.Application.DTOs
{
    public class DashboardStatsDto
    {
        public decimal TodaySales { get; set; }
        public int TodayOrders { get; set; }
        public int LowStockItems { get; set; }
        public int TotalCustomers { get; set; }
        public decimal TotalInventoryValue { get; set; }
        public int TotalProducts { get; set; }
    }

    public class SalesChartDto
    {
        public List<string> Labels { get; set; } = new List<string>();
        public List<decimal> Values { get; set; } = new List<decimal>();
    }

    public class TopProductDto
    {
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int QuantitySold { get; set; }
        public decimal Revenue { get; set; }
    }

    public class LowStockProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int Stock { get; set; }
        public int MinStockLevel { get; set; }
    }
}