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
                        isvisible as IsVisible
                    FROM 
                        Replies
                ");
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
                        isvisible as IsVisible
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
                    await connection.ExecuteAsync(@"
                        INSERT INTO Replies (user_id, post_id, content, parent_reply_id, likes, created_at, updated_at, isvisible) 
                        VALUES (@UserId, @PostId, @Content, @ParentReplyId, @Likes, @CreatedAt, @UpdatedAt, ,@IsVisible)
                    ", new
                    {
                        reply.UserId,
                        reply.PostId,
                        reply.Content,
                        reply.ParentReplyId,
                        reply.Likes,
                        reply.CreatedAt,
                        reply.UpdatedAt,
                        reply.IsVisible,
                    });
                }
                return true;
            }
            catch
            {
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
                            user_id = @UserId,
                            content = @Content,
                            timestamp = @Timestamp
                        WHERE 
                            reply_id = @ReplyId
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
                r.user_id AS UserId,
                r.content AS Content,
                r.timestamp AS Timestamp
            FROM 
                Replies r
            INNER JOIN 
                PostReplies pr ON r.reply_id = pr.reply_id
            WHERE 
                pr.post_id = @PostId
        ";
                return await connection.QueryAsync<Reply>(sql, new { PostId = postId });
            }
        }
        public async Task<bool> InsertReplyWithAuthor(Reply reply, int userId, int postId)
        {
            try
            {
                using (var connection = new SqlConnection(_databaseSettings.DefaultConnection))
                {
                    // Hämta författarens namn baserat på användaridentifieringen
                    var author = await GetReplyAuthorName(connection, userId);

                    // Uppdatera SQL-frågan för att infoga inlägg med författarens namn
                    await connection.ExecuteAsync(@"
                INSERT INTO Replies (user_id, content, timestamp, author, post_id) 
                VALUES (@UserId, @Content, @Timestamp, @Author, @PostId)
            ", new
                    {
                        UserId = userId, // Användaridentifiering som författare
                        reply.Content,
                        Timestamp = DateTime.UtcNow, // Sätt den aktuella tidsstämpeln
                        Author = author, // Författarens namn
                        PostId = postId, // Postens ID
                    });
                    Console.WriteLine(postId);

                    // Hämta ID för den insatta replyn
                    var replyId = await connection.ExecuteScalarAsync<int>("SELECT CAST(SCOPE_IDENTITY() as int)");
                    // Lägg till postens ID och replyns ID i PostReplies-tabellen
                    await connection.ExecuteAsync(@"
                INSERT INTO PostReplies (post_id, reply_id)
                VALUES (@PostId, @ReplyId)
            ", new
                    {
                        PostId = postId,
                        ReplyId = replyId
                    });

                    return true;
                }
            }
            catch
            {
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
    }
}
