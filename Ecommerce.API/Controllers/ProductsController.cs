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

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var products = await _context.Products.ToListAsync();
            return Ok(products);
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


    }
}
