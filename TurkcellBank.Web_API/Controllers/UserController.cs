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
        //I inject Database Context (Entity Framework)
        private readonly AppDbContext _context;
        //I inject JWT Handling Service
        private readonly IJwtService _jwtService;
        //I inject Password Hashing Service
        private readonly IPasswordService _passwordService;

        public UserController(AppDbContext context, IJwtService jwtService, IPasswordService passwordService)
        {
            _context = context;
            _jwtService = jwtService;
            _passwordService = passwordService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO dto)
        {
            // 1. Null/empty check
            if (string.IsNullOrWhiteSpace(dto.Username) ||
                string.IsNullOrWhiteSpace(dto.Email) ||
                string.IsNullOrWhiteSpace(dto.Password))
            {
                return BadRequest("Username, Email, and Password are required.");
            }
            // 2. Check if email already exists
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (existingUser != null)
            {
                return Conflict("A user with this email already exists.");
            }

            // 3. Hash the password
            var hashedPassword = _passwordService.HashPassword(dto.Password);

            // 4. Create user entity
            var user = new User()
            {
                Username = dto.Username,
                FullName = dto.FullName,
                Email = dto.Email,
                PasswordHash = hashedPassword,
                CreatedAt = DateTime.UtcNow
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

            if (!_passwordService.VerifyPassword(dto.Password, user.PasswordHash))
                return Unauthorized("Invalid Credentials.");

            // 1. Create JWT token here (use a JwtService or similar)
            var token = _jwtService.GenerateToken(user);
            return Ok(new { token });
        }
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetProfile()
        {
            var userIDClaim = User.FindFirst("user_id")?.Value;
            if (string.IsNullOrEmpty(userIDClaim))
                return Unauthorized("Token does not contain user ID");
            if (!int.TryParse(userIDClaim, out int userID))
                return Unauthorized("Invalid user ID in token.");

            var user = await _context.Users
                .Include(u => u.Accounts)
                .FirstOrDefaultAsync(u => u.ID == userID);

            if (user == null)
                return NotFound("User not found.");

            var dto = new UserProfileDTO
            {
                ID = user.ID.ToString(),
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                CreatedAt = user.CreatedAt,
                AccountIDs = user.Accounts.Select(a => a.ID.ToString()).ToList(),
                AccountTypes = user.Accounts.Select(a => a.AccountType).ToList(),
                IBANs = user.Accounts.Select(a => a.IBAN).ToList(),
                Balances = user.Accounts.Select(a => a.Balance).ToList(),
                AccountCreatedDates = user.Accounts.Select(a => a.CreatedAt).ToList()
            };
            return Ok(dto);
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

            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                // Reminder: this is raw password, hash it properly before production
                user.PasswordHash = _passwordService.HashPassword(dto.Password);
            }
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            
            return Ok(new { message = "Profile updated successfully." });
        }
        [Authorize]
        [HttpDelete("delete")]
        public async Task<IActionResult> SoftDeleteUser()
        {
            var userIdClaim = User.FindFirst("user_id")?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized("Invalid token: missing user_id");

            if (!int.TryParse(userIdClaim, out int userId))
                return BadRequest("Invalid user ID format.");

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound("User not found.");

            if (!user.IsActive)
                return BadRequest("User is already deactivated.");

            user.IsActive = false;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User has been deleted" });
        }
    }
}