using System.Collections.Generic;
using System.Threading.Tasks;
using Grupp4forum.Dev.Infrastructure.Models;
using Grupp4forum.Dev.Infrastructure.Repository;

namespace Grupp4forum.Dev.API.Services
{
    public class CategoryService
    {
        private readonly CategoryRepository _categoryRepository;

        public CategoryService(CategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<IEnumerable<Category>> GetAllCategories()
        {
            return await _categoryRepository.GetAll();
        }

        public async Task<Category> GetCategoryById(int id)
        {
            return await _categoryRepository.GetById(id);
        }

        public async Task<int> AddCategory(Category category)
        {
            category.CreatedAt = DateTime.UtcNow;
            category.UpdatedAt = DateTime.UtcNow;
            return await _categoryRepository.Add(category);
        }

        public async Task<bool> UpdateCategory(Category category)
        {
            category.UpdatedAt = DateTime.UtcNow;
            return await _categoryRepository.Update(category);
        }

        public async Task<bool> DeleteCategory(int id)
        {
            return await _categoryRepository.Delete(id);
        }
    }
}
