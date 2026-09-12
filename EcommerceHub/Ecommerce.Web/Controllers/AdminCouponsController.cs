using Ecommerce.Web.Models;
using Ecommerce.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Web.Controllers
{
    public class AdminCouponsController : Controller
    {
        private readonly ApiService _apiService;

        public AdminCouponsController(ApiService apiService)
        {
            _apiService = apiService;
        }

        private IActionResult? CheckAdminAccess()
        {
            var role = HttpContext.Session.GetString("Role");
            if(role!="Admin")
                return RedirectToAction("Index", "Home");
            return null;
        }

        // GET: /AdminCoupons/Index
        public async Task<IActionResult> Index()
        {
            var accessCheck = CheckAdminAccess();
            if (accessCheck != null) return accessCheck;

            var token = HttpContext.Session.GetString("JWToken");
            var coupons = await _apiService.GetAsync<List<CouponListItemModel>>("api/Coupons", token);

            return View(coupons ?? new List<CouponListItemModel>());
        }

        /// GET: /AdminCoupons/Create
        public IActionResult Create()
        {
            var accessCheck = CheckAdminAccess();
            if (accessCheck != null) return accessCheck;

            return View(new CouponCreateFormModel());
        }

        // POST: /AdminCoupons/Create
        [HttpPost]
        public async Task<IActionResult> Create(CouponCreateFormModel model)
        {
            var accessCheck = CheckAdminAccess();
            if(accessCheck!= null) return accessCheck;

            if (!ModelState.IsValid) return View(model);
            var token = HttpContext.Session.GetString("JWToken");

            try
            {
                await _apiService.PostAsync<CouponCreateFormModel, object>("api/Coupons", model, token);
                TempData["SuccessMessage"] = "Coupon created successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Failed: " + ex.Message);
                return View(model);
            }
        }

    }
}
