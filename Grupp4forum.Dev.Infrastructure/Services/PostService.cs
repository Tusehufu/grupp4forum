using Grupp4forum.Dev.Infrastructure.Models;
using Grupp4forum.Dev.Infrastructure.Repository;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grupp4forum.Dev.Infrastructure.Services
{
    public class PostService
    {
        private readonly PostRepository _postRepository;

        public PostService(PostRepository postRepository)
        {
            _postRepository = postRepository;
        }

        public async Task<IEnumerable<Post>> GetAllPosts()
        {
            return await _postRepository.GetAllPosts();
        }

        public async Task<Post> GetPostById(int id)
        {
            return await _postRepository.GetPostById(id);
        }

        public async Task<int> AddPost(Post post, int userId)
        {
            var result = await _postRepository.AddPost(post, userId);
            if (result)
            {
                return post.PostId; // Returnerar det nya postens ID om tillagd
            }
            else
            {
                throw new Exception("Misslyckades med att skapa inlägget");
            }
        }

        public async Task<bool> UpdatePost(Post post)
        {
            return await _postRepository.UpdatePost(post);
        }

        public async Task<bool> RemovePost(int userId, int postId)
        {
            return await _postRepository.DeletePost(userId, postId);
        }
        public async Task<string> LikePostAsync(int postId, int userId)
        {
            // Hämta posten för att kontrollera att den existerar
            var post = await _postRepository.GetPostById(postId);
            if (post == null)
            {
                return "Posten kunde inte hittas.";
            }

            // Kontrollera om användaren redan har gillat posten
            var existingLike = await _postRepository.GetPostLikeAsync(postId, userId);
            if (existingLike != null)
            {
                return "Du har redan gillat denna post.";
            }

            // Lägg till gillningen
            var postLike = new PostLike
            {
                PostId = postId,
                UserId = userId
            };
            await _postRepository.AddPostLikeAsync(postLike);

            // Öka gillningar i posten och uppdatera
            post.Likes += 1;
            await _postRepository.UpdatePostLikesAsync(postId, post.Likes);

            return "Post gillad!";
        }
    }

    public interface IPostService
    {
        Task<string> LikePostAsync(int postId, int userId);
    }
}


