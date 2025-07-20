using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TurkcellBank.Application.DTOs;
using TurkcellBank.Domain;
using TurkcellBank.Infrastructure.Data;
using TurkcellBank.Infrastructure.Services;

namespace TurkcellBank.Web_API.Controllers
{
    [ApiController] 
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IJwtService _jwtService;

        public UserController(AppDbContext context, IJwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO dto)
        {
            // 1. Null/empty check
            if (string.IsNullOrWhiteSpace(dto.Username) ||
                string.IsNullOrWhiteSpace(dto.Email) ||
                string.IsNullOrWhiteSpace(dto.PasswordHash))
            {
                return BadRequest("Username, Email, and Password are required.");
            }
            // 2. Check if email already exists
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (existingUser != null)
            {
                return Conflict("A user with this email already exists.");
            }

            // 3. Hash the password later.

            // 4. Create user entity
            var user = new User()
            {
                Username = dto.Username,
                FullName = dto.FullName,
                Email = dto.Email,
                PasswordHash = dto.PasswordHash
            };

            // 5. Save to database
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // 6. Return 201 Created
            return CreatedAtAction(nameof(Register), new { id = user.ID }, "User registered successfully.");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => 
                u.Email == dto.Identifier || u.Username == dto.Identifier);

            if (user == null)
                return Unauthorized("Invalid email/username or password.");

            bool isPasswordValid = dto.PasswordHash == user.PasswordHash;
            if (!isPasswordValid)
                return Unauthorized("Invalid email or password.");

            // 1. Create JWT token here (use a JwtService or similar)
            var token = _jwtService.GenerateToken(user);

            return Ok(new { token });
        }
        [Authorize]
        [HttpGet("Get User Data")]
        public async Task<IActionResult> GetProfile()
        {
            var userIDClaim = User.FindFirst("user_id")?.Value;
            if (string.IsNullOrEmpty(userIDClaim))
                return Unauthorized("Token does not contain user ID");
            var user = await _context.Users.FindAsync(int.Parse(userIDClaim));
            if (user == null)
                return NotFound("User not found.");
            return Ok(new
            {
                user.Username,
                user.FullName,
                user.Email,
                user.PasswordHash,
                user.ID,
                user.CreatedAt,
                user.IsActive
            });
        }
        [Authorize]
        [HttpPut("update")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateDTO dto)
        {
            var userIdClaim = User.FindFirst("user_id")?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized("Invalid token: missing user_id");

            var user = await _context.Users.FindAsync(int.Parse(userIdClaim));
            if (user == null)
                return NotFound("User not found");

            // Update fields
            if (!string.IsNullOrWhiteSpace(dto.FullName))
                user.FullName = dto.FullName;

            if (!string.IsNullOrWhiteSpace(dto.Email))
                user.Email = dto.Email;

            if (!string.IsNullOrWhiteSpace(dto.Username))
                user.Username = dto.Username;

            if (!string.IsNullOrWhiteSpace(dto.PasswordHash))
            {
                // Reminder: this is raw password, hash it properly before production
                user.PasswordHash = dto.PasswordHash;
            }

            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            
            return Ok(new { message = "Profile updated successfully." });
        }
    }
}