using Ecommerce.API.DTOs;
using Ecommerce.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WishlistController : ControllerBase
    {
        private readonly ECommerceDbContext _context;

        public WishlistController(ECommerceDbContext context)
        {
            _context = context;
        }

        // GET: api/Wishlist/user/3
        // Purpose: User ki poori wishlist dikhana
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUser(int userId)
        {
            var wishlist = await _context.Wishlists
                .Include(w => w.Product)   // Product details bhi chahiye
                .Where(w => w.UserId == userId)
                .ToListAsync();

            return Ok(wishlist);
        }

        // POST: api/Wishlist/add
        // Purpose: Product wishlist mein add karna
        
        [HttpPost("add")]
        public async Task<IActionResult> Add(AddToWishlistDto dto)
        {
            var product = await _context.Products.FindAsync(dto.ProductID);
            if (product == null)
                return BadRequest($"Product with ID {dto.ProductID} does not exist.");

            // Check karo already wishlist mein toh nahi hai (UQ_User_Product_Wishlist constraint bhi yahi enforce karta hai,
            // lekin humara check zyada friendly error message deta hai)
            var alreadyExists = await _context.Wishlists
                .AnyAsync(w => w.UserId == dto.UserID && w.ProductId == dto.ProductID);

            if (alreadyExists)
                return BadRequest("This product is already in your wishlist.");

            var wishlistItem = new Wishlist
            {
                UserId = dto.UserID,
                ProductId = dto.ProductID
            };

            _context.Wishlists.Add(wishlistItem);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Added to wishlist successfully" });
        }

        // DELETE: api/Wishlist/5
        // Purpose: Wishlist se remove karna
       
        [HttpDelete("{id}")]
        public async Task<IActionResult> Remove(int id)
        {
            var item = await _context.Wishlists.FindAsync(id);
            if (item == null)
                return NotFound();

            _context.Wishlists.Remove(item);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}