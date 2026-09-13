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
            return View(dashboard);

            return View();
        }
    }
}