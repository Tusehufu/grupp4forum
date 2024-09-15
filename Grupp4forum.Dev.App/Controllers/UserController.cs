using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Grupp4forum.Dev.API.Services;
using Grupp4forum.Dev.Infrastructure.Models;
using Grupp4forum.Dev.Infrastructure.ViewModel;
using System;

namespace Grupp4forum.Dev.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UserService _userService;

        public UsersController(UserService userService)
        {
            _userService = userService;
        }

        // GET: api/users
        [HttpGet]
        public async Task<IEnumerable<User>> GetAll()
        {
            return await _userService.GetAllUsers();
        }

        // GET: api/users/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _userService.GetUserById(id);

            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        // POST: api/users
        [HttpPost]
        public async Task<IActionResult> Add(UserViewModel userViewModel)
        {
            // Validera modellen
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Skapa ett nytt User-objekt och mappa egenskaperna från UserViewModel
            User user = new User
            {
                Username = userViewModel.Username,
                Email = userViewModel.Email,
                Password = userViewModel.Password,   // Lösenordet hanteras senare vid hashning
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                RoleId = 1  // Standardroll (kan justeras)
            };

            // Lägg till användaren i databasen
            int newUserId = await _userService.AddUser(user);

            // Returnera det nyskapade användarens ID med 201 Created
            return CreatedAtAction(nameof(GetById), new { id = newUserId }, user);
        }

        // PUT: api/users/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UserViewModel userViewModel)
        {
            // Validera modellen
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Hämta användaren från databasen
            var existingUser = await _userService.GetUserById(id);
            if (existingUser == null)
            {
                return NotFound();
            }

            // Uppdatera användarinformationen
            existingUser.Username = userViewModel.Username;
            existingUser.Email = userViewModel.Email;
            existingUser.UpdatedAt = DateTime.UtcNow;

            // Om ett nytt lösenord skickades med, hasha och uppdatera det
            if (!string.IsNullOrEmpty(userViewModel.Password))
            {
                existingUser.Password = userViewModel.Password;  // Lösenordet hanteras vid hashning
            }

            // Uppdatera användaren i databasen
            bool success = await _userService.UpdateUser(existingUser);
            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }

        // DELETE: api/users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            bool success = await _userService.DeleteUser(id);
            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }

        // Åtgärd för att lägga till en roll till en användare
        [HttpPost("{userId}/roles/{roleId}")]
        public async Task<IActionResult> AddRoleToUser(int userId, int roleId)
        {
            var success = await _userService.AddRoleToUser(userId, roleId);

            if (!success)
            {
                return NotFound("Användaren eller rollen hittades inte.");
            }

            return NoContent();
        }

        // Hämta användar-ID baserat på användarnamn
        [HttpGet("username/{username}")]
        public async Task<IActionResult> GetUserIdByUsername(string username)
        {
            var user = await _userService.FindUserByUsername(username);

            if (user == null)
            {
                return NotFound("Användaren hittades inte.");
            }

            return Ok(user.Id);
        }
    }
}
