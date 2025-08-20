using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using System.Security.Claims;
using TurkcellBank.Application.Common.Abstractions;
using TurkcellBank.Application.Common.DTOs;
using TurkcellBank.Application.User.DTOs;
using TurkcellBank.Application.User.Services.Interfaces;
using TurkcellBank.Domain;
using TurkcellBank.Infrastructure.Data;
using TurkcellBank.Infrastructure.Services;

namespace TurkcellBank.Web_API.Controllers
{
    [ApiController] 
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        //Depends on abstractions for context
        private readonly IUserRepository _users;
        //I inject Athentication Service
        private readonly IAuthService _authService;
        //Injecting IBAN Generation Service
        private readonly IGenerateIBAN _ibanGenerator;
        //Injecting Transaction Service
        private readonly ITransactionService _transactionService;
        public UserController(IUserRepository userRepository, IAuthService authService, IGenerateIBAN ibanGenerator,
                                                    ITransactionService transactionService)
        {
            _users = userRepository;
            _authService = authService;
            _ibanGenerator = ibanGenerator;
            _transactionService = transactionService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Username) ||
                string.IsNullOrWhiteSpace(dto.Email) ||
                string.IsNullOrWhiteSpace(dto.Password))
            {
                return BadRequest("Username, Email, and Password are required.");
            }
            if (await _users.EmailExistsAsync(dto.Email))
                return Conflict("A user with this email already exists.");
            if (await _users.UsernameExistsAsync(dto.Username))
                return Conflict("A user with this username already exists.");

            var token = await _authService.RegisterAsync(dto);
            return Ok(new { message = "User registered successfully.", token });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.Identifier) || string.IsNullOrWhiteSpace(dto.Password))
                {
                    return BadRequest(new { success = false, message = "Email/Username and Password are required." });
                }
                var token = await _authService.LoginAsync(dto);
                if (token is null)
                    return Unauthorized(new { success = false, message = "Invalid email/username or password." });

                return Ok(new { success = true, token });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Login Error] {ex}");
                return StatusCode(500, new { success = false, message = "An internal error occurred. Please try again later." });
            }
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetProfile()
        {
            var userIdClaim = User.FindFirst("user_id")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                return Unauthorized("Invalid or missing user_id in token.");

            var user = await _users.GetByIdAsync(userId);
            if (user == null)
                return NotFound("User not found.");

            var accounts = user.Accounts ?? new List<Account>();

            var dto = new UserProfileDTO
            {
                ID = user.ID,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                CreatedAt = user.CreatedAt,
                AccountIDs = user.Accounts.Select(a => a.ID).ToList(),
                AccountTypes = user.Accounts.Select(a => a.AccountType).ToList(),
                CurrencyCode = user.Accounts.Select(a => a.CurrencyCode).ToList(),
                IBANs = user.Accounts.Select(a => a.IBAN).ToList(),
                Balances = user.Accounts.Select(a => a.Balance).ToList(),
                AccountCreatedDates = user.Accounts.Select(a => a.CreatedAt).ToList(),
                IsActive = user.Accounts.Select(a => a.IsActive).ToList()
            };
            return Ok(dto);
        }

        [Authorize]
        [HttpPut("update")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateDTO dto)
        {
            var userIdClaim = User.FindFirst("user_id")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                return Unauthorized("Invalid token: missing user_id");

            var user = await _users.GetByIdAsync(userId);
            if (user == null)
                return NotFound("User not found");

            if (!string.IsNullOrWhiteSpace(dto.FullName))
                user.FullName = dto.FullName;
            if (!string.IsNullOrWhiteSpace(dto.Email))
                user.Email = dto.Email;
            if (!string.IsNullOrWhiteSpace(dto.Username))
                user.Username = dto.Username;
            if (!string.IsNullOrWhiteSpace(dto.Password))
                user.PasswordHash = _authService.HashPassword(dto.Password);

            _users.Update(user);
            await _users.SaveChangesAsync();

            return Ok(new { message = "Profile updated successfully." });
        }

        [Authorize]
        [HttpDelete("delete")]
        public async Task<IActionResult> SoftDeleteUser()
        {
            var userIdClaim = User.FindFirst("user_id")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                return Unauthorized("Invalid token: missing user_id");
            var user = await _users.GetByIdAsync(userId);
            if (user == null)
                return NotFound("User not found.");

            if (!user.IsActive)
                return BadRequest("User is already deactivated.");

            user.IsActive = false;
            _users.Update(user);
            await _users.SaveChangesAsync();

            return Ok(new { message = "User has been deleted" });
        }

        [Authorize]
        [HttpPost("close/{AccountID}")]
        public async Task<IActionResult> CloseAccount(int accountId)
        {
            var userIdClaim = User.FindFirst("user_id")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                return Unauthorized("Invalid token: missing user_id");

            var user = await _users.GetByIdAsync(userId);
            if (user == null)
                return NotFound("User not found");

            var account = user.Accounts.FirstOrDefault(a => a.ID == accountId);
            if (account == null)
                return NotFound("Account not found.");
            if (!account.IsActive)
                return BadRequest("Account is already closed.");
            if (account.Balance > 0)
                return BadRequest("Account must be empty before closing.");

            account.IsActive = false;
            await _users.SaveChangesAsync();

            return Ok(new { message = "Account closed successfully" });
        }

        [Authorize]
        [HttpPost("open")]
        public async Task<IActionResult> OpenAccount([FromBody] OpenAccountDTO dto)
        {
            try
            {
                if (dto is null)
                    return BadRequest(new { success = false, message = "Geçersiz istek." });

                //User Verification
                var userIdClaim = User.FindFirst("user_id")?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                    return Unauthorized("Invalid token: missing user_id");
                var user = await _users.GetByIdAsync(userId);
                if (user == null)
                    return NotFound("User not found");

                //Account Data Validation
                var accountType = (dto.AccountType ?? string.Empty).Trim();
                var currency = (dto.CurrencyCode ?? string.Empty).Trim().ToUpperInvariant();
                var initial = dto.InitialDeposit;

                if (string.IsNullOrWhiteSpace(accountType))
                    return BadRequest("Hesap türü zorunlu.");
                if (string.IsNullOrWhiteSpace(currency))
                    return BadRequest("Para birimi zorunlu.");

                var account = new Account
                {
                    UserID = userId,
                    AccountType = accountType,
                    CurrencyCode = currency,
                    Balance = initial,
                    IBAN = string.Empty,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                user.Accounts.Add(account);
                await _users.SaveChangesAsync();

                account.IBAN = _ibanGenerator.Generate(userId, account.ID);
                await _users.SaveChangesAsync();

                var result = new
                {
                    account.ID,
                    account.AccountType,
                    account.CurrencyCode,
                    account.IBAN,
                    account.Balance,
                    account.IsActive,
                    account.CreatedAt
                };
                return Ok(new { success = true, data = result });
            }
            catch (Exception)
            {
                return StatusCode(500, new { success = false, message = "An internal error occurred. Please try again later." });
            }
        }

        [Authorize]
        [HttpPost("transfer")]
        public async Task<IActionResult> Transfer([FromBody] TransferRequestDTO request, CancellationToken ct)
        {
            if (request == null)
                return BadRequest("Transfer request cannot be null.");

            var userIdClaim = User.FindFirst("user_id")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                return Unauthorized(new { success = false, message = "Invalid token: missing or invalid user_id." });

            int userID = int.Parse(userIdClaim);
            try
            {
                var result = await _transactionService.TransferAsync(userId, request, ct);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}