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
    public class OrdersController : ControllerBase
    {
        private readonly ECommerceDbContext _context;

        public OrdersController(ECommerceDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var orders = await _context.Orders.Include(o => o.OrderItems).ToListAsync();
            return Ok(orders);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var order = await _context.Orders.Include(o => o.OrderItems).ThenInclude
                (oi => oi.Product).FirstOrDefaultAsync(o => o.OrderId == id);
            if (order == null)
                return NotFound();

            return Ok(order);
        }

        [HttpGet("by-user/{userId}")]
        public async Task<IActionResult> GetByUser(int userId)
        {
            var orders = await _context.Orders
                .Include(o => o.OrderItems).Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate).ToListAsync();
            return Ok(orders);
        }

        
        [HttpPost]
        public async Task<IActionResult> Create(OrderCreateDto dto)
        {
            if (dto.Items == null || !dto.Items.Any())
                return BadRequest("Order must have atleast one item");

            var userExits = await _context.Users.AnyAsync(u => u.UserId == dto.UserId);
            if (!userExits)
                return BadRequest($"User with ID {dto.UserId} does not exist");

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                decimal orderTotal = 0;
                var orderItems = new List<OrderItem>();


                foreach(var item in dto.Items)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product == null)
                        return BadRequest($"Product with ID {item.ProductId} does not exist");

                    if (product.StockQuantity < item.Quantity)
                        return BadRequest($"InSufficient stock for '{product.ProductName}'. Available : {product.StockQuantity}, Requested : {item.Quantity}");

                    var orderItem = new OrderItem
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = product.Price,
                        DiscountAmount = 0
                    };

                    orderItems.Add(orderItem);
                    orderTotal += product.Price * item.Quantity;

                    
                }

                var pendingStatus = await _context.OrderStatusMasters
                    .FirstOrDefaultAsync(s => s.StatusName == "Pending");

                var order = new Order
                {
                    UserId = dto.UserId,
                    OrderStatusId = pendingStatus?.OrderStatusId,
                    TotalAmount = orderTotal,
                    OrderItems = orderItems
                };

                decimal discountAmount = 0;
                Coupon? appliedCoupon = null;

                // Agar customer ne coupon code diya hai
                if (!string.IsNullOrEmpty(dto.CouponCode))
                {
                    appliedCoupon = await _context.Coupons
                        .FirstOrDefaultAsync(c => c.CouponCode == dto.CouponCode.ToUpper());

                    if (appliedCoupon != null && appliedCoupon.IsActive == true
                        && DateTime.Now >= appliedCoupon.StartDate && DateTime.Now <= appliedCoupon.EndDate &&
                        orderTotal >= appliedCoupon.MinOrderAmount)
                    {
                        discountAmount = appliedCoupon.DiscountType == "Percentage"
                            ? orderTotal * (appliedCoupon.DiscountValue / 100) :
                            appliedCoupon.DiscountValue;

                        if (discountAmount > orderTotal)
                            discountAmount = orderTotal;

                        orderTotal -= discountAmount; // Final total se discount kam karo
                    }
                }

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // Agar coupon apply hua tha, OrderCoupons mein record banao (history ke liye)
                if(appliedCoupon!=null && discountAmount>0)
                {
                    var orderCoupon = new OrderCoupon
                    {
                        OrderId = order.OrderId,
                        CouponId = appliedCoupon.CouponId,
                        DiscountAmount = discountAmount,
                        AppliedAt = DateTime.Now
                    };
                    _context.OrderCoupons.Add(orderCoupon);
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();

                return CreatedAtAction(nameof(GetById), new { id = order.OrderId }, order);
            }

            catch(Exception ex)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, OrderStatusUpdateDto dto)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
                return NotFound();

            var status = await _context.OrderStatusMasters.
                FirstOrDefaultAsync(s => s.StatusName == dto.StatusName);

            if (status == null)
                return BadRequest($"'{dto.StatusName}' is not a valid order status");

            order.OrderStatusId = status.OrderStatusId;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/Orders/statuses
        // Purpose: Saare possible Order Statuses dena (dropdown banane ke liye Admin panel mein)
        [HttpGet("statuses")]
        public async Task<IActionResult> GetOrderStatuses()
        {
            var statuses = await _context.OrderStatusMasters.ToListAsync();
            return Ok(statuses);
        }
    }
}
