using Ecommerce.API.Controllers;
using Ecommerce.API.DTOs;
using Ecommerce.API.Services;
using Ecommerce.Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Tests
{
    public class UsersControllerTests
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

        // TokenService ko banane ke liye IConfiguration chahiye - test ke liye ek "fake" config banate hain
        private TokenService GetTokenService()
        {
            var inMemorySettings = new Dictionary<string, string?>
            {
                   { "Jwt:Key", "TestSecretKeyForUnitTestingPurposeOnly12345" },
                { "Jwt:Issuer", "TestIssuer" },
                { "Jwt:Audience", "TestAudience" },
                { "Jwt:ExpiryMinutes", "60" }
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
            .Build();

            return new TokenService(configuration);
        }

        [Fact]
        public async Task Login_WithCorrectPassword_ReturnsSuccessWithToken()
        {
            // ===== ARRANGE =====
            var context = GetInMemoryDbContext();

            // Password ko sahi se hash karke store karo (jaisa Register method karta hai)
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword("Test@123");

            context.UserProfiles.Add(new UserProfile
            {
                UserId = 14,
                FullName = "Test User",
                Email = "test@example.com",
                PasswordHash = hashedPassword
            });
            context.Users.Add(new User
            {
                UserId = 13,
                ProfileId = 14,
                Username = "testuser1"
            });
            await context.SaveChangesAsync();

            var controller = new UsersController(context, GetTokenService());

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };


            var loginDto = new UserLoginDto { Email = "test@example.com", Password = "Test@123" };

            // ===== ACT =====
            var result = await controller.Login(loginDto);

            // ===== ASSERT =====
            // Sahi password se login successful hona chahiye (OkObjectResult)
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task Login_WithIncorrectPassword_ReturnsUnauthorized()
        {
            var context = GetInMemoryDbContext();

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword("CorrectPassword123");

            context.UserProfiles.Add(new UserProfile
            {
                UserId = 14,
                FullName = "Test User",
                Email = "test@example.com",
                PasswordHash = hashedPassword
            });
            context.Users.Add(new User
            {
                UserId = 13,
                ProfileId = 14,
                Username = "testuser"
            });
            await context.SaveChangesAsync();

            var controller = new UsersController(context, GetTokenService());

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            // Galat password bhej rahe hain
            var loginDto = new UserLoginDto { Email = "test@example.com", Password = "WrongPassword" };

            var result = await controller.Login(loginDto);

            // Galat password se Unauthorized (401) aana chahiye
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task Login_WithNonExistentEmail_ReturnsUnauthorized()
        {
            var context = GetInMemoryDbContext();
            // Koi user add nahi kiya - database khaali hai

            var controller = new UsersController(context, GetTokenService());
            var loginDto = new UserLoginDto { Email = "nobody@example.com", Password = "AnyPassword" };

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            var result = await controller.Login(loginDto);

            // Email exist hi nahi karta, isliye Unauthorized expected hai
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task Register_WithDuplicateEmail_ReturnsBadRequest()
        {
            var context = GetInMemoryDbContext();

            // Ek user already existing email ke saath
            context.UserProfiles.Add(new UserProfile
            {
                UserId = 1,
                FullName = "Existing User",
                Email = "rashi@gmail.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("SomePassword")
            });
            await context.SaveChangesAsync();

            var controller = new UsersController(context, GetTokenService());

            // Same email se dobara register karne ki koshish
            var registerDto = new UserRegisterDto
            {
                FullName = "New User",
                Email = "rashi@gmail.com",   // duplicate
                Password = "NewPassword123",
                Username = "newuser"
            };

            var result = await controller.Register(registerDto);

            // Duplicate email hone ki wajah se BadRequest expected hai
            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}
