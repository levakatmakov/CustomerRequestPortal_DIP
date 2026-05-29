using CustomerRequestPortal.Models;

namespace CustomerRequestPortal.ViewModels
{
    public class AnalyticsViewModel
    {
        public int TotalRequests { get; set; }
        public int NewRequests { get; set; }
        public int InWorkRequests { get; set; }
        public int DoneRequests { get; set; }
        public int RejectedRequests { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal MonthRevenue { get; set; }
        public int ActiveProducts { get; set; }
        public int LowStockProductsCount { get; set; }
        public List<StatusSummaryItem> StatusSummary { get; set; } = new();
        public List<PopularProductItem> PopularProducts { get; set; } = new();
        public List<Product> LowStockProducts { get; set; } = new();
        public List<MonthlyRevenueItem> MonthlyRevenue { get; set; } = new();
    }

    public class StatusSummaryItem
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal Percent { get; set; }
    }

    public class PopularProductItem
    {
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Amount { get; set; }
    }

    public class MonthlyRevenueItem
    {
        public string Month { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public int RequestsCount { get; set; }
    }
}
