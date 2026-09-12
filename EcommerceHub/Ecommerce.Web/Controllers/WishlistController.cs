using Ecommerce.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Web.Controllers
{
    public class WishlistController : Controller
    {
        private readonly ApiService _apiService;

        public WishlistController(ApiService apiService)
        {
            _apiService = apiService;
        }

        // Ye class batati hai - AJAX request se kya aayega (sirf ProductId chahiye)
        public class WishlistToggleRequest
        {
            public int ProductId { get; set; }
        }

        // POST: /Wishlist/Toggle
        // Purpose: Agar product wishlist mein NAHI hai, toh add karo. Agar HAI, toh remove karo.
        // Isse ek hi button "heart icon" ki tarah toggle kar sakta hai (jaise Instagram ka like button)
        [HttpPost]
        public async Task<IActionResult> Toggle([FromBody] WishlistToggleRequest request)
        {
            var token = HttpContext.Session.GetString("JWToken");
            var userIdString = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(userIdString))
            {
                return Unauthorized();
            }

            int userId = int.Parse(userIdString);

            try
            {
                // Pehle check karo - ye product already wishlist mein hai ki nahi
                var wishlist = await _apiService.GetAsync<List<WishlistItemModel>>($"api/Wishlist/user/{userId}", token);
                wishlist ??= new List<WishlistItemModel>();

                var existingItem = wishlist.FirstOrDefault(w => w.ProductID == request.ProductId);

                if(existingItem!=null)
                {
                    // Already hai - REMOVE karo (DELETE endpoint call karo, WishlistID ke saath)
                    await _apiService.DeleteAsync($"api/Wishlist/{existingItem.WishlistID}", token);

                    // "isInWishlist: false" bhejo - taaki JS ko pata chale heart ab "empty" dikhana hai
                    return Json(new { success = true, isInWishlist = false });
                }
                else
                {
                    // Nahi hai - ADD karo
                    await _apiService.PostAsync<object, object>("api/Wishlist/add", new
                    {
                        userId = userId,
                        productId = request.ProductId
                    },token);

                    // "isInWishlist: true" bhejo - taaki JS ko pata chale heart ab "filled" dikhana hai
                    return Json(new { success = true, isInWishlist = true });
                }
            }
            catch(Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: /Wishlist/Index
        // Purpose: Customer ki poori wishlist dikhana
        public async Task<IActionResult> Index()
        {
            var token = HttpContext.Session.GetString("JWToken");
            var userIdString = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(userIdString))
                return RedirectToAction("Login", "Account");

            int userId = int.Parse(userIdString);

            // Poori wishlist fetch karo, Product details ke saath
            var wishlist = await _apiService.GetAsync<List<WishlistPageItemModel>>($"api/Wishlist/user/{userId}", token);
            wishlist ??= new List<WishlistPageItemModel>();

            return View(wishlist);
        }

        [HttpGet]
        public async Task<IActionResult> GetWishlistProductIds()
        {
            var token = HttpContext.Session.GetString("JWToken");
            var userIdString = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(userIdString))
                return Unauthorized();

            int userId = int.Parse(userIdString);

            var wishlist = await _apiService.GetAsync<List<WishlistItemModel>>(
                $"api/Wishlist/user/{userId}",
                token);

            wishlist ??= new List<WishlistItemModel>();

            var productIds = wishlist
                .Select(w => w.ProductID)
                .ToList();

            return Json(productIds);
        }

    }


    // Wishlist API se jo response aata hai, uska shape (sirf jo chahiye wahi fields)
    public class WishlistItemModel
    {
        public int WishlistID { get; set; }
        public int ProductID { get; set; }
    }
    public class WishlistPageItemModel
    {
        public int WishlistID { get; set; }
        public int ProductID { get; set; }
        public WishlistProductModel? Product { get; set; }
    }

    public class WishlistProductModel
    {
        public string ProductName { get; set; } = "";
        public decimal Price { get; set; }
    }
}
