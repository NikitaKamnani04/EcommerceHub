using Ecommerce.API.Controllers;
using Ecommerce.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Tests
{
    public class ProductsControllerTests
    {
        // Ye ek helper method hai - har test ke liye ek NAYA, KHAALI in-memory database banata hai
        // "naya" isliye zaroori hai kyunki agar sab tests same database share karein,
        // ek test ka data doosre test ko affect kar sakta hai (tests independent hone chahiye)
        //private ECommerceDbContext GetInMemoryDbContext()
        //{
        //    var options = new DbContextOptionsBuilder<ECommerceDbContext>()
        //        // Guid.NewGuid() - har baar ek UNIQUE database naam banata hai,
        //        // taaki har test ka apna alag, fresh database ho
        //        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options;
        //    return new ECommerceDbContext(options);
        //}
        private ECommerceDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ECommerceDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                // Ye extra line add karo - ye EF Core ko explicitly batati hai
                // "sirf In-Memory use karo is context ke liye, OnConfiguring ko ignore karo"
                .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.CoreEventId.ManyServiceProvidersCreatedWarning))
                .Options;

            return new ECommerceDbContext(options);
        }

        // [Fact] attribute batata hai xUnit ko - "ye ek test method hai, ise chalao"
        // Test method ka naam descriptive hona chahiye - "MethodName_Scenario_ExpectedResult" pattern
        [Fact]
        public async Task GetAll_WhenProductsExist_ReturnsAllProducts()
        {
            // ===== ARRANGE (setup) =====
            // Ek fake/temporary database banao is test ke liye

            var context = GetInMemoryDbContext();

            // Isme kuch test products manually daal do (real database ki zaroorat nahi)
            context.Products.Add(new Product
            {
                ProductId = 1,
                ProductName = "Test Laptop",
                Price = 50000,
                StockQuantity = 10,
                CategoryId = 1,
                SubCategoryId = 1,
                BrandId = 1
            });

            context.Products.Add(new Product
            {
                ProductId = 2,
                ProductName = "Test Phone",
                Price = 20000,
                StockQuantity = 5,
                CategoryId = 1,
                SubCategoryId = 1,
                BrandId = 1
            });
            // SaveChangesAsync() - in-memory database mein bhi ye zaroori hai, taaki data "save" ho
            await context.SaveChangesAsync();

            // Ab humara asli Controller banao, isi fake database ke context ke saath
            var controller = new ProductsController(context);

            // ===== ACT (jo method test karna hai, use call karo) =====
            var result = await controller.GetAll();

            // ===== ASSERT (check karo result sahi hai) =====
            // Check 1: Result "OkObjectResult" hona chahiye (matlab HTTP 200 status)
            var okResult = Assert.IsType<OkObjectResult>(result);

            // Check 2: Result ke andar jo data hai, wo List<Product> honi chahiye
            var product = Assert.IsAssignableFrom<System.Collections.Generic.IEnumerable<Product>>(okResult.Value);

            // Check 3: Humne 2 products daale the, toh return bhi 2 hi hone chahiye
            Assert.Equal(2,product.Count());
        }

        [Fact]
        public async Task GetAll_WhenNoProductsExist_ReturnsEmptyList()
        {
            var context = GetInMemoryDbContext();
            var controller = new ProductsController(context);

            var result = await controller.GetAll();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var products = Assert.IsAssignableFrom<System.Collections.Generic.IEnumerable<Product>>(okResult.Value);

            Assert.Empty(products);
        }

    }
}
