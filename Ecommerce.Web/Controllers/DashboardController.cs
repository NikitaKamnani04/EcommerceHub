using Microsoft.AspNetCore.Mvc;
using Ecommerce.Web.Services;
using Ecommerce.Web.Models;


namespace Ecommerce.Web.Controllers
{
    // Ye controller sirf Admin ke liye hai - dashboard, product management, etc.
    public class DashboardController : Controller
    {
        private readonly ApiService _apiService;

        public DashboardController(ApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<IActionResult> Index()
        {
            // SECURITY CHECK: Agar koi Customer directly URL type karke 
            // "/Dashboard/Index" pe aane ki koshish kare, use rok do
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin")
            {
                // Admin nahi hai - Homepage pe wapas bhej do
                return RedirectToAction("Index", "Home");
            }


            // Session se token nikaalo - Analytics endpoints ko call karne ke liye chahiye
            var token = HttpContext.Session.GetString("JWToken");

            //// Saare Analytics endpoints ek saath call karo
            // Har GetAsync call token bhi bhej rahi hai (kyunki Analytics Admin-only hai)
            var dashboard = new DashboardViewModel
            {
                MonthlyRevenue = await _apiService.GetAsync<List<MonthlyRevenueViewsModel>>(
                    "api/Analytics/monthly-revenue", token) ?? new(),
                SalesByCategory = await _apiService.GetAsync<List<SalesByCategoryViewModel>>(
                    "api/Analytics/sales-by-category", token) ?? new(),

                RFMData = await _apiService.GetAsync<List<RFMViewModel>>(
                    "api/Analytics/rfm-analysis", token) ?? new(),

                BestSellingProducts = await _apiService.GetAsync<List<BestSellingProductViewModel>>(
                    "api/Analytics/best-selling-products", token) ?? new(),

                Retention = await _apiService.GetAsync<CustomerRetentionViewModel>(
                    "api/Analytics/customer-retention", token),

                AOV = await _apiService.GetAsync<OverallAOVViewModel>(
                    "api/Analytics/overall-aov", token)
            };
            // ================== NAYA CODE - INSIGHTS GENERATE KARNA ==================
            var insights = new List<string>();

            // Insight 1: Revenue growth (agar kam se kam 2 mahine ka data hai)
            if(dashboard.MonthlyRevenue.Count >=2)
            {
                // Sabse latest mahine ka growth % nikaalo (jo already proc se calculate hokar aata hai)
                var latestMonth = dashboard.MonthlyRevenue.Last();
                if (latestMonth.RevenueGrowthPercentage.HasValue)
                {
                    var growth = latestMonth.RevenueGrowthPercentage.Value;
                    if(growth>0)
                    {
                        insights.Add($"Revenue increased {growth:0.0}% compared to last month.");
                    }
                    else if(growth<0)
                    {
                        insights.Add($"Revenue decreased {Math.Abs(growth):0.0}% compared to last month.");
                    }
                }
            }

            // Insight 2: Repeat customers ka revenue contribution
            if(dashboard.Retention!=null && dashboard.Retention.TotalCustomers >0)
            {
                var repeatPercentage = dashboard.Retention.RetentionRatePercentage;
                insights.Add($"{repeatPercentage:0.0}% of your customers are repeat buyers");
            }

            // Insight 3: Sabse acha performing category
            if(dashboard.SalesByCategory.Any())
            {
                var topCategory = dashboard.SalesByCategory.OrderByDescending(c => c.TotalRevenue).First();
                var totalRevenue = dashboard.SalesByCategory.Sum(c => c.TotalRevenue);

                if(totalRevenue > 0)
                {
                    var categoryPercentage = (topCategory.TotalRevenue / totalRevenue) * 100;
                    insights.Add($"'{topCategory.CategoryName}' is your top category, contributing {categoryPercentage:0.0}% of total revenue.");
                }
            }

            // Insight 4: Best selling product highlight
            if(dashboard.BestSellingProducts.Any())
            {
                var topProduct = dashboard.BestSellingProducts.First();
                insights.Add($"'{topProduct.ProductName}' is your best-selling product with {topProduct.TotalUnitsSold} units sold.");
            }
            ViewBag.Insights = insights;
            return View(dashboard);

        }
    }
}