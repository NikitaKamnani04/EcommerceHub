using Ecommerce.Web.Models;
using Ecommerce.Web.Services;
using Microsoft.AspNetCore.Mvc;
using QRCoder;

namespace Ecommerce.Web.Controllers
{
    public class CheckoutController : Controller
    {

        // ApiService - isse hum Ecommerce.API ko call karenge
        private readonly ApiService _apiService;

        public CheckoutController(ApiService apiService)
        {
            _apiService = apiService;
        }

        // GET: /Checkout/Index
        // Purpose: Checkout page dikhana - cart summary + confirm button
        public async Task<IActionResult> Index()
        {
            // Session se token aur userId nikaalo
            var token = HttpContext.Session.GetString("JWToken");
            var userIdString = HttpContext.Session.GetString("UserId");

            // Agar login nahi hai, Login page pe bhej do
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(userIdString))
                return RedirectToAction("Login", "Account");

            int userId = int.Parse(userIdString);

            // Cart fetch karo - dikhane ke liye ki customer kya order karne wala hai
            var cart = await _apiService.GetAsync<CartPageModel>($"api/Cart/user/{userId}", token);
            cart ??= new CartPageModel
            {
                CartItems = new List<CartPageItemModel>()
            };

            // Agar cart khaali hai, Cart page pe wapas bhej do
            if (!cart.CartItems.Any())
                return RedirectToAction("Index", "Cart");

            return View(cart);
        }

        // ================== NAYA CODE - Apply Coupon endpoint ==================
        // Ye class batati hai - jab jQuery se AJAX request aayegi coupon apply karne ke liye,
        // usme kaunse fields honge
        public class ApplyCouponRequest
        {
            public string CouponCode { get; set; } = "";
            public decimal OrderAmount { get; set; }
        }

        // POST: /Checkout/ApplyCoupon
        // Purpose: Coupon ko validate karna - AJAX se site.js/checkout script se call hoga
        [HttpPost]
        public async Task<IActionResult> ApplyCoupon([FromBody] ApplyCouponRequest request)
        {
            var token = HttpContext.Session.GetString("JWToken");

            try
            {
                // API ke "api/Coupons/validate" endpoint ko call karo
                var result = await _apiService.PostAsync<object, object>("api/Coupons/validate", new
                {
                    couponCode = request.CouponCode,
                    orderAmount = request.OrderAmount
                }, token);

                return Ok(result);
            }
            catch (Exception ex)
            {
                // Agar coupon invalid hai, error message wapas bhejo (jQuery isko dikhayega)
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST: /Checkout/PlaceOrder
        // Purpose: Actual Order create karna - cart ke items se Order banega
        [HttpPost]
        public async Task<IActionResult> PlaceOrder(string? couponCode = null)
        {
            var token = HttpContext.Session.GetString("JWToken");
            var userIdString = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(userIdString))
                return RedirectToAction("Login", "Account");

            int userId = int.Parse(userIdString);

            // Cart dobara fetch karo (fresh data)
            var cart = await _apiService.GetAsync<CartPageModel>($"api/Cart/user/{userId}", token);

            if (cart == null || !cart.CartItems.Any())
                return RedirectToAction("Index", "Cart");

            try
            {
                // Cart items ko Order API ke expected format mein convert karo
                var orderItems = cart.CartItems.Select(item => new
                {
                    productID = item.ProductID,
                    quantity = item.Quantity
                }).ToList();

                // Order create karo
                var orderResult = await _apiService.PostAsync<object, OrderResultModel>("api/Orders", new
                {
                    userID = userId,
                    items = orderItems,
                    couponCode = couponCode
                }, token);

                if(orderResult!=null)
                {
                    // Ab Payment page pe bhejo, Confirmation pe nahi
                    return RedirectToAction("Payment", new
                    {
                        orderId = orderResult.OrderID,
                        amount = orderResult.TotalAmount,
                    });
                }

                return RedirectToAction("Index");

                
            }
            catch(Exception ex)
            {
                TempData["OrderError"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        // GET: /Checkout/Payment
        // Purpose: UPI QR Code dikhana - customer scan karke pay kare
        public IActionResult Payment(int orderId, decimal amount)
        {
            // TUMHARA APNA UPI ID YAHAN DAALO
            string upiId = "yourupiid@paytm";   // apna actual UPI ID daalna (jaise 9876543210@ybl)
            string payeeName = "EcommerceHub";

            // UPI deep-link banate hain - standard format hai ye, har UPI app isse samajhta hai
            // pa = payee address (UPI ID), pn = payee name, am = amount, cu = currency, tn = transaction note
            string upiLink = $"upi://pay?pa={upiId}&pn={Uri.EscapeDataString(payeeName)}&am={amount}&cu=INR&tn=Order{orderId}";

            // QR Code generate karo is link se
            var qrGenerator = new QRCoder.QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(upiLink, QRCoder.QRCodeGenerator.ECCLevel.Q);
            var qrCode = new QRCoder.PngByteQRCode(qrCodeData);
            byte[] qrCodeBytes = qrCode.GetGraphic(20);

            // Byte array ko Base64 string mein convert karo - taaki seedha HTML <img> mein dikha sakein
            string qrCodeBase64 = Convert.ToBase64String(qrCodeBytes);

            var model = new PaymentPageModel
            {
                OrderID = orderId,
                Amount = amount,
                UpiQrCodeBase64 = qrCodeBase64,
                UpiLink = upiLink
            };

            return View(model);
        }

        // POST: /Checkout/ConfirmPayment
        // Purpose: Customer "I've Paid" click kare - Payment record banayenge, Order status update hoga
        [HttpPost]
        public async Task<IActionResult> ConfirmPayment(int orderId, decimal amount)
        {
            var token = HttpContext.Session.GetString("JWToken");

            try
            {
                await _apiService.PostAsync<object, object>("api/Payments", new
                {
                    orderID = orderId,
                    paymentMethod = "UPI",
                    transactionID = Guid.NewGuid().ToString(),   // real gateway hota toh gateway se milta, abhi generate kar rahe
                    paidAmount = amount,
                    paymentGateway = "UPI Direct",
                    paymentStatus = "Success"
                }, token);

                TempData["OrderSuccess"] = "Payment confirmed! Your order has been placed successfully.";
                return RedirectToAction("Confirmation");
            }

            catch (Exception ex)
            {
                TempData["OrderError"] = "Payment confirmation failed: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // GET: /Checkout/Confirmation
        public IActionResult Confirmation()
        {
            return View();
        }
    }
}
