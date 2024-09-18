using Grupp4forum.Dev.Infrastructure.Configuration;
using Grupp4forum.Dev.Infrastructure.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Dapper;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace Grupp4forum.Dev.Infrastructure.Repository
{
    public class ReplyRepository
    {
        private readonly DatabaseSettings _databaseSettings;

        public ReplyRepository(IOptions<DatabaseSettings> databaseSettings)
        {
            _databaseSettings = databaseSettings.Value;
        }

        public async Task<IEnumerable<Reply>> GetAllReplies()
        {
            IEnumerable<Reply> replies;
            using (var connection = new SqlConnection(_databaseSettings.DefaultConnection))
            {
                replies = await connection.QueryAsync<Reply>(@"
                    SELECT 
                        reply_id AS ReplyId,
                        post_id AS PostId,
                        user_id AS UserId,
                        content AS Content,
                        parent_reply_id AS ParentReplyId,
                        likes AS Likes,
                        created_at AS CreatedAt,
                        updated_at AS UpdatedAt,
                        isvisible as IsVisible,
                        Image
                    FROM 
                        Replies
                ");
            }
            foreach (var reply in replies)
            {
                if (reply.Image != null && reply.Image.Length > 0)
                {
                    reply.ImageBase64 = Convert.ToBase64String(reply.Image);
                }
            }
            return replies;
        }

        public async Task<Reply> GetReplyById(int id)
        {
            using (var connection = new SqlConnection(_databaseSettings.DefaultConnection))
            {
                return await connection.QueryFirstOrDefaultAsync<Reply>(@"
                    SELECT 
                         reply_id AS ReplyId,
                        post_id AS PostId,
                        user_id AS UserId,
                        content AS Content,
                        parent_reply_id AS ParentReplyId,
                        likes AS Likes,
                        created_at AS CreatedAt,
                        updated_at AS UpdatedAt,
                        isvisible as IsVisible,
                        Image
                    FROM 
                        Replies
                    WHERE 
                        reply_id = @Id
                ", new { Id = id });
            }
        }

        public async Task<bool> InsertReply(Reply reply, int userId)
        {
            try
            {
                using (var connection = new SqlConnection(_databaseSettings.DefaultConnection))
                {
                    // Modifierad SQL för att hämta det genererade ReplyId
                    var query = @"
                INSERT INTO Replies (user_id, post_id, content, parent_reply_id, likes, created_at, updated_at, isvisible, Image) 
                OUTPUT INSERTED.reply_id
                VALUES (@UserId, @PostId, @Content, @ParentReplyId, @Likes, @CreatedAt, @UpdatedAt, @IsVisible, @Image);
            ";

                    // Använd ExecuteScalarAsync för att hämta det nya ReplyId
                    reply.ReplyId = await connection.ExecuteScalarAsync<int>(query, new
                    {
                        reply.UserId,
                        reply.PostId,
                        reply.Content,
                        reply.ParentReplyId,
                        reply.Likes,
                        reply.CreatedAt,
                        reply.UpdatedAt,
                        reply.IsVisible,
                        Image = reply.Image
                    });
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fel vid insättning av svar: {ex.Message}");
                return false;
            }
        }


        public async Task<bool> UpdateReply(Reply reply)
        {
            try
            {
                using (var connection = new SqlConnection(_databaseSettings.DefaultConnection))
                {
                    await connection.ExecuteAsync(@"
                UPDATE Replies
                SET 
                    content = @Content,
                    updated_at = @UpdatedAt
                WHERE 
                    reply_id = @ReplyId AND user_id = @UserId
            ", reply);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }


        public async Task<bool> DeleteReply(int userId, int id)
        {
            try
            {
                //var isReplyAuthor = await IsUserReplyAuthor(userId, id);
                //if(!isReplyAuthor) 
                //{
                //    var isAdminOrModerator = await IsAdminOrModerator(userId);
                //    if (!isAdminOrModerator) 
                //    {
                //        return false;
                //    }
                //}

                using (var connection = new SqlConnection(_databaseSettings.DefaultConnection))
                {
                    await connection.ExecuteAsync(@"
                    UPDATE Replies
                    SET  
                        content = 'Raderat'
                    WHERE 
                        reply_id = @Id
                ", new { Id = id });
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<IEnumerable<Reply>> GetRepliesByPostId(int postId)
        {
            using (var connection = new SqlConnection(_databaseSettings.DefaultConnection))
            {
                var sql = @"
        SELECT 
            r.reply_id AS ReplyId,
            r.post_id AS PostId,
            r.user_id AS UserId,
            r.content AS Content,
            r.parent_reply_id AS ParentReplyId,
            r.likes AS Likes,
            r.created_at AS CreatedAt,
            r.updated_at AS UpdatedAt,
            r.isvisible AS IsVisible,
            u.Username AS Author,   
            r.Image

        FROM 
            Replies r
        INNER JOIN 
            Users u ON r.user_id = u.Id  
        WHERE 
            r.post_id = @PostId
        ";

                // Hämta alla replies för det givna postId
                var replies = await connection.QueryAsync<Reply>(sql, new { PostId = postId });

                // Gå igenom varje reply och omvandla binär bilddata till Base64-sträng
                foreach (var reply in replies)
                {
                    if (reply.Image != null && reply.Image.Length > 0)
                    {
                        // Konvertera binärdata till Base64-sträng
                        reply.ImageBase64 = Convert.ToBase64String(reply.Image);
                    }
                }

                return replies;
            }
        }


        public async Task<bool> InsertReplyWithAuthor(Reply reply, int userId, int postId)
        {
            try
            {
                using (var connection = new SqlConnection(_databaseSettings.DefaultConnection))
                {
                    var query = @"
                INSERT INTO Replies (user_id, post_id, content, parent_reply_id, likes, created_at, updated_at) 
                OUTPUT INSERTED.reply_id
                VALUES (@UserId, @PostId, @Content, @ParentReplyId, @Likes, @CreatedAt, @UpdatedAt);
            ";

                    reply.ReplyId = await connection.ExecuteScalarAsync<int>(query, new
                    {
                        reply.UserId,
                        reply.PostId,
                        reply.Content,
                        reply.ParentReplyId,
                        reply.Likes,
                        reply.CreatedAt,
                        reply.UpdatedAt, 
                    });
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fel vid insättning av svar: {ex.Message}");
                return false;
            }
        }

        private async Task<string> GetReplyAuthorName(SqlConnection connection, int userId)
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
        public async Task<bool> IsUserReplyAuthor(int userId, int replyId)
        {
            using (var connection = new SqlConnection(_databaseSettings.DefaultConnection))
            {
                var ownerId = await connection.ExecuteScalarAsync<int?>(@"
         SELECT user_id
         FROM Replies
         WHERE reply_id = @ReplyId
     ", new { ReplyId = replyId });

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
        // Kontrollera om användaren redan har gillat en reply
        public async Task<ReplyLike> GetReplyLikeAsync(int replyId, int userId)
        {
            using (var connection = new SqlConnection(_databaseSettings.DefaultConnection))
            {
                var replyLike = await connection.QuerySingleOrDefaultAsync<ReplyLike>(@"
                    SELECT 
                        reply_id AS ReplyId,
                        user_id AS UserId
                    FROM reply_likes
                    WHERE reply_id = @ReplyId AND user_id = @UserId",
                    new { ReplyId = replyId, UserId = userId });

                return replyLike;
            }
        }

        // Lägg till en reply-like
        public async Task AddReplyLikeAsync(ReplyLike replyLike)
        {
            using (var connection = new SqlConnection(_databaseSettings.DefaultConnection))
            {
                var query = @"
                    INSERT INTO reply_likes (reply_id, user_id)
                    VALUES (@ReplyId, @UserId)";

                await connection.ExecuteAsync(query, new { ReplyId = replyLike.ReplyId, UserId = replyLike.UserId });
            }
        }

        // Uppdatera likes för en reply
        public async Task UpdateReplyLikesAsync(int replyId, int likes)
        {
            using (var connection = new SqlConnection(_databaseSettings.DefaultConnection))
            {
                var query = @"
                    UPDATE replies
                    SET likes = @Likes
                    WHERE reply_id = @ReplyId";

                await connection.ExecuteAsync(query, new { Likes = likes, ReplyId = replyId });
            }
        }
    }

    public interface IReplyRepository
    {
        Task<Reply> GetReplyByIdAsync(int replyId);
        Task<ReplyLike> GetReplyLikeAsync(int replyId, int userId);
        Task AddReplyLikeAsync(ReplyLike replyLike);
        Task UpdateReplyLikesAsync(int replyId, int likes);
    }

}
