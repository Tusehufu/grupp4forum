using Grupp4forum.Dev.Infrastructure.Models;
using Grupp4forum.Dev.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Grupp4forum.Dev.Infrastructure.ViewModel;
using System;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

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
        [Authorize]
        [HttpPost]
        public async Task<ActionResult> AddPost([FromForm] PostViewModel postViewModel, IFormFile? image)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
           
            var userIdClaim = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            // Om användaren inte är inloggad, hantera som anonym eller returnera obehörigt
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized();
            }

            // Konvertera användarens ID från string till int (eller annan relevant datatyp)
            var userId = int.Parse(userIdClaim);
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
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePost(int id, PostViewModel postViewModel)
        {
            // Hämta användarens ID från ClaimsPrincipal (den inloggade användaren)
            var userIdClaim = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            // Om användaren inte är inloggad, hantera som anonym eller returnera obehörigt
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized();
            }

            // Konvertera användarens ID från string till int (eller annan relevant datatyp)
            var userId = int.Parse(userIdClaim);

            // Skapa post-objektet med userId
            var post = new Post
            {
                PostId = id,
                Title = postViewModel.Title,
                Content = postViewModel.Content,
                CategoryId = postViewModel.CategoryId,
                UserId = userId, // Anonym användare eller standardvärde
                UpdatedAt = DateTime.UtcNow
            };

            var success = await _postService.UpdatePost(post, userId); // userId skickas inom post-objektet
            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }

        // Ta bort ett inlägg
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> RemovePost(int id)
        {
            // Hämta användarens ID från ClaimsPrincipal (den inloggade användaren)
            var userIdClaim = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            // Om användaren inte är inloggad, hantera som anonym eller returnera obehörigt
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized();
            }

            // Konvertera användarens ID från string till int (eller annan relevant datatyp)
            var userId = int.Parse(userIdClaim);

            var result = await _postService.RemovePost(userId, id);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }
        [Authorize]
        [HttpPost("{postId}/like")]
        public async Task<IActionResult> LikePost(int postId)
        {
            // Hämta användarens ID från ClaimsPrincipal (den inloggade användaren)
            var userIdClaim = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            // Om användaren inte är inloggad, hantera som anonym eller returnera obehörigt
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized();
            }

            // Konvertera användarens ID från string till int (eller annan relevant datatyp)
            var userId = int.Parse(userIdClaim);

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

        [Authorize]
        [HttpPost("{postId}/unlike")]
        public async Task<IActionResult> UnlikePost(int postId)
        {
            // Hämta användarens ID från ClaimsPrincipal (den inloggade användaren)
            var userIdClaim = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized();
            }

            var userId = int.Parse(userIdClaim);
            var result = await _postService.UnlikePostAsync(postId, userId);

            if (result == "Posten kunde inte hittas.")
            {
                return NotFound(new { message = result });
            }

            if (result == "Du har inte gillat denna post.")
            {
                return BadRequest(new { message = result });
            }

            return Ok(new { message = result });
        }

        [Authorize]
        [HttpGet("{postId}/hasLiked")]
        public async Task<IActionResult> HasUserLikedPost(int postId)
        {
            try
            {
                // Hämta userId från ClaimsPrincipal (den inloggade användaren)
                var userIdClaim = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                {
                    return Unauthorized();
                }

                var userId = int.Parse(userIdClaim);

                // Kontrollera om användaren har gillat posten
                var hasLiked = await _postService.HasUserLikedPost(postId, userId);

                return Ok(new { hasLiked });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Interna serverfel: {ex.Message}");
            }
        }

        [HttpGet("{postId}/can-edit")]
        public async Task<IActionResult> CanEditPost(int postId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var canEdit = await _postService.CanEditPost(userId, postId);

            return Ok(new { canEdit });
        }

    }
}