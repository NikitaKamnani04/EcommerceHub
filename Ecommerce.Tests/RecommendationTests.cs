using Ecommerce.API.Controllers;
using Ecommerce.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;

namespace Ecommerce.Tests
{
    public class RecommendationTests
    {
        private ECommerceDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ECommerceDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(warnings =>
                    warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            return new ECommerceDbContext(options);
        }

        [Fact]
        public async Task GetRecommendations_ReturnsProductsBoughtTogether()
        {
            // ===== ARRANGE =====
            var context = GetInMemoryDbContext();

            // 3 products banao: Laptop, Mouse, Keyboard
            context.Products.Add(new Product { ProductId = 1, ProductName = "Laptop", Price = 50000, StockQuantity = 10, CategoryId = 1, SubCategoryId = 1, BrandId = 1 });
            context.Products.Add(new Product { ProductId = 2, ProductName = "Mouse", Price = 500, StockQuantity = 10, CategoryId = 1, SubCategoryId = 1, BrandId = 1 });
            context.Products.Add(new Product { ProductId = 3, ProductName = "Keyboard", Price = 1000, StockQuantity = 10, CategoryId = 1, SubCategoryId = 1, BrandId = 1 });

            // Ek Order banao jisme Laptop + Mouse dono hain (same order mein)
            context.Orders.Add(new Order { OrderId = 1, UserId = 1, TotalAmount = 50500 });
            context.OrderItems.Add(new OrderItem { OrderItemId = 1, OrderId = 1, ProductId = 1, Quantity = 1, UnitPrice = 50000 });
            context.OrderItems.Add(new OrderItem { OrderItemId = 2, OrderId = 1, ProductId = 2, Quantity = 1, UnitPrice = 500 });

            // Doosra Order - Laptop + Keyboard (alag customer, alag order)
            context.Orders.Add(new Order { OrderId = 2, UserId = 2, TotalAmount = 51000 });
            context.OrderItems.Add(new OrderItem { OrderItemId = 3, OrderId = 2, ProductId = 1, Quantity = 1, UnitPrice = 50000 });
            context.OrderItems.Add(new OrderItem { OrderItemId = 4, OrderId = 2, ProductId = 3, Quantity = 1, UnitPrice = 1000 });

            await context.SaveChangesAsync();

            var controller = new ProductsController(context);

            // ===== ACT =====
            // Laptop (ProductID = 1) ke recommendations maango
            var result = await controller.GetRecommendations(1);

            // ===== ASSERT =====
            var okResult = Assert.IsType<OkObjectResult>(result);
            var recommendations = Assert.IsAssignableFrom<IEnumerable<Ecommerce.API.DTOs.RecommendedProductDto>>(okResult.Value);

            // Laptop ke saath Mouse aur Keyboard dono khareede gaye the (alag-alag orders mein),
            // toh dono recommendations mein aane chahiye
            var recommendationsList = recommendations.ToList();
            Assert.Contains(recommendationsList, r => r.ProductName == "Mouse");
            Assert.Contains(recommendationsList, r => r.ProductName == "Keyboard");
        }

        [Fact]
        public async Task GetRecommendations_WhenNoOrdersExist_ReturnsEmptyList()
        {
            var context = GetInMemoryDbContext();

            // Product hai, lekin koi order kabhi hua hi nahi
            context.Products.Add(new Product { ProductId = 1, ProductName = "Lonely Product", Price = 100, StockQuantity = 10, CategoryId = 1, SubCategoryId = 1, BrandId = 1 });
            await context.SaveChangesAsync();

            var controller = new ProductsController(context);

            var result = await controller.GetRecommendations(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var recommendations = Assert.IsAssignableFrom<IEnumerable<Ecommerce.API.DTOs.RecommendedProductDto>>(okResult.Value);

            // Koi order nahi hua, toh koi recommendation bhi nahi honi chahiye
            Assert.Empty(recommendations);
        }

        [Fact]
        public async Task GetRecommendations_DoesNotRecommendSameProduct()
        {
            var context = GetInMemoryDbContext();

            context.Products.Add(new Product { ProductId = 1, ProductName = "Product A", Price = 100, StockQuantity = 10, CategoryId = 1, SubCategoryId = 1, BrandId = 1 });

            // Ek order mein sirf Product A hi hai (do baar, quantity 2 se, self-reference test ke liye)
            context.Orders.Add(new Order { OrderId = 1, UserId = 1, TotalAmount = 200 });
            context.OrderItems.Add(new OrderItem { OrderItemId = 1, OrderId = 1, ProductId = 1, Quantity = 2, UnitPrice = 100 });

            await context.SaveChangesAsync();

            var controller = new ProductsController(context);
            var result = await controller.GetRecommendations(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var recommendations = Assert.IsAssignableFrom<IEnumerable<Ecommerce.API.DTOs.RecommendedProductDto>>(okResult.Value);

            // Product A khud ko kabhi recommend nahi karna chahiye
            Assert.DoesNotContain(recommendations, r => r.ProductID == 1);
        }
    }
}