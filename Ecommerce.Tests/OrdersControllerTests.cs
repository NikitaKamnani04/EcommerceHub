using Ecommerce.API.Controllers;
using Ecommerce.API.DTOs;
using Ecommerce.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Tests
{
    public class OrdersControllerTests
    {
        //private ECommerceDbContext GetInMemoryDbContext()
        //{
        //    var options = new DbContextOptionsBuilder<ECommerceDbContext>()
        //        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
        //        .Options;

        //    return new ECommerceDbContext(options);
        //}

        private ECommerceDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ECommerceDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(warnings =>
                    warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            return new ECommerceDbContext(options);
        }

        // Common setup - dono tests mein user aur pending-status chahiye hoga
        private async Task SeedCommonData(ECommerceDbContext context)
        {
            context.Users.Add(new User { UserId = 1, ProfileId = 1, Username = "testUser" });
            context.OrderStatusMasters.Add(new OrderStatusMaster { OrderStatusId = 1, StatusName = "Pending" });
            await context.SaveChangesAsync();
        }

        [Fact]
        public async Task Create_WhenStockInsufficient_ReturnsBadRequest()
        {
            // ===== ARRANGE =====
            var context = GetInMemoryDbContext();
            await SeedCommonData(context);

            // Product jisme sirf 3 stock hai
            context.Products.Add(new Product
            {
                ProductId = 1,
                ProductName = "Limited Stock Item",
                Price = 1000,
                StockQuantity = 3,
                CategoryId = 1,
                SubCategoryId = 1,
                BrandId = 1
            });
            await context.SaveChangesAsync();

            var controller = new OrdersController(context);

            // Order mein 10 quantity maangi ja rahi hai, jabki stock sirf 3 hai
            var dto = new OrderCreateDto
            {
                UserId = 1,
                Items = new List<OrderItemCreateDto>
                {
                    new OrderItemCreateDto { ProductId = 1, Quantity = 10 }
                }
            };

            // ===== ACT =====
            var result = await controller.Create(dto);

            // ===== ASSERT =====
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Create_WhenValidOrder_CreatesOrderWithCorrectTotal()
        {
            var context = GetInMemoryDbContext();
            await SeedCommonData(context);

            // 2 products banao, kaafi stock ke saath
            context.Products.Add(new Product
            {
                ProductId = 1,
                ProductName = "Product A",
                Price = 500,
                StockQuantity = 50,
                CategoryId = 1,
                SubCategoryId = 1,
                BrandId = 1
            });
            context.Products.Add(new Product
            {
                ProductId = 2,
                ProductName = "Product B",
                Price = 300,
                StockQuantity = 50,
                CategoryId = 1,
                SubCategoryId = 1,
                BrandId = 1
            });
            await context.SaveChangesAsync();

            var controller = new OrdersController(context);

            // Order: 2 units of Product A (500 x 2 = 1000) + 1 unit of Product B (300 x 1 = 300)
            // Expected total = 1300
            var dto = new OrderCreateDto
            {
                UserId = 1,
                Items = new List<OrderItemCreateDto>
                {
                    new OrderItemCreateDto { ProductId = 1, Quantity = 2 },
                    new OrderItemCreateDto { ProductId = 2, Quantity = 1 }
                }
            };

            var result = await controller.Create(dto);

            // Expect success (CreatedAtActionResult - jo humne Controller mein return kiya tha)
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);

            // Return hui Order object ko nikaalo, uska TotalAmount check karo
            var order = Assert.IsType<Order>(createdResult.Value);
            Assert.Equal(1300, order.TotalAmount);

            // Database mein bhi confirm karo - OrderItems sahi bane
            var orderItemsCount = await context.OrderItems.CountAsync();
            Assert.Equal(2, orderItemsCount);
        }

        [Fact]
        public async Task Create_WhenProductDoesNotExist_ReturnsBadRequest()
        {
            var context = GetInMemoryDbContext();
            await SeedCommonData(context);
            // Koi product add nahi kiya

            var controller = new OrdersController(context);

            var dto = new OrderCreateDto
            {
                UserId = 1,
                Items = new List<OrderItemCreateDto>
                {
                    new OrderItemCreateDto { ProductId = 999, Quantity = 1 }
                }
            };

            var result = await controller.Create(dto);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Create_WhenNoItemsProvided_ReturnsBadRequest()
        {
            var context = GetInMemoryDbContext();
            await SeedCommonData(context);

            var controller = new OrdersController(context);

            // Khaali items list bhej rahe hain
            var dto = new OrderCreateDto
            {
                UserId = 1,
                Items = new List<OrderItemCreateDto>()
            };

            var result = await controller.Create(dto);

            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}
