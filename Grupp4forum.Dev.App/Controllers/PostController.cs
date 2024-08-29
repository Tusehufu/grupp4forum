using Grupp4forum.Dev.Infrastructure.Models;
using Grupp4forum.Dev.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Grupp4forum.Dev.Infrastructure.ViewModel;
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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Post>>> GetAllPosts()
        {
            var posts = await _postService.GetAllPosts();
            return Ok(posts);
        }

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
        public async Task<ActionResult> AddPost(PostViewModel postViewModel)
        {
            // Kontrollera om modellen är giltig
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Hämta den inloggade användarens ID från HttpContext
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized("Du måste vara inloggad för att skapa ett inlägg.");
            }

            // Convert the user ID to an integer or handle conversion errors
            int userId;
            if (!int.TryParse(userIdClaim.Value, out userId))
            {
                Console.WriteLine("Ogiltigt id");
                return BadRequest("Ogiltigt användar-ID.");
            }
            Console.WriteLine(userId);

            // Skapa ett nytt Post-objekt från viewmodel
            var post = new Post
            {
                Title = postViewModel.Title,
                Content = postViewModel.Content,
                UserId = userId // Sätt användarens id
            };

            // Lägg till inlägget
            var id = await _postService.AddPost(post, userId);
            var author = post.Author;
            // Skapa ett svarsobjekt med inläggets ID och författare
            var response = new
            {
                PostId = id,
                Author = author
            };

            // Returnera svar med skapad status
            return Ok(response);
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> RemovePost(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized("Du måste vara inloggad för att ta bort evenemanget.");
            }

            // Convert the user ID to an integer or handle conversion errors
            int userId;
            if (!int.TryParse(userIdClaim.Value, out userId))
            {
                Console.WriteLine("Ogiltigt id");
                return BadRequest("Ogiltigt användar-ID.");
            }
            var result = await _postService.RemovePost(userId, id);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}
