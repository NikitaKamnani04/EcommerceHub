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
    public class CartControllerTests
    {
        private ECommerceDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ECommerceDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new ECommerceDbContext(options);
        }

        [Fact]
        public async Task AddToCart_WhenProductAlreadyInCart_IncreasesQuantity()
        {
            // ===== ARRANGE =====
            var context = GetInMemoryDbContext();

            // Ek product banao stock ke saath
            context.Products.Add(new Product
            {
                ProductId = 1,
                ProductName = "Test Product",
                Price = 500,
                StockQuantity = 50,
                CategoryId = 1,
                SubCategoryId = 1,
                BrandId = 1
            });

            // User ka cart pehle se bana hua hai, usme ye product already 2 quantity mein hai
            context.Carts.Add(new Cart { CartId = 1, UserId = 1 });
            context.CartItems.Add(new CartItem
            {
                CartItemId = 1,
                CartId = 1,
                ProductId = 1,
                Quantity = 2   // pehle se 2 hain cart mein
            });

            await context.SaveChangesAsync();

            var controller = new CartController(context);

            // Dobara wahi product add karo, 3 aur quantity ke saath
            var dto = new AddToCartDto { UserID = 1, ProductID =1, Quantity = 3 };

            // ===== ACT =====
            await controller.AddToCart(dto);

            // ===== ASSERT =====
            // Database mein wapas dekho - is product ka CartItem kya dikh raha hai
            var cartItem = await context.CartItems.FirstOrDefaultAsync
                (ci => ci.ProductId == 1);

            // Naya row nahi bana hona chahiye - same row ki quantity badhni chahiye (2 + 3 = 5)
            Assert.NotNull(cartItem);
            Assert.Equal(5, cartItem.Quantity);

            // Confirm karo sirf EK hi CartItem row hai (duplicate nahi bana)
            var totalCartItems = await context.CartItems.CountAsync(ci => ci.ProductId == 1);
            Assert.Equal(1, totalCartItems);
        }

        [Fact]
        public async Task AddToCart_WhenStockInsufficient_ReturnsBadRequest()
        {
            var context = GetInMemoryDbContext();

            // Product banao jisme sirf 2 stock hai
            context.Products.Add(new Product
            {
                ProductId = 2,
                ProductName = "Low Stock Product",
                Price = 500,
                StockQuantity = 2,
                CategoryId = 1,
                SubCategoryId = 1,
                BrandId = 1
            });
            await context.SaveChangesAsync();

            var controller = new CartController(context);
            // 5 quantity maango, jabki stock sirf 2 hai
            var dto = new AddToCartDto { UserID = 1, ProductID = 2, Quantity = 5 };

            var result = await controller.AddToCart(dto);

            // Expect BadRequest, kyunki stock kam hai
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task AddToCart_WhenProductDoesNotExist_ReturnsBadRequest()
        {
            var context = GetInMemoryDbContext();
            // Koi product add nahi kiya - database khaali hai

            var controller = new CartController(context);
            var dto = new AddToCartDto { UserID = 1, ProductID = 999, Quantity = 1 };

            var result = await controller.AddToCart(dto);

            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}
