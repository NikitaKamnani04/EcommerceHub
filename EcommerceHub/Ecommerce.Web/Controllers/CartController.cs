using Ecommerce.Web.Models;
using Ecommerce.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Web.Controllers
{
    public class CartController : Controller
    {

        private readonly ApiService _apiService;

        public CartController(ApiService apiService)
        {
            _apiService = apiService;
        }

        // Ye class batati hai - jab jQuery se AJAX request aayegi,
        // usme kaunse fields honge (productId, quantity)
        // [ApiController] jaisi cheez yahan nahi chahiye kyunki ye MVC controller hai, Web API nahi

        public class AddToCartRequest
        {
            public int ProductId { get; set; }
            public int Quantity { get; set; }
        }

        // POST: /Cart/AddToCart
        // Ye method jQuery ke $.ajax() call se hit hoga (site.js mein jo likha tha)
        // [HttpPost] zaroori hai kyunki jQuery POST request bhej rahi hai

        [HttpPost]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
        {
            // Session se check karo - user login hai ki nahi
            var token = HttpContext.Session.GetString("JWToken");
            var userIdString = HttpContext.Session.GetString("UserId");

            // Agar token ya userId nahi mila, matlab user login nahi hai
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(userIdString))
            {
                // 401 Unauthorized bhejo - jQuery ka error() function ise catch karega
                // (yaad hai site.js mein humne xhr.status === 401 check kiya tha)
                return Unauthorized();
            }

            // UserId ko string se int mein convert karo (API ko int chahiye)
            int userId = int.Parse(userIdString);

            // TEMPORARY DEBUG
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest(new { message = "TOKEN IS NULL/EMPTY" });
            }
            try
            {
                // API ke "api/Cart/add" endpoint ko call karo
                // Humara ApiService ka PostAsync method use kar rahe hain, token ke saath
                // (yaad hai - Cart endpoints [Authorize] the, isliye token bhejna zaroori hai)
                await _apiService.PostAsync<object, object>("api/Cart/add", new
                {
                    userId = userId,
                    productId = request.ProductId,
                    quantity = request.Quantity
                }, token);

                // Cart mein add hone ke baad, poora cart fetch karo taaki naya count pata chale
                var cartCount = await GetCartItemCount(userId, token);

                // jQuery ko JSON response bhejo - isme "cartCount" hai jo site.js use karega
                // (yaad hai - response.cartCount se navbar badge update hota hai)
                return Json(new { success = true, cartCount = cartCount });
            }
            catch (Exception ex)
            {
                // Agar kuch galat hua (jaise stock nahi hai), error message bhejo
                return BadRequest(new { message = ex.Message });
            }
        }

        // Ye ek helper method hai - cart mein total kitne items hain, wo count karta hai
        // (badge mein number dikhane ke liye)

        private async Task<int> GetCartItemCount(int userId, string token)
        {
            var cart = await _apiService.GetAsync<CartPageModel>($"api/Cart/user/{userId}", token);   // updated

            if (cart?.CartItems == null)
                return 0;

            return cart.CartItems.Sum(item => item.Quantity);
        }

        // GET: /Cart/GetCartCount
        // Purpose: Sirf current cart count return karna (badge update karne ke liye, page load pe)
        [HttpGet]
        public async Task<IActionResult> GetCartCount()
        {
            var token = HttpContext.Session.GetString("JWToken");
            var userIdString = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(userIdString))
                return Json(new { cartCount = 0 });

            int userId = int.Parse(userIdString);
            var cartCount = await GetCartItemCount(userId, token);

            return Json(new { cartCount = cartCount });
        }

        // GET: /Cart/Index
        // Purpose: Poora cart page dikhana (jab user "Cart" link pe click kare navbar mein)
        public async Task<IActionResult> Index()
        {
            var token = HttpContext.Session.GetString("JWToken");
            var userIdString = HttpContext.Session.GetString("UserId");

            // TEMPORARY DEBUG
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(userIdString))
            {
                return Content($"DEBUG: Token={token ?? "NULL"}, UserId={userIdString ?? "NULL"}, SessionId={HttpContext.Session.Id}");
            }

            int userId = int.Parse(userIdString);
            var cart = await _apiService.GetAsync<CartPageModel>($"api/Cart/user/{userId}", token);
            cart ??= new CartPageModel { CartItems = new List<CartPageItemModel>() };

            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> Remove(int id)
        {
            var token = HttpContext.Session.GetString("JWToken");

            if (string.IsNullOrEmpty(token))
                return Unauthorized();

            try
            {
                await _apiService.DeleteAsync($"api/Cart/item/{id}", token);

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
