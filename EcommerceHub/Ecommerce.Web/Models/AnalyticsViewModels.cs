

namespace Ecommerce.Web.Models
{
    
        public class MonthlyRevenueViewsModel
        {
            public int OrderYear { get; set; }
            public string MonthName { get; set; } = "";
            public int TotalOrders { get; set; }
            public decimal TotalRevenue { get; set; }
            public decimal AverageOrderValue { get; set; }
            public decimal? RevenueGrowthPercentage { get; set; }
        }

        public class SalesByCategoryViewModel
        {
            public int CategoryID { get; set; }
            public string CategoryName { get; set; } = "";
            public int TotalUnitsSold { get; set; }
            public decimal TotalRevenue { get; set; }
        }

        public class RFMViewModel
        {
            public int UserID { get; set; }
            public string FullName { get; set; } = "";
            public string CustomerSegment { get; set; } = "";
        }

        public class BestSellingProductViewModel
        {
            public int ProductID { get; set; }
            public string ProductName { get; set; } = "";
            public int TotalUnitsSold { get; set; }
            public decimal TotalRevenue { get; set; }
        }

        public class CustomerRetentionViewModel
        {
            public int TotalCustomers { get; set; }
            public int RepeatCustomers { get; set; }
            public decimal RetentionRatePercentage { get; set; }
        }

        public class OverallAOVViewModel
        {
            public int TotalOrders { get; set; }
            public decimal TotalRevenue { get; set; }
            public decimal AverageOrderValue { get; set; }
        }

        // Ye ek "wrapper" hai - Dashboard View ko ek hi jagah sab data bhejne ke liye
        // (5-6 alag Model bhejne ke bajaye, sab ek ViewModel mein pack kar diya)
        public class DashboardViewModel
        {
            public List<MonthlyRevenueViewsModel> MonthlyRevenue { get; set; } = new();
            public List<SalesByCategoryViewModel> SalesByCategory { get; set; } = new();
            public List<RFMViewModel> RFMData { get; set; } = new();
            public List<BestSellingProductViewModel> BestSellingProducts { get; set; } = new();
            public CustomerRetentionViewModel? Retention { get; set; }
            public OverallAOVViewModel? AOV { get; set; }
        }

    
}
