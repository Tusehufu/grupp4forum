using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Grupp4forum.Dev.Infrastructure.Configuration;
using Grupp4forum.Dev.Infrastructure.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace Grupp4forum.Dev.Infrastructure.Repository
{
    public class CategoryRepository
    {
        private readonly DatabaseSettings _databaseSettings;

        public CategoryRepository(IOptions<DatabaseSettings> databaseSettings)
        {
            _databaseSettings = databaseSettings.Value;
        }

        // Hämta alla kategorier
        public async Task<IEnumerable<Category>> GetAll()
        {
            using var connection = new SqlConnection(_databaseSettings.DefaultConnection);
            var query = @"
        SELECT category_id AS CategoryId, 
               name AS Name, 
               description AS Description, 
               created_at AS CreatedAt, 
               updated_at AS UpdatedAt
        FROM Categories";

            return await connection.QueryAsync<Category>(query);
        }

        // Hämta en kategori efter ID
        public async Task<Category> GetById(int id)
        {
            using var connection = new SqlConnection(_databaseSettings.DefaultConnection);
            var query = @"
        SELECT category_id AS CategoryId, 
               name AS Name, 
               description AS Description, 
               created_at AS CreatedAt, 
               updated_at AS UpdatedAt
        FROM Categories 
        WHERE category_id = @Id";

            return await connection.QueryFirstOrDefaultAsync<Category>(query, new { Id = id });
        }


        // Lägg till en ny kategori
        public async Task<int> Add(Category category)
        {
            using var connection = new SqlConnection(_databaseSettings.DefaultConnection);
            var query = @"
        INSERT INTO Categories (name, description, created_at, updated_at)
        OUTPUT INSERTED.category_id
        VALUES (@Name, @Description, @CreatedAt, @UpdatedAt)";

            return await connection.ExecuteScalarAsync<int>(query, new
            {
                category.Name,
                category.Description,
                category.CreatedAt,
                category.UpdatedAt
            });
        }



        // Uppdatera en kategori
        public async Task<bool> Update(Category category)
        {
            using var connection = new SqlConnection(_databaseSettings.DefaultConnection);
            var query = @"
        UPDATE Categories
        SET name = @Name, description = @Description, updated_at = @UpdatedAt
        WHERE category_id = @CategoryId";
            int affectedRows = await connection.ExecuteAsync(query, new
            {
                category.Name,
                category.Description,
                category.UpdatedAt,
                category.CategoryId
            });
            return affectedRows > 0;
        }


        // Ta bort en kategori
        public async Task<bool> Delete(int id)
        {
            using var connection = new SqlConnection(_databaseSettings.DefaultConnection);
            var query = "DELETE FROM Categories WHERE category_id = @Id";
            int affectedRows = await connection.ExecuteAsync(query, new { Id = id });
            return affectedRows > 0;
        }

    }
}
