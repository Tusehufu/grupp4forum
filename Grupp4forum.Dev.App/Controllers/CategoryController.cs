using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Grupp4forum.Dev.API.Services;
using Grupp4forum.Dev.Infrastructure.Models;
using Grupp4forum.Dev.Infrastructure.ViewModel;

namespace Grupp4forum.Dev.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly CategoryService _categoryService;

        public CategoryController(CategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        // Hämta alla kategorier
        [HttpGet]
        public async Task<IEnumerable<Category>> GetAll()
        {
            return await _categoryService.GetAllCategories();
        }

        // Hämta en kategori via ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var category = await _categoryService.GetCategoryById(id);

            if (category == null)
            {
                return NotFound();
            }

            return Ok(category);
        }

        // Skapa en ny kategori
        [HttpPost]
        public async Task<IActionResult> Add(CategoryViewModel categoryViewModel)
        {
            var category = new Category
            {
                Name = categoryViewModel.Name,
                Description = categoryViewModel.Description
            };

            var categoryId = await _categoryService.AddCategory(category);
            
            category.CategoryId = categoryId;

            return CreatedAtAction(nameof(GetById), new { id = categoryId }, category);
        }

        // Uppdatera en kategori
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, CategoryViewModel categoryViewModel)
        {
            var existingCategory = await _categoryService.GetCategoryById(id);
            if (existingCategory == null)
            {
                return NotFound();
            }

            existingCategory.Name = categoryViewModel.Name;
            existingCategory.Description = categoryViewModel.Description;

            bool success = await _categoryService.UpdateCategory(existingCategory);
            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }

        // Ta bort en kategori
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            bool success = await _categoryService.DeleteCategory(id);
            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
