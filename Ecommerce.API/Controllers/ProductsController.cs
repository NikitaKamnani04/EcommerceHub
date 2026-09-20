using Ecommerce.API.DTOs;
using Ecommerce.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ECommerceDbContext _context;

        public ProductsController(ECommerceDbContext context)
        {
            _context = context;
        }

        // GET: api/Products?search=phone&page=1&pageSize=12
        // Purpose: Products ki list dena, optionally search filter + pagination ke saath
        [HttpGet]
        public async Task<IActionResult> GetAll(string? search = null, int page = 1, int pageSize = 12)
        {
            //var query = _context.Products.AsNoTracking();   // NAYA - read-only query hai, tracking ki zaroorat nahi
            //// Base query banao - abhi tak database ko hit nahi kiya, sirf query "build" ho rahi hai
            //var query = _context.Products.AsQueryable();

            // Base query - read-only hai, isliye tracking ki zaroorat nahi
            var query = _context.Products
                .AsNoTracking()
                .AsQueryable();

            // Agar search term diya gaya hai, filter lagao
            if (!string.IsNullOrWhiteSpace(search))
            {
                // Contains() SQL mein "LIKE '%search%'" ban jata hai - partial match
                query = query.Where(p => p.ProductName.Contains(search));
            }

            // Total count nikaalo (pagination ke liye zaroori - pehle FILTER lagao, phir count karo)
            var totalCount = await query.CountAsync();

            // Ab actual page ka data nikaalo
            // Skip() - pichhle pages ke items ko "skip" karo
            // Take() - sirf itne hi items lo jitna ek page mein chahiye
            var products = await query.OrderBy(p => p.ProductId).Skip((page - 1) * pageSize)
                .Take(pageSize).ToListAsync();

            var result = new PagedResultDto<Product>
            {
                Items = products,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount/(double)pageSize)
            };
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            return Ok(product);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(ProductCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var product = new Product
            {
                CategoryId = dto.CategoryID,
                SubCategoryId = dto.SubCategoryID,
                BrandId = dto.BrandID,
                ProductName = dto.ProductName,
                Price = dto.Price,
                StockQuantity = dto.StockQuantity
            };
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = product.ProductId }, product);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, ProductUpdateDto dto)
        {
            var product = await _context.Products.FindAsync(id);
            if(product == null)
            {
                return NotFound();
            }

            if (dto.CategoryID.HasValue) product.CategoryId = dto.CategoryID.Value;
            if (dto.SubCategoryID.HasValue) product.SubCategoryId = dto.SubCategoryID.Value;
            if (dto.BrandID.HasValue) product.BrandId = dto.BrandID.Value;
            if (dto.ProductName != null) product.ProductName = dto.ProductName;
            if (dto.Price.HasValue) product.Price = dto.Price.Value;
            if (dto.StockQuantity.HasValue) product.StockQuantity = dto.StockQuantity.Value;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound();

            _context.Remove(product);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        //// TEMPORARY - sirf Exception Handling test karne ke liye
        //[HttpGet("test-error")]
        //public IActionResult TestError()
        //{
        //    throw new Exception("This is a test exception to check global error handling.");
        //}

        // GET: api/Products/5/recommendations
        // Purpose: "Frequently bought together" products dikhana
        [HttpGet("{id}/recommendations")]
        public async Task<IActionResult> GetRecommendations(int id)
        {
            var result = await _context.Database
                .SqlQuery<RecommendedProductDto>($"EXEC usp_GetFrequentlyBoughtTogether @ProductID = {id}")
                .ToListAsync();

            return Ok(result);
        }

    }
}
