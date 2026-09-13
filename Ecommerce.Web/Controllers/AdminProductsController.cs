using Ecommerce.Web.Models;
using Ecommerce.Web.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;

namespace Ecommerce.Web.Controllers
{
    // Ye poora controller sirf Admin ke liye hai
    public class CategoryApiModel
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; } = "";
    }

    public class SubCategoryApiModel
    {
        public int SubCategoryID { get; set; }
        public string SubCategoryName { get; set; } = "";
        public int CategoryID { get; set; }
    }

    public class BrandApiModel
    {
        public int BrandID { get; set; }
        public string BrandName { get; set; } = "";
    }
    public class AdminProductsController : Controller
    {

        private readonly ApiService _apiService;

        public AdminProductsController(ApiService apiService)
        {
            _apiService = apiService;
        }

        // Har method mein baar-baar "Role == Admin" check likhna avoid karne ke liye,
        // ek chhota helper method banaya hai - jo bhi Admin nahi hai, use turant bahar bhej dega
        private IActionResult? CheckAdminAccess()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin")
                return RedirectToAction("Index", "Home");

            return null;   // null matlab Admin hai, aage badhne do
        }

        // GET: /AdminProducts/Index
        // Purpose: Saare products ki list dikhana, edit/delete buttons ke saath
        public async Task<IActionResult> Index()
        {
            var accessCheck = CheckAdminAccess();
            if (accessCheck != null) return accessCheck;

            var token = HttpContext.Session.GetString("JWToken");

            var products = await _apiService.GetAsync<List<ProductListItemModel>>("api/Products",token);
            products ??= new List<ProductListItemModel>();

            return View(products);

        }

        // GET: /AdminProducts/Create
        // Purpose: Naya product add karne ka form dikhana
        public async Task<IActionResult> Create()
        {
            var accessCheck = CheckAdminAccess();
            if (accessCheck != null) return accessCheck;

            var token = HttpContext.Session.GetString("JWToken");

            // Saare dropdowns ka data fetch karo API se
            var model = await BuildProductFormDropdowns(token);

            return View(model);
        }

        // Ye helper method categories/subcategories/brands fetch karta hai
        // (Create aur Edit dono mein use hoga, isliye alag method banaya - DRY principle)
        private async Task<ProductCreateViewModel> BuildProductFormDropdowns(string? token)
        {
            // API se raw data fetch karo (Category/SubCategory/Brand ke apne existing shapes mein)
            var categories = await _apiService.GetAsync<List<CategoryApiModel>>("api/Categories", token) ?? new();
            var subCategories = await _apiService.GetAsync<List<SubCategoryApiModel>>("api/SubCategories", token) ?? new();
            var brands = await _apiService.GetAsync<List<BrandApiModel>>("api/Brands", token) ?? new();

            return new ProductCreateViewModel
            {
                // Raw API data ko DropdownItem shape mein convert kar rahe hain (sirf Id aur Name chahiye)
                Categories = categories.Select(c => new DropdownItem { Id = c.CategoryID, Name = c.CategoryName }).ToList(),
                SubCategories = subCategories.Select(sc=> new SubCategoryDropdownItem
                {
                    Id = sc.SubCategoryID,
                    Name = sc.SubCategoryName,
                    CategoryId = sc.CategoryID
                }).ToList(),
                Brands = brands.Select(b=> new DropdownItem {Id = b.BrandID, Name = b.BrandName }).ToList()
            };
        }

        // POST: /AdminProducts/Create
        
        [HttpPost]
        public async Task<IActionResult> Create(ProductCreateViewModel viewModel)
        {
            var accessCheck = CheckAdminAccess();
            if (accessCheck != null) return accessCheck;

            var token = HttpContext.Session.GetString("JWToken");

            if (!ModelState.IsValid)
            {
                // Form invalid hai - dropdowns dobara load karke wapas dikhao
                var dropdowns = await BuildProductFormDropdowns(token);
                viewModel.Categories = dropdowns.Categories;
                viewModel.SubCategories = dropdowns.SubCategories;
                viewModel.Brands = dropdowns.Brands;
                return View(viewModel);
            }

            try
            {
                // Ab viewModel.Product mein hi actual form data hai
                await _apiService.PostAsync<ProductCreateFormModel, object>("api/Products", viewModel.Product, token);
                TempData["SuccessMessage"] = "Product added successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                var dropdowns = await BuildProductFormDropdowns(token);
                viewModel.Categories = dropdowns.Categories;
                viewModel.SubCategories = dropdowns.SubCategories;
                viewModel.Brands = dropdowns.Brands;
                ModelState.AddModelError("", "Failed to add product: " + ex.Message);
                return View(viewModel);
            }
        }

        // GET: /AdminProducts/Edit/5
        // Purpose: Existing product ka form dikhana, values pehle se bhare hue

        public async Task<IActionResult> Edit(int id)
        {
            var accessCheck = CheckAdminAccess();
            if (accessCheck != null) return accessCheck;

            var token = HttpContext.Session.GetString("JWToken");

            // Product ki current details fetch karo API se
            var product = await _apiService.GetAsync<ProductListItemModel>($"api/Products/{id}", token);

            if (product == null)
                return NotFound();


            // Dropdowns ka data fetch karo (Categories, SubCategories, Brands - sab)
            var dropdowns = await BuildProductFormDropdowns(token);

            // Fetched data ko Edit form ke shape mein convert karo
            // ViewModel banao - Product ki current values + saare dropdown options
            var viewModel = new ProductEditViewModel
            {
                Product = new ProductEditFormModel
                {
                    ProductId = product.ProductId,
                    CategoryID = product.CategoryId,        // Ye already current value hai - dropdown mein "selected" khud ho jayega
                    SubCategoryID = product.SubCategoryId,
                    BrandID = product.BrandId,
                    ProductName = product.ProductName,
                    Price = product.Price,
                    StockQuantity = product.StockQuantity
                },
                Categories = dropdowns.Categories,
                SubCategories = dropdowns.SubCategories,
                Brands = dropdowns.Brands
            };

            return View(viewModel);
        }

        // POST: /AdminProducts/Edit/5
        [HttpPost]
        [HttpPost]
        public async Task<IActionResult> Edit(int id, ProductEditViewModel viewModel)
        {
            var accessCheck = CheckAdminAccess();
            if (accessCheck != null) return accessCheck;

            var token = HttpContext.Session.GetString("JWToken");

            if (!ModelState.IsValid)
            {
                var dropdowns = await BuildProductFormDropdowns(token);
                viewModel.Categories = dropdowns.Categories;
                viewModel.SubCategories = dropdowns.SubCategories;
                viewModel.Brands = dropdowns.Brands;
                return View(viewModel);
            }

            try
            {
                bool success = await _apiService.PutAsync($"api/Products/{id}", viewModel.Product, token);

                if (success)
                {
                    TempData["SuccessMessage"] = "Product updated successfully!";
                    return RedirectToAction("Index");
                }

                ModelState.AddModelError("", "Failed to update product.");
                var dropdowns2 = await BuildProductFormDropdowns(token);
                viewModel.Categories = dropdowns2.Categories;
                viewModel.SubCategories = dropdowns2.SubCategories;
                viewModel.Brands = dropdowns2.Brands;
                return View(viewModel);
            }
            catch (Exception ex)
            {
                var dropdowns3 = await BuildProductFormDropdowns(token);
                viewModel.Categories = dropdowns3.Categories;
                viewModel.SubCategories = dropdowns3.SubCategories;
                viewModel.Brands = dropdowns3.Brands;
                ModelState.AddModelError("", "Error: " + ex.Message);
                return View(viewModel);
            }
        }
        // POST: /AdminProducts/Delete/5
        // Purpose: Product delete karna - ye form-submit se hoga (button click), AJAX nahi
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var accessCheck = CheckAdminAccess();
            if (accessCheck != null) return accessCheck;

            var token = HttpContext.Session.GetString("JWToken");

            bool success = await _apiService.DeleteAsync($"api/Products/{id}", token);

            if (success)
                TempData["SuccessMessage"] = "Product deleted successfully!";
            else
                TempData["ErrorMessage"] = "Failed to delete product. It may be referenced by existing orders.";

            return RedirectToAction("Index");
        }


    }
}
