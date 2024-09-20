using Grupp4forum.Dev.Infrastructure.Configuration;
using Grupp4forum.Dev.Infrastructure.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Dapper;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

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
                        category_id AS CategoryId,
                        title,
                        content,
                        created_at AS CreatedAt,
                        updated_at AS UpdatedAt,
                        author,
                        likes AS Likes,
                        isvisible,
                        Image
                    FROM 
                        Posts
                ");
            }
            // Konvertera bilden till Base64-sträng om den finns
            foreach (var post in posts)
            {
                if (post.Image != null && post.Image.Length > 0)
                {
                    post.ImageBase64 = Convert.ToBase64String(post.Image);
                }
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
                        category_id AS CategoryId,
                        title,
                        content,
                        created_at AS CreatedAt,
                        updated_at AS UpdatedAt,
                        likes AS Likes,
                        author
                    FROM 
                        Posts
                    WHERE 
                        post_id = @Id
                ", new { Id = id });
            }
        }

        public async Task<bool> AddPost(Post post, int userId)
        {
            try
            {
                using (var connection = new SqlConnection(_databaseSettings.DefaultConnection))
                {
                    var author = await GetAuthorName(connection, userId);

                    await connection.ExecuteAsync(@"
                INSERT INTO Posts (user_id, category_id, title, content, created_at, updated_at, author, Image) 
                VALUES (@UserId, @CategoryId, @Title, @Content, @CreatedAt, @UpdatedAt, @Author, @Image)
            ", new
                    {
                        post.UserId,
                        post.CategoryId,
                        post.Title,
                        post.Content,
                        post.CreatedAt,
                        post.UpdatedAt,
                        Author = author,
                        Image = post.Image // Lägg till bilden
                    });

                    return true;
                }
            }
            catch
            {
                return false;
            }
        }


        public async Task<bool> UpdatePost(Post post, int userId)
        {
            try
            {
                // Kontrollera om användaren är författaren till inlägget
                var isPostAuthor = await IsUserPostAuthor(userId, post.PostId);
                if (!isPostAuthor)
                {
                    // Om användaren inte är författare, kolla om de är admin eller moderator
                    var isAdminOrModerator = await IsAdminOrModerator(userId);
                    if (!isAdminOrModerator)
                    {
                        return false; // Returnera false om de inte har tillräckliga rättigheter
                    }
                }

                // Uppdatera inlägget i databasen
                using (var connection = new SqlConnection(_databaseSettings.DefaultConnection))
                {
                    await connection.ExecuteAsync(@"
                UPDATE Posts
                SET 
                    title = @Title,
                    content = @Content,
                    category_id = @CategoryId,
                    updated_at = @UpdatedAt
                WHERE 
                    post_id = @PostId AND user_id = @UserId
            ", new
                    {
                        post.Title,
                        post.Content,
                        post.CategoryId,
                        post.UpdatedAt,
                        post.PostId,
                        UserId = userId // Skicka med userId här
                    });
                }
                return true;
            }
            catch
            {
                return false; // Hantera fel om det sker något oväntat
            }
        }


        public async Task<bool> DeletePost(int userId, int id)
        {
            var isPostAuthor = await IsUserPostAuthor(userId, id);
            if (!isPostAuthor)
            {
                Console.WriteLine("Fisk");
                var isAdminOrModerator = await IsAdminOrModerator(userId);
                if (!isAdminOrModerator)
                {
                    return false;
                }
            }

            using (var connection = new SqlConnection(_databaseSettings.DefaultConnection))
            {
                await connection.ExecuteAsync(@"
                    UPDATE Posts
                    SET 
                        title = 'Raderat', 
                        content = 'Raderat',
                        isVisible = 0 
                    WHERE 
                        post_id = @Id
                ", new { Id = id });
            }

            return true;
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
                SELECT role_id FROM Users WHERE Id = @UserId", new { UserId = userId });

            return roleId.HasValue && (roleId == 2 || roleId == 3);
        }

        private async Task<string> GetAuthorName(SqlConnection connection, int userId)
        {
            var authorNameQuery = @"
                SELECT Username AS Author
                FROM Users
                WHERE Id = @UserId";

            return await connection.QueryFirstOrDefaultAsync<string>(authorNameQuery, new { UserId = userId });
        }

        // Hämta en specifik post-like för att kontrollera om användaren redan har gillat posten
        public async Task<PostLike> GetPostLikeAsync(int postId, int userId)
        {
            using (var connection = new SqlConnection(_databaseSettings.DefaultConnection))
            {
                var postLike = await connection.QuerySingleOrDefaultAsync<PostLike>(@"
                    SELECT 
                        post_id AS PostId,
                        user_id AS UserId
                    FROM post_likes
                    WHERE post_id = @PostId AND user_id = @UserId",
                    new { PostId = postId, UserId = userId });

                return postLike;
            }
        }

        // Lägg till en post-like
        public async Task AddPostLikeAsync(PostLike postLike)
        {
            using (var connection = new SqlConnection(_databaseSettings.DefaultConnection))
            {
                var query = @"
                    INSERT INTO post_likes (post_id, user_id)
                    VALUES (@PostId, @UserId)";

                await connection.ExecuteAsync(query, new { PostId = postLike.PostId, UserId = postLike.UserId });
            }
        }

        // Uppdatera en posts likes
        public async Task UpdatePostLikesAsync(int postId, int likes)
        {
            using (var connection = new SqlConnection(_databaseSettings.DefaultConnection))
            {
                var query = @"
                    UPDATE Posts
                    SET likes = @Likes
                    WHERE post_id = @PostId";

                await connection.ExecuteAsync(query, new { Likes = likes, PostId = postId });
            }
        }

        // Ta bort en post-like
        public async Task RemovePostLikeAsync(int postId, int userId)
        {
            using (var connection = new SqlConnection(_databaseSettings.DefaultConnection))
            {
                var query = @"
            DELETE FROM post_likes
            WHERE post_id = @PostId AND user_id = @UserId";

                await connection.ExecuteAsync(query, new { PostId = postId, UserId = userId });
            }
        }

        public async Task<bool> UserHasLikedPost(int postId, int userId)
        {
            using (var connection = new SqlConnection(_databaseSettings.DefaultConnection))
            {
                var query = @"
            SELECT COUNT(1) FROM post_likes
            WHERE post_id = @PostId AND user_id = @UserId";

                var hasLiked = await connection.ExecuteScalarAsync<int>(query, new { PostId = postId, UserId = userId });
                return hasLiked > 0;
            }
        }

    }

    public interface IPostRepository
    {
        Task<Post> GetPostByIdAsync(int postId);
        Task<PostLike> GetPostLikeAsync(int postId, int userId);
        Task AddPostLikeAsync(PostLike postLike);
        Task UpdatePostLikesAsync(int postId, int likes);
    }
}

