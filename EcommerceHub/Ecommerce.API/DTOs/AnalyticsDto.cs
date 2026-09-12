namespace Ecommerce.API.DTOs
{
    public class MonthlyRevenueDto
    {
        public int OrderYear { get; set; }
        public string MonthName { get; set; }
        public int TotalOrders { get; set; }
        public int TotalCustomers { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageOrderValue { get; set; }
        public decimal? PreviousMonthRevenue { get; set; }
        public decimal? RevenueGrowthPercentage { get; set; }

    }

    public class TopRepeatCustomerDto
    {
        public long CustomerRank { get; set; }
        public int UserID { get; set; }
        public string FullName { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
        public string CustomerSegment { get; set; }
    }

    public class CustomerLifetimeValueDto
    {
        public int UserID { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public int TotalOrders { get; set; }
        public decimal LifetimeValue { get; set; }
        public decimal AverageOrderValue { get; set; }
        public DateTime? FirstOrderDate { get; set; }
        public DateTime? LastOrderDate { get; set; }
        public int CustomerLifespanDays { get; set; }
    }

    public class RFMAnalysisDto
    {
        public int UserID { get; set; }
        public string FullName { get; set; }
        public int Recency { get; set; }
        public int Frequency { get; set; }
        public decimal Monetary { get; set; }
        public long RecencyScore { get; set; }
        public long FrequencyScore { get; set; }
        public long MonetaryScore { get; set; }
        public string CustomerSegment { get; set; }
    }

    public class CustomerRetentionDto
    {
        public int TotalCustomers { get; set; }
        public int RepeatCustomers { get; set; }
        public int OneTimeCustomers { get; set; }
        public decimal RetentionRatePercentage { get; set; }
    }

    public class OverallAOVDto
    {
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageOrderValue { get; set; }
        public decimal SmallestOrder { get; set; }
        public decimal LargestOrder { get; set; }
    }

    public class SalesByCategoryDto
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
        public int TotalOrders { get; set; }
        public int TotalUnitsSold { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class SalesByBrandDto
    {
        public int BrandID { get; set; }
        public string BrandName { get; set; }
        public int TotalOrders { get; set; }
        public int TotalUnitsSold { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class BestSellingProductDto
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public int TotalUnitsSold { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class LeastSellingProductDto
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public int TotalUnitsSold { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class DeadStockDto
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public int StockQuantity { get; set; }
        public decimal Price { get; set; }
        public decimal CapitalStuck { get; set; }
        public DateTime? LastSoldDate { get; set; }
        public int DaysSinceLastSale { get; set; }
    }

    public class CouponUsageDto
    {
        public int CouponID { get; set; }
        public string CouponCode { get; set; }
        public string DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public int TimesUsed { get; set; }
        public decimal? TotalDiscountGiven { get; set; }
        public decimal? AvgDiscountPerOrder { get; set; }
    }

    public class CancellationReturnDto
    {
        public int TotalOrders { get; set; }
        public int CancelledOrders { get; set; }
        public int TotalReturns { get; set; }
        public decimal CancellationRatePercentage { get; set; }
        public decimal TotalRefundAmount { get; set; }
    }
}
