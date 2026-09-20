using Ecommerce.Web.Models;
using Ecommerce.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : Controller
    {
        // Wahi ApiService use karenge jo HomeController mein bhi use kiya tha
        private readonly ApiService _apiService;

        // Constructor - Dependency Injection se ApiService yahan aayega automatically
        public ProductsController(ApiService apiService)
        {
            _apiService = apiService;
        }

        // Details() method - jab URL "/Products/Details/5" jaisa hoga, ye chalega
        // "int id" - URL se product ki ID yahan automatically aa jayegi (jaise 5)
        public async Task<IActionResult> Details(int id)
        {
            // API ko call karo specific product ki detail lene ke liye
            // "api/Products/5" jaisa URL banega (id automatically jud jayegi)
            var product = await _apiService.GetAsync<ProductViewModel>($"api/products/{id}");

            // Agar product nahi mila (galat ID, ya API down hai),
            // toh "404 Not Found" page dikhao - user ko pata chale ki product exist nahi karta

            if (product == null)
                return NotFound();

            // NAYA: Recommendations bhi fetch karo isi product ke liye
            var recommendations = await _apiService.GetAsync<List<RecommendedProductViewModel>>(
                $"api/Products/{id}/recommendations");

            // ViewBag se recommendations bhej rahe hain - alag se, taaki Model wahi ProductViewModel rahe
            ViewBag.Recommendations = recommendations ?? new List<RecommendedProductViewModel>();

            // Product mil gaya - View() ko bhej do, taaki Details.cshtml page ise dikha sake
            return View(product);
        }

    }
}
