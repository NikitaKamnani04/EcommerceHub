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
    public class CartController : ControllerBase
    {
        private readonly ECommerceDbContext _context;

        public CartController(ECommerceDbContext context)
        {
            _context = context;
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetCartByUser(int userId)
        {
            var cart = await _context.Carts.Include(c => c.CartItems)
                .ThenInclude(p => p.Product).FirstOrDefaultAsync(c => c.UserId == userId);

            if(cart == null)
            {
                return Ok(new { message = "Cart is empty", items = new List<object>() });
            }

            return Ok(cart);
        }

        [Authorize]
        [HttpPost("add")]
        public async Task<IActionResult> AddToCart(AddToCartDto dto)
        {
            var product = await _context.Products.FindAsync(dto.ProductID);
            if (product == null)
                return BadRequest($"Product with ID {dto.ProductID} does not exist");

            if (product.StockQuantity < dto.Quantity)
                return BadRequest($"Insufficient stock for '{product.ProductName}'");

            // User ka cart dhundo, agar nahi hai toh naya bana do
            var cart = await _context.Carts.FirstOrDefaultAsync(u => u.UserId == dto.UserID);
            if(cart == null)
            {
                cart = new Cart { UserId = dto.UserID };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            var existingItem = await _context.CartItems.FirstOrDefaultAsync(ci=>ci.CartId
            == cart.CartId && ci.ProductId == product.ProductId);

            if(existingItem != null)
            {
                existingItem.Quantity += dto.Quantity;
            }

            else
            {
                var newItem = new CartItem
                {
                    CartId = cart.CartId,
                    ProductId = dto.ProductID,
                    Quantity = dto.Quantity
                };
                _context.CartItems.Add(newItem);
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Added to cart successfully" });
        }

        [Authorize]
        [HttpPut("item/{cartItemId}")]
        public async Task<IActionResult> UpdateQuantity(int cartItemId, UpdateCartItemDto dto)
        {
            var item = await _context.CartItems.FindAsync(cartItemId);
            if (item == null)
                return NotFound();

            if (dto.Quantity <= 0)
                return BadRequest("Quantity must be greater than 0. Use DELETE to remove the item instead.");

            item.Quantity = dto.Quantity;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [Authorize]
        [HttpDelete("item/{cartItemId}")]
        public async Task<IActionResult> RemoveItem(int cartItemId)
        {
            var item = await _context.CartItems.FindAsync(cartItemId);
            if (item == null)
                return NotFound();

            _context.CartItems.Remove(item);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
