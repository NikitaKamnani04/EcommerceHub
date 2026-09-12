using Ecommerce.Web.Models;
using Ecommerce.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Web.Controllers
{
    public class AdminOrdersController : Controller
    {
        private readonly ApiService _apiService;

        public AdminOrdersController(ApiService apiService)
        {
            _apiService = apiService;
        }

        // Reusable Admin-check helper (jaisa AdminProductsController mein tha)
        private IActionResult? CheckAdminAccess()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin")
                return RedirectToAction("Index", "Home");
            return null;
        }

        // GET: /AdminOrders/Index
        // Purpose: Saare orders ki list dikhana (Admin ke liye)
        public async Task<IActionResult> Index(string? statusFilter = null)
        {
            var accessCheck = CheckAdminAccess();
            if (accessCheck != null) return accessCheck;

            var token = HttpContext.Session.GetString("JWToken");

            var orders = await _apiService.GetAsync<List<OrderListItemModel>>("api/Orders", token);
            var statutes = await _apiService.GetAsync<List<OrderStatusModel>>("api/Orders/statuses", token);

            ViewBag.DebugStatusCount = statutes.Count;
            // Agar filter diya gaya hai (jaise "Pending"), toh sirf usi status wale orders dikhao
            if (!string.IsNullOrEmpty(statusFilter))
            {
                var filterStatusId = statutes.FirstOrDefault(s=>s.StatusName == statusFilter)?.OrderStatusID;
                if(filterStatusId.HasValue)
                {
                    orders = orders.Where(o=>o.OrderStatusID == filterStatusId.Value).ToList();
                }
            }

            // ViewBag use kar rahe hain statuses pass karne ke liye - View mein status naam dikhane ke liye
            ViewBag.Statuses = statutes ?? new List<OrderStatusModel>();
            ViewBag.CurrentFilter = statusFilter;

            return View(orders ?? new List<OrderListItemModel>());
        }

        // GET: /AdminOrders/Details/5
        // Purpose: Ek order ki poori detail dikhana (kaunse products, kitni quantity)
        public async Task<IActionResult> Details(int id)
        {
            var accessCheck = CheckAdminAccess();
            if (accessCheck != null) return accessCheck;

            var token = HttpContext.Session.GetString("JWToken");

            var order = await _apiService.GetAsync<OrderDetailModel>($"api/Orders/{id}", token);
            if (order == null) return NotFound();

            var statuses = await _apiService.GetAsync<List<OrderStatusModel>>("api/Orders/statuses", token);
            ViewBag.Statuses = statuses ?? new List<OrderStatusModel>();

            return View(order);
        }

        // POST: /AdminOrders/UpdateStatus
        // Purpose: Order ka status change karna (Pending -> Shipped -> Delivered)
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int orderId, string statusName)
        {
            var accessCheck = CheckAdminAccess();
            if (accessCheck != null) return accessCheck;

            var token = HttpContext.Session.GetString("JWToken");

            try
            {
                // API ke "api/Orders/{id}/status" endpoint ko call karo
                bool success = await _apiService.PutAsync($"api/Orders/{orderId}/status", new
                {
                    statusName = statusName
                }, token);

                if (success)
                    TempData["SuccessMessage"] = "Order status updated successfully!";
                else
                    TempData["ErrorMessage"] = "Failed to update order status.";
            }
            catch(Exception ex)
            {
                TempData["ErrorMessage"] = "Error: " + ex.Message;
            }   

            // Wahi Details page pe wapas bhejo (jaha se update kiya tha)
            return RedirectToAction("Details", new { id = orderId });
        }

        public class BulkUpdateRequest
        {
            public List<int> OrderIds { get; set; } = new();
            public string StatusName { get; set; } = "";
        }

        // POST: /AdminOrders/BulkUpdateStatus
        // Purpose: Multiple orders ka status ek saath update karna
        [HttpPost]
        public async Task<IActionResult> BulkUpdateStatus([FromBody] BulkUpdateRequest request)
        {
            var accessCheck = CheckAdminAccess();
            if (accessCheck != null) return accessCheck;

            var token = HttpContext.Session.GetString("JWToken");

            // Har Order ID ke liye, ek-ek karke API ko call karo (loop se)
            // Note: real production mein ye ek single "bulk" API endpoint hota (efficiency ke liye),
            // lekin humare paas already single-order UpdateStatus endpoint hai, usi ko reuse kar rahe hain
            foreach (var orderId in request.OrderIds)
            {
                try
                {
                    await _apiService.PutAsync($"api/Orders/{orderId}/status", new
                    {
                        statusName = request.StatusName
                    }, token);
                }
                catch
                {
                    // Agar ek order fail ho jaye, baaki ko process hone do (skip karke aage badho)
                    continue;
                }
            }

            return Ok();
        }

        // GET: /AdminOrders/CreateShipment/5
        public async Task<IActionResult> CreateShipment(int orderId)
        {
            var accessCheck = CheckAdminAccess();
            if (accessCheck != null) return accessCheck;

            return View(new ShipmentCreateFormModel { OrderID = orderId });
        }

        // POST: /AdminOrders/CreateShipment
        [HttpPost]
        public async Task<IActionResult> CreateShipment(ShipmentCreateFormModel model)
        {
            var accessCheck = CheckAdminAccess();
            if (accessCheck != null) return accessCheck;

            if (!ModelState.IsValid)
                return View(model);

            var token = HttpContext.Session.GetString("JWToken");

            try
            {
                await _apiService.PostAsync<ShipmentCreateFormModel, object>("api/Shipments", model, token);
                TempData["SuccessMessage"] = "Shipment created successfully!";
                return RedirectToAction("Details", new { id = model.OrderID });
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", "Failed: " + ex.Message);
                return View(model);
            }
        }
    }
}
