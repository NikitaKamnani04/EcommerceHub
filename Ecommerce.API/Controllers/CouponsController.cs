using Ecommerce.API.DTOs;
using Ecommerce.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.API.Controllers
{
    [ApiController]                          
    [Route("api/[controller]")]
    public class CouponsController : ControllerBase
    {
        private readonly ECommerceDbContext _context;

        public CouponsController(ECommerceDbContext context)
        {
            _context = context;
        }

        // GET: api/Coupons
        // Purpose: Saare coupons dikhana (Admin panel ke liye)
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var coupons = await _context.Coupons.ToListAsync();
            return Ok(coupons);
        }

        // POST: api/Coupons
        // Purpose: Naya coupon banana (sirf Admin)
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(CouponCreateDto dto)
        {
            // Check karo - ye code already exist toh nahi karta
            var codeExists = await _context.Coupons.AnyAsync(c => c.CouponCode == dto.CouponCode);
            if (codeExists)
                return BadRequest("This coupon code already exists.");

            var coupon = new Coupon
            {
                CouponCode = dto.CouponCode.ToUpper(),   // Hamesha uppercase store karo - "save10" aur "SAVE10" same maane jayenge
                DiscountType = dto.DiscountType,
                DiscountValue = dto.DiscountValue,
                MinOrderAmount = dto.MinOrderAmount,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate
            };

            _context.Coupons.Add(coupon);
            await _context.SaveChangesAsync();

            return Ok(coupon);
        }

        // POST: api/Coupons/validate
        // Purpose: Checkout ke waqt coupon check karna - valid hai ki nahi, kitna discount milega
        // Ye endpoint koi bhi logged-in customer use kar sake (Admin-only nahi)
        [Authorize]
        [HttpPost("validate")]
        public async Task<IActionResult> ValidateCoupon(ApplyCouponDto dto)
        {
            var coupon = await _context.Coupons.FirstOrDefaultAsync
                (c => c.CouponCode == dto.CouponCode.ToUpper());

            if (coupon == null)
                return BadRequest(new { message = "Invalid coupon code." });

            // Active hai ki nahi check karo
            if (coupon.IsActive != true)
                return BadRequest(new { message = "This coupon is no longer active." });

            // Date range check karo - abhi expire toh nahi hua, ya abhi start toh hua hai
            var now = DateTime.Now;
            if (now < coupon.StartDate || now > coupon.EndDate)
                return BadRequest(new { message = "This coupon has expired or is not yet active." });
            
            // Minimum order amount check karo
            if(dto.OrderAmount < coupon.MinOrderAmount)
                return BadRequest(new { message = $"Minimum order amount for this coupon is ₹{coupon.MinOrderAmount}." });

            // Sab sahi hai - discount calculate karo
            decimal discountAmount;
            if(coupon.DiscountType=="Percentage")
            {
                discountAmount = dto.OrderAmount * (coupon.DiscountValue / 100);
            }
            else
            {
                discountAmount = coupon.DiscountValue;
            }

            // Discount kabhi bhi order amount se zyada nahi hona chahiye (safety check)
            if (discountAmount > dto.OrderAmount)
                discountAmount = dto.OrderAmount;

            return Ok(new
            {
                valid = true,
                couponId = coupon.CouponId,
                discountAmount = Math.Round(discountAmount, 2),
                finalAmount = Math.Round(dto.OrderAmount - discountAmount,2)
            });


        }


        
    }
}
