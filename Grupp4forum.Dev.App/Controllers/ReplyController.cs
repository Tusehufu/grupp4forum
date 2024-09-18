using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Grupp4forum.Dev.Infrastructure.Models;
using Grupp4forum.Dev.Infrastructure.Repository;
using Grupp4forum.Dev.Infrastructure.ViewModel;
using System.Security.Claims;
using Grupp4forum.Dev.Infrastructure.Services;
using Microsoft.Extensions.Hosting;


[Route("api/[controller]")]
[ApiController]
public class RepliesController : ControllerBase
{
    private readonly ReplyService _replyService;

    public RepliesController(ReplyService replyService)
    {
        _replyService = replyService;


    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Reply>>> GetAllReplies()
    {
        var replies = await _replyService.GetAllReplies();
        return Ok(replies);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Reply>> GetReplyById(int id)
    {
        var reply = await _replyService.GetReplyById(id);
        if (reply == null)
        {
            return NotFound();
        }
        return reply;
    }

    [HttpPost]
    public async Task<ActionResult<Reply>> CreateReply(int postId, int? parentReplyId, [FromForm] ReplyViewModel replyViewModel, IFormFile? image)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = 1; // Här hårdkodas användar-ID

        // Skapa ett nytt Reply-objekt
        var reply = new Reply
        {
            Content = replyViewModel.Content,
            UserId = userId,
            PostId = postId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsVisible = true,
            //ParentReplyId = parentReplyId // Kan vara null om det är ett svar på en post
        };

        // Hantera bilduppladdningen om det finns en bild
        if (image != null)
        {
            using (var memoryStream = new MemoryStream())
            {
                await image.CopyToAsync(memoryStream);
                reply.Image = memoryStream.ToArray(); // Lagra bilden som byte-array
            }
        }

        // Anropa ReplyService för att spara reply i databasen och få det genererade ReplyId
        var replyId = await _replyService.CreateReply(reply, userId, postId);

        // Uppdatera reply med det genererade ReplyId
        reply.ReplyId = replyId;

        // Returnera det skapade reply-objektet med det genererade ID:t
        return Ok(reply);
    }


    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateReply(int id, ReplyViewModel replyViewModel)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Användarverifiering eller sätt standardvärde för UserId (om det behövs)
        var userId = 1; // Detta är ett exempelvärde för en användare

        // Skapa en Reply-entitet baserat på den mottagna ViewModel
        var reply = new Reply
        {
            ReplyId = id,
            Content = replyViewModel.Content,
            UserId = userId, // Kan vara en faktisk användare eller ett standardvärde
            UpdatedAt = DateTime.UtcNow
        };

        // Skicka Reply till ditt service-lager/repository för uppdatering
        var success = await _replyService.UpdateReply(reply);
        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }


    [HttpDelete("{id}")]
    public async Task<IActionResult> RemoveReply(int id)
    {
        //var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        //if (userIdClaim == null)
        //{
        //    return Unauthorized("Du måste vara inloggad för att ta bort svaret.");
        //}

        //// Convert the user ID to an integer or handle conversion errors
        //int userId;
        //if (!int.TryParse(userIdClaim.Value, out userId))
        //{
        //    Console.WriteLine("Ogiltigt id");
        //    return BadRequest("Ogiltigt användar-ID.");
        //}
        var userId = 1;
        var result = await _replyService.RemoveReply(userId, id);
        if (!result)
        {
            return NotFound();
        }
        return Ok("Tja");
    }
    [HttpGet("post/{postId}")]
    public async Task<ActionResult<IEnumerable<Reply>>> GetRepliesForPost(int postId)
    {
        var replies = await _replyService.GetRepliesByPostId(postId);

        if (replies == null || !replies.Any())
        {
            return NotFound($"Inga replies hittades för postId: {postId}");
        }

        return Ok(replies);
    }

    [HttpPost("{replyId}/like")]
    public async Task<IActionResult> LikeReply(int replyId)
    {
        // Här antas att användar-ID hämtas från autentiseringssystem (t.ex. JWT)
        var userId = 1;

        var result = await _replyService.LikeReplyAsync(replyId, userId);

        if (result == "Reply kunde inte hittas.")
        {
            return NotFound(new { message = result });
        }

        if (result == "Du har redan gillat denna reply.")
        {
            return BadRequest(new { message = result });
        }

        return Ok(new { message = result });
    }

}

