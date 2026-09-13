using Ecommerce.API.DTOs;
using Ecommerce.Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnalyticsController : ControllerBase
    {
        private readonly ECommerceDbContext _context;
        public AnalyticsController(ECommerceDbContext context)
        {
            _context = context;
        }

        // GET: api/Analytics/monthly-revenue
        // Purpose: Har mahine ka revenue, order count, growth % - dashboard ke top chart ke liye
        [HttpGet("monthly-revenue")]
        public async Task<IActionResult> GetMonthlyRevenue()
        {
            var result = await _context.Database.SqlQuery<MonthlyRevenueDto>
                ($"exec usp_GetMonthlyRevenue").ToListAsync();

            return Ok(result);
        }

        // GET: api/Analytics/top-repeat-customers
        // Purpose: Sabse zyada kharch karne wale repeat customers - VIP list dashboard ke liye
        [HttpGet("top-repeat-customers")]
        public async Task<IActionResult> GetTopRepeatCustomers()
        {
            var result = await _context.Database.SqlQuery<TopRepeatCustomerDto>
                ($"exec usp_GetTopRepeatCustomers").ToListAsync();

            return Ok(result);
        }

        [HttpGet("customer-lifetime-value")]
        public async Task<IActionResult> GetCustomerLifetimeValue()
        {
            var result = await _context.Database
                .SqlQuery<CustomerLifetimeValueDto>($"EXEC usp_GetCustomerLifetimeValue")
                .ToListAsync();
            return Ok(result);
        }

        // GET: api/Analytics/rfm-analysis
        [HttpGet("rfm-analysis")]
        public async Task<IActionResult> GetRFMAnalysis()
        {
            var result = await _context.Database
                .SqlQuery<RFMAnalysisDto>($"EXEC usp_GetRFMAnalysis")
                .ToListAsync();
            return Ok(result);
        }

        [HttpGet("customer-retention")]
        public async Task<IActionResult> GetCustomerRetention()
        {
            var result = await _context.Database
                .SqlQuery<CustomerRetentionDto>($"EXEC usp_GetCustomerRetention")
                .ToListAsync();
            return Ok(result.FirstOrDefault());   // single row hai, list ki jagah ek object return karo
        }

        [HttpGet("overall-aov")]
        public async Task<IActionResult> GetOverallAOV()
        {
            var result = await _context.Database
                .SqlQuery<OverallAOVDto>($"EXEC usp_GetOverallAOV")
                .ToListAsync();
            return Ok(result.FirstOrDefault());
        }

        [HttpGet("sales-by-category")]
        public async Task<IActionResult> GetSalesByCategory()
        {
            var result = await _context.Database
                .SqlQuery<SalesByCategoryDto>($"EXEC usp_GetSalesByCategory")
                .ToListAsync();
            return Ok(result);
        }

        [HttpGet("sales-by-brand")]
        public async Task<IActionResult> GetSalesByBrand()
        {
            var result = await _context.Database
                .SqlQuery<SalesByBrandDto>($"EXEC usp_GetSalesByBrand")
                .ToListAsync();
            return Ok(result);
        }

        [HttpGet("best-selling-products")]
        public async Task<IActionResult> GetBestSellingProducts()
        {
            var result = await _context.Database
                .SqlQuery<BestSellingProductDto>($"EXEC usp_GetBestSellingProducts")
                .ToListAsync();
            return Ok(result);
        }

        [HttpGet("least-selling-products")]
        public async Task<IActionResult> GetLeastSellingProducts()
        {
            var result = await _context.Database
                .SqlQuery<LeastSellingProductDto>($"EXEC usp_GetLeastSellingProducts")
                .ToListAsync();
            return Ok(result);
        }

        [HttpGet("dead-stock")]
        public async Task<IActionResult> GetDeadStock()
        {
            var result = await _context.Database
                .SqlQuery<DeadStockDto>($"EXEC usp_GetDeadStockAnalysis")
                .ToListAsync();
            return Ok(result);
        }

        [HttpGet("coupon-usage")]
        public async Task<IActionResult> GetCouponUsage()
        {
            var result = await _context.Database
                .SqlQuery<CouponUsageDto>($"EXEC usp_GetCouponUsageAnalysis")
                .ToListAsync();
            return Ok(result);
        }

        [HttpGet("cancellation-return")]
        public async Task<IActionResult> GetCancellationReturn()
        {
            var result = await _context.Database
                .SqlQuery<CancellationReturnDto>($"EXEC usp_GetCancellationReturnAnalysis")
                .ToListAsync();
            return Ok(result.FirstOrDefault());
        }

    }
}
