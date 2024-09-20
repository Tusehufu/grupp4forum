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
            return reply.ReplyId;
        }

        public async Task<bool> UpdateReply(Reply reply, int ReplyId)
        {
            return await _replyRepository.UpdateReply(reply, ReplyId);
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

        public async Task<string> UnlikeReplyAsync(int replyId, int userId)
        {
            var reply = await _replyRepository.GetReplyById(replyId);
            if (reply == null)
            {
                return "Reply kunde inte hittas.";
            }

            var existingLike = await _replyRepository.GetReplyLikeAsync(replyId, userId);
            if (existingLike == null)
            {
                return "Du har inte gillat denna reply.";
            }

            await _replyRepository.RemoveReplyLikeAsync(replyId, userId);
            reply.Likes -= 1;
            await _replyRepository.UpdateReplyLikesAsync(replyId, reply.Likes);

            return "Reply ogillad!";
        }

        public async Task<bool> HasUserLikedReply(int replyId, int userId)
        {
            // Anropar repository för att kolla om användaren har gillat replyn
            return await _replyRepository.UserHasLikedReply(replyId, userId);
        }

        public async Task<bool> CanEditReply(int userId, int replyId)
        {
            var isAuthor = await _replyRepository.IsUserReplyAuthor(userId, replyId);
            var isAdminOrModerator = await _replyRepository.IsAdminOrModerator(userId);

            return isAuthor || isAdminOrModerator;
        }
    }



    public interface IReplyService
    {
        Task<string> LikeReplyAsync(int replyId, int userId);
        Task<bool> CanEditReply(int userId, int replyId);

    }
}

