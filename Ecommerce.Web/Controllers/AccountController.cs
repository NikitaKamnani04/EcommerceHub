using Ecommerce.Web.Models;
using Ecommerce.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Web.Controllers
{
    
    public class AccountController : Controller
    {

        private readonly ApiService _apiService;

        public AccountController(ApiService apiService)
        {
          _apiService = apiService;
        }

        // GET: /Account/Login
        // Ye method sirf LOGIN FORM dikhata hai (khaali form)
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        // Ye method tab chalta hai jab user form SUBMIT karta hai
        // [HttpPost] zaroori hai - warna ye method GET request pe bhi chal jayega, jo galat hai

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            // ModelState.IsValid check karta hai - humare [Required], [EmailAddress] attributes
            // (jo LoginViewModel mein likhe the) sahi se follow hue ki nahi
            if (!ModelState.IsValid)
                return View(model); // Form dobara dikhao, saath mein error messages

            try
            {
                // API ke "api/Users/login" endpoint ko call karo, LoginViewModel data bhej ke
                var result = await _apiService.PostAsync<LoginViewModel, LoginResponseModel>(
                    "api/Users/login", model);

                if (result != null)
                {
                    // Token ko Session mein store karo - taaki future requests mein use kar sakein
                    HttpContext.Session.SetString("JWToken", result.Token);
                    HttpContext.Session.SetString("Username", result.Username);
                    HttpContext.Session.SetString("UserId", result.UserId.ToString());
                    HttpContext.Session.SetString("Role", result.Role);

                    if(result.Role == "Admin")
                    {
                        return RedirectToAction("Index", "Dashboard");
                    }
                    //login sucessful
                    return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception ex)
            {
                // Agar login fail hua (galat password, user nahi mila, etc.)
                // ModelState.AddModelError - ye error View mein dikhane ke liye hai
                ModelState.AddModelError("", "Invalid email or password");
            }

            return View(model);   // Kuch galat hua - form dobara dikha
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                // API ke register endpoint ko call karo
                // <RegisterViewModel, object> - "object" isliye kyunki response ka exact shape 
                // humein abhi important nahi, bas "successful hua ki nahi" jaanna hai

                await _apiService.PostAsync<RegisterViewModel, object>("api/Users/register",model);

                // Registration successful - Login page pe bhej do (taaki wo login kar sake)
                TempData["SuccessMessage"] = "Registration succesful! Please login";
                return RedirectToAction("Login");
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Registration failed. Email or username may already be taken.");
                return View(model);
            }
        }


        // GET: /Account/Logout
        public IActionResult Logout()
        {
            // Session poori tarah clear kar do - matlab token, username sab hat jayega
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}
    

