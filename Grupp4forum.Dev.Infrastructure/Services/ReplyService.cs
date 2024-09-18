using Grupp4forum.Dev.Infrastructure.Configuration;
using Grupp4forum.Dev.Infrastructure.Models;
using Microsoft.Extensions.Options;
using Dapper;
using Microsoft.Data.SqlClient;
using Grupp4forum.Dev.Infrastructure.ViewModel;
using Microsoft.Extensions.Hosting;
using Grupp4forum.Dev.Infrastructure.Models;

namespace Grupp4forum.Dev.Infrastructure.Repository
{

    public class ReplyService
    {
        private readonly ReplyRepository _replyRepository;

        public ReplyService(ReplyRepository replyRepository)
        {
            _replyRepository = replyRepository;
        }

        public async Task<IEnumerable<Reply>> GetAllReplies()
        {
            return await _replyRepository.GetAllReplies();
        }

        public async Task<Reply> GetReplyById(int id)
        {
            return await _replyRepository.GetReplyById(id);
        }

        public async Task<int> CreateReply(Reply reply, int userId, int postId)
        {
            await _replyRepository.InsertReplyWithAuthor(reply, userId, postId);
            Console.WriteLine(postId);
            return reply.ReplyId;
        }

        public async Task<bool> UpdateReply(Reply reply)
        {
            return await _replyRepository.UpdateReply(reply);
        }


        public async Task<bool> RemoveReply(int userId, int id)
        {
            return await _replyRepository.DeleteReply(userId, id);
        }
        public async Task<IEnumerable<Reply>> GetRepliesByPostId(int postId)
        {
            return await _replyRepository.GetRepliesByPostId(postId);
        }

        public async Task<string> LikeReplyAsync(int replyId, int userId)
        {
            // Hämta reply för att kontrollera att den existerar
            var reply = await _replyRepository.GetReplyById(replyId);
            if (reply == null)
            {
                return "Reply kunde inte hittas.";
            }

            // Kontrollera om användaren redan har gillat replyn
            var existingLike = await _replyRepository.GetReplyLikeAsync(replyId, userId);
            if (existingLike != null)
            {
                return "Du har redan gillat denna reply.";
            }

            // Lägg till gillningen
            var replyLike = new ReplyLike
            {
                ReplyId = replyId,
                UserId = userId
            };
            await _replyRepository.AddReplyLikeAsync(replyLike);

            // Öka gillningar i replyn
            reply.Likes += 1;
            await _replyRepository.UpdateReplyLikesAsync(replyId, reply.Likes);

            return "Reply gillad!";
        }
    }

    public interface IReplyService
    {
        Task<string> LikeReplyAsync(int replyId, int userId);
    }
}
