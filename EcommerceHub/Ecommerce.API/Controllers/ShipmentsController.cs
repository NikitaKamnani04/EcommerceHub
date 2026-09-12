using Ecommerce.API.DTOs;
using Ecommerce.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShipmentsController : ControllerBase
    {
        private readonly ECommerceDbContext _context;

        public ShipmentsController(ECommerceDbContext context)
        {
            _context = context;
        }

        // GET: api/Shipments/order/5
        // Purpose: Ek order ki shipping detail dekhna (Customer ya Admin dono use kar sakte hain)
        [Authorize]
        [HttpGet("order/{orderId}")]
        public async Task<IActionResult> GetByOrder(int orderId)
        {
            var shipment = await _context.Shipments
                .FirstOrDefaultAsync(s=>s.OrderId == orderId);

            if (shipment == null)
                return NotFound(new { message = "No shipment created for this order yet." });

            return Ok(shipment);
        }

        // POST: api/Shipments
        // Purpose: Naya shipment banana (Admin order ko "Shipped" karte waqt)
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(ShipmentCreateDto dto)
        {
            var order = await _context.Orders.FindAsync(dto.OrderID);
            if (order == null)
                return BadRequest("Order does not exist.");

            // Check karo - is order ke liye shipment already toh nahi bana
            var exists = await _context.Shipments.AnyAsync(s => s.OrderId == dto.OrderID);
            if (exists)
                return BadRequest("Shipment already exists for this order.");

            var shipment = new Shipment
            {
                OrderId = dto.OrderID,
                TrackingNumber = dto.TrackingNumber,
                CourierName = dto.CourierName,
                ShipmentStatus = "In Transit",
                ShippedDate = DateTime.Now,
                EstimatedDelivery = dto.EstimatedDelivery
            };

            _context.Shipments.Add(shipment);
            await _context.SaveChangesAsync();

            return Ok(shipment);
        }

        // PUT: api/Shipments/5
        // Purpose: Shipment status update karna (jaise "Delivered" mark karna)
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, ShipmentUpdateDto dto)
        {
            var shipment = await _context.Shipments.FindAsync(id);
            if (shipment == null)
                return NotFound();

            if (!string.IsNullOrEmpty(dto.ShipmentStatus))
                shipment.ShipmentStatus = dto.ShipmentStatus;

            if (dto.DeliveredDate.HasValue)
                shipment.DeliveredDate = dto.DeliveredDate.Value;

            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
