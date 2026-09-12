using Ecommerce.Web.Models;
using Ecommerce.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Web.Controllers
{
    public class OrdersController : Controller
    {

        private readonly ApiService _apiService;

        public OrdersController(ApiService apiService)
        {
            _apiService = apiService;
        }

        // GET: /Orders/Index
        // Purpose: Customer ka apna order history dikhana
        public async Task<IActionResult> Index()
        {
            var token = HttpContext.Session.GetString("JWToken");
            var userIdString = HttpContext.Session.GetString("UserId");

            // Agar login nahi hai, Login page pe bhej do
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(userIdString))
                return RedirectToAction("Login", "Account");

            int userId = int.Parse(userIdString);

            // Sirf isi user ke orders fetch karo - "by-user" endpoint use kar rahe hain
            // (yaad hai OrdersController API mein ye endpoint pehle se hai)

            var orders = await _apiService.GetAsync<List<OrderListItemModel>>($"api/Orders/by-user/{userId}",token);
            orders ??= new List<OrderListItemModel>();

            // Status names bhi chahiye honge dikhane ke liye
            var statuses = await _apiService.GetAsync<List<OrderStatusModel>>("api/Orders/statuses", token);
            ViewBag.Statuses = statuses ?? new List<OrderStatusModel>();

            return View(orders);
        }

        // GET: /Orders/Details/5
        // Purpose: Ek order ki poori detail dikhana (customer ke liye)
        public async Task<IActionResult> Details(int id)
        {
            var token = HttpContext.Session.GetString("JWToken");
            var userIdString = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(userIdString))
                return RedirectToAction("Login", "Account");

            var order = await _apiService.GetAsync<OrderDetailModel>($"api/Orders/{id}", token);

            if (order == null) return NotFound();

            // SECURITY CHECK: Confirm karo ye order isi customer ka hai
            // (warna koi customer URL manually type karke doosre ka order dekh sakta tha - "IDOR" vulnerability)
            int userId = int.Parse(userIdString);
            if (order.UserID != userId)
                return Forbid();

            var statuses = await _apiService.GetAsync<List<OrderStatusModel>>("api/Orders/statuses", token);
            ViewBag.Statuses = statuses ?? new List<OrderStatusModel>();

            // NAYA: Shipment info bhi fetch karo (agar bana hai)
            // GetAsync null return karega agar shipment nahi mila (404), koi crash nahi hoga
            var shipment = await _apiService.GetAsync<ShipmentModel>($"api/Shipments/order/{id}", token);
            ViewBag.Shipment = shipment;

            return View(order);
        }
    }
}
