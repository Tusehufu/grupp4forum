using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Isopoh.Cryptography.Argon2;
using Grupp4forum.Dev.Infrastructure.Models;
using Grupp4forum.Dev.Infrastructure.Repository;

namespace Grupp4forum.Dev.API.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly PasswordService _passwordService;

        public AuthController(UserRepository userRepository, IConfiguration configuration, PasswordService passwordService)
        {
            _userRepository = userRepository;
            _configuration = configuration;
            _passwordService = passwordService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // Hämta användare från databasen baserat på det angivna användarnamnet
            var user = await _userRepository.FindByUsername(request.Username);

            // Om användaren inte hittas eller lösenordet inte stämmer, returnera 401 Unauthorized
            if (user == null || !_passwordService.VerifyPassword(request.Password, user.PasswordHash))
            {
                return Unauthorized();
            }

            // Skapa en JWT-token om användaren är giltig
            var token = GenerateJwtToken(user);


            // Returnera tokenen i svaret
            return Ok(new { Token = token });
        }

        private string GenerateJwtToken(User user)
        {
            // Skapa claims för användarens ID, användarnamn och roll
            var claims = new[]
            {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()), // Lagrar användar-ID
        new Claim("userId", user.Id.ToString()), // Lägger till userId som en specifik claim
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // Unik token-identifierare
        new Claim(ClaimTypes.Name, user.Username), // Lagrar användarnamnet
        new Claim(ClaimTypes.Role, user.Role.Name) // Lagrar användarens roll
    };

            // Hämta signeringsnyckeln från konfigurationen
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Skapa JWT-token med claims, utgångstid och signatur
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(30), // Utgångstid
                signingCredentials: creds
            );

            // Returnera tokenen som sträng
            return new JwtSecurityTokenHandler().WriteToken(token);
        }




        // Klassen för inloggningsbegäran
        public class LoginRequest
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }
    }
}