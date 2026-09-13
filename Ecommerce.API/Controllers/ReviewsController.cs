using Ecommerce.API.DTOs;
using Ecommerce.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewsController : ControllerBase
    {
        private readonly ECommerceDbContext _context;

        public ReviewsController(ECommerceDbContext context)
        {
            _context = context;
        }

        [HttpGet("product/{productId}")]
        public async Task<IActionResult> GetByProduct(int productId)
        {
            var reviews = await _context.ProductReviews.Where(c => c.ProductId == productId)
                .OrderByDescending(r => r.CreatedAt).ToListAsync();

            return Ok(reviews);
        }

        [HttpGet("product/{productId}/summary")]
        public async Task<IActionResult> GetRatingSummary(int productId)
        {
            var reviews = await _context.ProductReviews
                .Where(r => r.ProductId == productId)
                .ToListAsync();

            if(!reviews.Any())
                return Ok(new {averageRating = 0,  totalReviews = 0});

            var summary = new
            {
                averageRating = Math.Round(reviews.Average(r => r.Rating ?? 0), 1),
                totalReviews = reviews.Count,
                fiveStarCount = reviews.Count(r => r.Rating == 5),
                fourStarCount = reviews.Count(r => r.Rating == 4),
                threeStarCount = reviews.Count(r => r.Rating == 3),
                twoStarCount = reviews.Count(r => r.Rating == 2),
                oneStarCount = reviews.Count(r => r.Rating == 1)
            };

            return Ok(summary);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(ReviewCreateDto dto)
        {
            if (dto.Rating < 1 || dto.Rating > 5)
                return BadRequest("Rating must be between 1 and 5");

            var product = await _context.Products.FindAsync(dto.ProductID);
            if (product == null)
                return BadRequest($"Product with ID {dto.ProductID} does not exist");

            var userExists = await _context.Users.AnyAsync(u=>u.UserId == dto.UserID);
            if (!userExists)
                return BadRequest($"User with ID {dto.UserID} does not exist");

            var alreadyReviewed = await _context.ProductReviews
                .AnyAsync(r=>r.ProductId == dto.ProductID && r.UserId == dto.UserID);

            if (alreadyReviewed)
                return BadRequest("You have already reviewed this product");

            var review = new ProductReview
            {
                ProductId = dto.ProductID,
                UserId = dto.UserID,
                Rating = dto.Rating,
                ReviewText = dto.ReviewText
            };

            _context.ProductReviews.Add(review);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetByProduct), new { productId = dto.ProductID }, review);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]

        public async Task<IActionResult> Delete(int id)
        {
            var review = await _context.ProductReviews.FindAsync(id);
            if (review == null)
                return NotFound();

            _context.ProductReviews.Remove(review);
            await _context.SaveChangesAsync();
            return NoContent();
        }

    }
}
