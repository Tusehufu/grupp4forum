using Grupp4forum.Dev.Infrastructure.Models;
using Grupp4forum.Dev.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Grupp4forum.Dev.Infrastructure.ViewModel;
using System;

namespace Grupp4forum.Dev.App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostController : ControllerBase
    {
        private readonly PostService _postService;

        public PostController(PostService postService)
        {
            _postService = postService;
        }

        // Hämta alla inlägg
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Post>>> GetAllPosts()
        {
            var posts = await _postService.GetAllPosts();
            return Ok(posts);
        }

        // Hämta ett specifikt inlägg via ID
        [HttpGet("{id}")]
        public async Task<ActionResult<Post>> GetPostById(int id)
        {
            var post = await _postService.GetPostById(id);
            if (post == null)
            {
                return NotFound();
            }
            return Ok(post);
        }

        // Lägg till ett nytt inlägg
        [HttpPost]
        public async Task<ActionResult> AddPost(PostViewModel postViewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Här behövs ingen användarverifiering längre
            var userId = 1; // Kan sätta till 0 eller annat värde för anonyma användare

            var post = new Post
            {
                Title = postViewModel.Title,
                Content = postViewModel.Content,
                UserId = userId, // Anonym användare eller standardvärde
                CategoryId = postViewModel.CategoryId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsVisible = true
            };

            var id = await _postService.AddPost(post, userId);
            var author = post.Author;
            var response = new
            {
                postId = post.PostId,
                userId = post.UserId,
                title = post.Title,
                author = post.Author ?? "Anonymous",  // Om författaren är null, returnera "Anonymous"
                isVisible = post.IsVisible,
                likes = post.Likes,
                content = post.Content,
                categoryId = post.CategoryId,
                createdAt = post.CreatedAt,
                updatedAt = post.UpdatedAt
            };

            return Ok(response);
        }

        // Uppdatera ett inlägg
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePost(int id, PostViewModel postViewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Här behövs ingen användarverifiering längre
            var userId = 1; // Kan sätta till 0 eller annat värde för anonyma användare

            var post = new Post
            {
                PostId = id, // Sätt inläggets ID
                Title = postViewModel.Title,
                Content = postViewModel.Content,
                CategoryId = postViewModel.CategoryId,
                UserId = userId, // Anonym användare eller standardvärde
                UpdatedAt = DateTime.UtcNow // Uppdaterad tid
            };

            var success = await _postService.UpdatePost(post);
            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }

        // Ta bort ett inlägg
        [HttpDelete("{id}")]
        public async Task<IActionResult> RemovePost(int id)
        {
            // Här behövs ingen användarverifiering längre
            var userId = 2; // Kan sätta till 0 eller annat värde för anonyma användare

            var result = await _postService.RemovePost(userId, id);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}
