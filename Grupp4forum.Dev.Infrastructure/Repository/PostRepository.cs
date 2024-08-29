using Grupp4forum.Dev.Infrastructure.Configuration;
using Grupp4forum.Dev.Infrastructure.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Dapper;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grupp4forum.Dev.Infrastructure.Repository
{
    public class PostRepository
    {
        private readonly DatabaseSettings _databaseSettings;

        public PostRepository(IOptions<DatabaseSettings> databaseSettings)
        {
            _databaseSettings = databaseSettings.Value;
        }

        public async Task<IEnumerable<Post>> GetAllPosts()
        {
            IEnumerable<Post> posts;
            using (var connection = new SqlConnection(_databaseSettings.DefaultConnection))
            {
                posts = await connection.QueryAsync<Post>(@"
                    SELECT 
                        post_id AS PostId,
                        user_id AS UserId,
                        title,
                        content,
                        timestamp AS Timestamp,
                        author
                    FROM 
                        Posts
                ");
            }
            return posts;
        }

        public async Task<Post> GetPostById(int id)
        {
            using (var connection = new SqlConnection(_databaseSettings.DefaultConnection))
            {
                return await connection.QueryFirstOrDefaultAsync<Post>(@"
                    SELECT 
                        post_id AS PostId,
                        user_id AS UserId,
                        title,
                        content,
                        timestamp AS Timestamp,
                        author
                    FROM 
                        Posts
                    WHERE 
                        post_id = @Id
                ", new { Id = id });
            }
        }
       
        public async Task<bool> UpdatePost(Post post)
        {
            try
            {
                using (var connection = new SqlConnection(_databaseSettings.DefaultConnection))
                {
                    await connection.ExecuteAsync(@"

    UPDATE Posts
                        SET 
                            user_id = @UserId,
                            title = @Title,
                            content = @Content,
                            timestamp = @Timestamp
                        WHERE 
                            post_id = @PostId
                    ", post);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeletePost(int userId, int id)
        {
            //try
            {
                var isPostAuthor = await IsUserPostAuthor(userId, id);
                if (!isPostAuthor)
                {
                    var isAdminOrModerator = await IsAdminOrModerator(userId);
                    if (!isAdminOrModerator)
                    {
                        return false;
                    }
                }

                using (var connection = new SqlConnection(_databaseSettings.DefaultConnection))
                {
                    await connection.ExecuteAsync(@"
                        DELETE FROM Posts
                        WHERE 
                            post_id = @Id
                    ", new { Id = id });
                }
                return true;
            }
            //catch
            {
                return false;
            }
        }
        public async Task<bool> IsUserPostAuthor(int userId, int postId)
        {
            using (var connection = new SqlConnection(_databaseSettings.DefaultConnection))
            {
                var ownerId = await connection.ExecuteScalarAsync<int?>(@"
         SELECT user_id
         FROM Posts
         WHERE post_id = @PostId
     ", new { PostId = postId });

                return ownerId.HasValue && ownerId.Value == userId;
            }
        }
        public async Task<bool> IsAdminOrModerator(int userId)
        {
            using var connection = new SqlConnection(_databaseSettings.DefaultConnection);
            var roleId = await connection.ExecuteScalarAsync<int?>(@"
        SELECT RoleId FROM Users WHERE Id = @UserId", new { UserId = userId });

            if (roleId.HasValue && (roleId == 2 || roleId == 3))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private async Task<string> GetAuthorName(SqlConnection connection, int userId)
        {
            // Skapa SQL-frågan för att hämta författarens namn från Users-tabellen
            var authorNameQuery = @"
        SELECT Username AS Author
        FROM Users
        WHERE Id = @UserId";

            // Exekvera SQL-frågan för att hämta författarens namn
            var author = await connection.QueryFirstOrDefaultAsync<string>(authorNameQuery, new { UserId = userId });

            return author;
        }
        public async Task<bool> InsertPostWithAuthor(Post post, int userId)
        {
            try
            {
                using (var connection = new SqlConnection(_databaseSettings.DefaultConnection))
                {
                    // Hämta författarens namn baserat på användaridentifieringen
                    var author = await GetAuthorName(connection, userId);

                    // Uppdatera SQL-frågan för att infoga inlägg med författarens namn
                    await connection.ExecuteAsync(@"
                INSERT INTO Posts (user_id, title, content, timestamp, author) 
                VALUES (@UserId, @Title, @Content, @Timestamp, @Author)
            ", new
                    {
                        UserId = userId, // Användaridentifiering som författare
                        post.Title,
                        post.Content,
                        Timestamp = DateTime.UtcNow, // Sätt den aktuella tidsstämpeln
                        Author = author // Författarens namn
                    });

                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

    }
}
