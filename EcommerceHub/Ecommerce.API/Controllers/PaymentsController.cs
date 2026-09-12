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
    public class PaymentsController : ControllerBase
    {
        private readonly ECommerceDbContext _context;

        public PaymentsController(ECommerceDbContext context)
        {
            _context = context;
        }

        [HttpGet("by-order/{orderId}")]
        public async Task<IActionResult> GetByOrder(int orderId)
        {
            var payments = await _context.Payments
                .Where(p => p.OrderId == orderId).OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            return Ok(payments);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(PaymentCreateDto dto)
        {
            var order = await _context.Orders.FindAsync(dto.OrderID);
            if (order == null)
                return BadRequest($"Order with ID {dto.OrderID} does not exist");

            var alreadyPaidStatus = await _context.PaymentStatusMasters
        .FirstOrDefaultAsync(s => s.StatusName == "Paid");

            if (alreadyPaidStatus != null && order.PaymentStatusId == alreadyPaidStatus.PaymentStatusId)
                return BadRequest("This order has already been paid.");

            // Check 2: Paid amount, order total se match karna chahiye
            if (dto.PaymentStatus == "Success" && Math.Abs(dto.PaidAmount - (order.TotalAmount ?? 0)) > 0.01m)
                return BadRequest($"Payment amount mismatch. Order total is {order.TotalAmount}, but received {dto.PaidAmount}.");

            using var Transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var payment = new Payment
                {
                    OrderId = dto.OrderID,
                    PaymentMethod = dto.PaymentMethod,
                    TransactionId = dto.TransactionID,
                    PaidAmount = dto.PaidAmount,
                    PaymentGateway = dto.PaymentGateway,
                    PaymentStatus = dto.PaymentStatus,
                    PaymentDate = DateTime.Now
                };
                _context.Payments.Add(payment);

                var statusName = dto.PaymentStatus == "Success" ? "Paid" : "Failed";
                var paymentStatus = await _context.PaymentStatusMasters
                    .FirstOrDefaultAsync(s => s.StatusName == statusName);

                if (paymentStatus != null)
                    order.PaymentStatusId = paymentStatus.PaymentStatusId;

                // ================== NAYA CODE - AUTO STATUS UPDATE ==================
                // Agar payment successful hui, toh Order status ko automatically "Processing" kar do
                // (Admin ko manually har order pe click karke "Pending -> Processing" karne ki zaroorat nahi)
                if(dto.PaymentStatus == "Success")
                {
                    var confirmedStatus = await _context.OrderStatusMasters
                        .FirstOrDefaultAsync(s => s.StatusName == "Confirmed");

                    if(confirmedStatus !=null)
                      order.OrderStatusId = confirmedStatus.OrderStatusId;
                }

                await _context.SaveChangesAsync();
                await Transaction.CommitAsync();

                return CreatedAtAction(nameof(GetByOrder), new { orderId = payment.OrderId }, payment);
            }
            catch(Exception)
            {
                await Transaction.RollbackAsync();
                throw;
            }
        }

    }
}
