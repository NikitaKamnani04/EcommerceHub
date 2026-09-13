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
    public class SubCategoriesController : ControllerBase
    {
        private readonly ECommerceDbContext _context;

        public SubCategoriesController(ECommerceDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var subCategories = await _context.SubCategories.ToListAsync();
            return Ok(subCategories);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var subCategory = await _context.SubCategories.FindAsync(id);
            if (subCategory == null)
                return NotFound();

            return Ok(subCategory);
        }

        [HttpGet("by-category/{categoryId}")]
        public async Task<IActionResult> GetByCategory(int categoryId)
        {
            var subCategories = await _context.SubCategories.Where(sc=>sc.CategoryId == categoryId).ToListAsync();

            return Ok(subCategories);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(SubCategoryCreateDto dto)
        {
            if(!ModelState.IsValid)
                return BadRequest (ModelState);

            var categoryExists = await _context.Categories.AnyAsync(c => c.CategoryId == dto.CategoryID);
            if (!categoryExists)
                return BadRequest($"Category with ID {dto.CategoryID} does not exist");

            var subCategory = new SubCategory
            {
                CategoryId = dto.CategoryID,
                SubCategoryName = dto.SubCategoryName,
                Description = dto.Description
            };

            _context.SubCategories.Add(subCategory);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = subCategory.SubCategoryId }, subCategory);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, SubCategoryUpdateDto dto)
        {
            var subCategory = await _context.SubCategories.FindAsync(id);
            if (subCategory == null)
                return NotFound();

            if(dto.CategoryID.HasValue)
            {
                var categoryExists = await _context.Categories.AnyAsync(c => c.CategoryId == dto.CategoryID.Value);
                if (!categoryExists)
                    return BadRequest($"Category with ID {dto.CategoryID} does not exist");

                subCategory.CategoryId = dto.CategoryID.Value;
            }

            if (dto.SubCategoryName != null) subCategory.SubCategoryName = dto.SubCategoryName;
            if (dto.Description != null) subCategory.Description = dto.Description;
            if (dto.IsActive.HasValue) subCategory.IsActive = dto.IsActive.Value;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var subCategory = await _context.SubCategories.FindAsync(id);
            if (subCategory == null) return NotFound();

            _context.SubCategories.Remove(subCategory);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
