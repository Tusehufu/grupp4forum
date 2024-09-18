using Grupp4forum.Dev.Infrastructure.Models;
using Grupp4forum.Dev.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Grupp4forum.Dev.Infrastructure.ViewModel;
using System;
using Microsoft.AspNetCore.Authorization;

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

        [HttpPost]
        public async Task<ActionResult> AddPost([FromForm] PostViewModel postViewModel, IFormFile? image)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = 1; // Användar-ID, standard för anonym användare

            var post = new Post
            {
                Title = postViewModel.Title,
                Content = postViewModel.Content,
                UserId = userId,
                CategoryId = postViewModel.CategoryId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsVisible = true
            };

            // Hantera bilduppladdningen om det finns en bild
            if (image != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await image.CopyToAsync(memoryStream);
                    post.Image = memoryStream.ToArray(); // Lagra bilden som byte-array
                }
            }

            var id = await _postService.AddPost(post, userId);

            var response = new
            {
                postId = post.PostId,
                userId = post.UserId,
                title = post.Title,
                author = post.Author ?? "Anonymous",
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
            var userId = 1; // Kan sätta till 0 eller annat värde för anonyma användare

            var result = await _postService.RemovePost(userId, id);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }

        [HttpPost("{postId}/like")]
        public async Task<IActionResult> LikePost(int postId)
        {
            var userId = 1;  // Exempel för att hämta userId från autentisering (exempelvis JWT)

            var result = await _postService.LikePostAsync(postId, userId);

            if (result == "Posten kunde inte hittas.")
            {
                return NotFound(new { message = result });
            }

            if (result == "Du har redan gillat denna post.")
            {
                return BadRequest(new { message = result });
            }

            return Ok(new { message = result });
        }
    }
}