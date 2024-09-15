﻿using Grupp4forum.Dev.Infrastructure.Configuration;
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
                        isvisible
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
                        category_id AS CategoryId,
                        title,
                        content,
                        created_at AS CreatedAt,
                        updated_at AS UpdatedAt,
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
                        INSERT INTO Posts (user_id, category_id, title, content, created_at, updated_at, author) 
                        VALUES (@UserId, @CategoryId, @Title, @Content, @CreatedAt, @UpdatedAt, @Author)
                    ", new
                    {
                        post.UserId,
                        post.CategoryId,
                        post.Title,
                        post.Content,
                        post.CreatedAt,
                        post.UpdatedAt,
                        Author = author
                    });

                    return true;
                }
            }
            catch
            {
                return false;
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
                            title = @Title,
                            content = @Content,
                            category_id = @CategoryId,
                            updated_at = @UpdatedAt
                        WHERE 
                            post_id = @PostId AND user_id = @UserId
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
                    UPDATE Posts
                    SET 
                        title = 'Raderat', 
                        content = 'Raderat'
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
    }
}
