using Ecommerce.API.Controllers;
using Ecommerce.API.DTOs;
using Ecommerce.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Tests
{
    public class CouponsControllerTests
    {
        // Wahi helper jo ProductsControllerTests mein tha - har test ke liye naya, khaali database

        private ECommerceDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ECommerceDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new ECommerceDbContext(options);
        }

        [Fact]
        public async Task ValidateCoupon_WhenExpired_ReturnsBadRequest()
        {
            var context = GetInMemoryDbContext();

            // Ek EXPIRED coupon database mein daalo (EndDate kal se pehle ki)
            context.Coupons.Add(new Coupon
            {
                CouponId = 1,
                CouponCode = "EXPIRED10",
                DiscountType = "Percentage",
                DiscountValue = 10,
                MinOrderAmount = 0,
                StartDate = DateTime.Now.AddDays(-30),   // 30 din pehle shuru hua
                EndDate = DateTime.Now.AddDays(-1),       // Kal hi khatam ho gaya - EXPIRED
                IsActive = true
            });
            await context.SaveChangesAsync();

            var controller = new CouponsController(context);
            var dto = new ApplyCouponDto
            {
                CouponCode = "EXPIRED10",
                OrderAmount = 1000
            };

            // ===== ACT =====
            var result = await controller.ValidateCoupon(dto);

            // ===== ASSERT =====
            // Expect karte hain BadRequest (400) aaye, kyunki coupon expire ho chuka hai
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task ValidateCoupon_WhenOrderBelowMinimum_ReturnsBadRequest()
        {
            var context = GetInMemoryDbContext();

            // Coupon banao jisme MinOrderAmount = 1000 hai
            context.Coupons.Add(new Coupon
            {
                CouponId = 2,
                CouponCode = "MIN1000",
                DiscountType = "Flat",
                DiscountValue = 100,
                MinOrderAmount = 1000,
                StartDate = DateTime.Now.AddDays(-5),
                EndDate = DateTime.Now.AddDays(5),
                IsActive = true
            });
            await context.SaveChangesAsync();

            var controller = new CouponsController(context);

            // Order amount sirf 500 hai - minimum (1000) se kam
            var dto = new ApplyCouponDto { CouponCode = "MIN1000", OrderAmount = 500 };

            var result = await controller.ValidateCoupon(dto);

            // Expect BadRequest, kyunki order amount minimum se kam hai
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task ValidateCoupon_WhenInvalidCode_ReturnsBadRequest()
        {
            var context = GetInMemoryDbContext();
            // Jaanbojh ke koi coupon add nahi kiya - database khaali hai

            var controller = new CouponsController(context);
            var dto = new ApplyCouponDto { CouponCode = "NOTEXIST", OrderAmount = 1000 };

            var result = await controller.ValidateCoupon(dto);

            // Coupon exist hi nahi karta, isliye BadRequest expected hai
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task ValidateCoupon_WhenValid_ReturnsCorrectDiscount()
        {
            var context = GetInMemoryDbContext();

            // Valid, active coupon - 10% discount
            context.Coupons.Add(new Coupon
            {
                CouponId = 3,
                CouponCode = "SAVE10",
                DiscountType = "Percentage",
                DiscountValue = 10,
                MinOrderAmount = 0,
                StartDate = DateTime.Now.AddDays(-5),
                EndDate = DateTime.Now.AddDays(5),
                IsActive = true
            });
            await context.SaveChangesAsync();

            var controller = new CouponsController(context);
            var dto = new ApplyCouponDto { CouponCode = "SAVE10", OrderAmount = 1000 };

            var result = await controller.ValidateCoupon(dto);

            // Expect OkObjectResult (success), kyunki sab conditions sahi hain
            var okResult = Assert.IsType<OkObjectResult>(result);

            // 1000 ka 10% = 100 discount hona chahiye
            // Yahan hum response object ke andar jhank rahe hain (reflection se property nikaal rahe hain)
            var response = okResult.Value;
            var discountProperty = response!.GetType().GetProperty("discountAmount");
            var discountValue = (decimal)discountProperty!.GetValue(response)!;

            Assert.Equal(100m, discountValue);
        }
    }
    }
