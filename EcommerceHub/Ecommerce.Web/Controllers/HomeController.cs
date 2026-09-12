using Ecommerce.Web.Models;
using Ecommerce.Web.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Ecommerce.Web.Controllers
{
    public class HomeController : Controller
    {
        // ApiService ko yahan store karenge - taaki API calls kar sakein
        private readonly ApiService _apiService;

        // Constructor - jab HomeController banega, ASP.NET Core khud
        // ApiService ka instance yahan "inject" kar dega (Dependency Injection)
        // (yaad hai humne Program.cs mein ApiService register kiya tha - isiliye ye kaam karta hai)
        public HomeController(ApiService apiService)
        {
            _apiService = apiService;
        }

        // Index() - ye method chalta hai jab koi "/" ya "/Home/Index" URL pe aata hai
        // Ye humara Homepage hai
        public async Task<IActionResult> Index()
        {
            try
            {
                // ApiService se "api/Products" endpoint call karo
                // <List<ProductViewModel>> batata hai - hume products ki LIST chahiye
                // Agar API fail ho jaye (server band ho, ya error aaye), toh null milega
                var products = await _apiService.GetAsync<List<ProductViewModel>>("api/Products");

                // Agar products null aaye (API se koi response nahi mila),
                // toh ek KHAALI list bana do - taaki View mein crash na ho
                // (View mein hum products.Count wagera use karenge, null pe crash karega)
                products ??= new List<ProductViewModel>();

                // View() ko products bhej rahe hain - ye "Model" ban jayega View ke liye
                // Matlab humara Index.cshtml page ab ye products dekh sakega
                return View(products);

            }

            catch (Exception ex)
            {
                // TEMPORARY: asli error dekhne ke liye
                ViewBag.DebugError = ex.Message;
                return View(new List<ProductViewModel>());
            }
        }
    }
}
