using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TurkcellBank.Application.Common.Abstractions;
using TurkcellBank.Application.Common.DTOs;
using TurkcellBank.Application.Common.Services.Interfaces;
using TurkcellBank.Application.User.DTOs;
using TurkcellBank.Application.User.Services.Interfaces;
using TurkcellBank.Domain.Entities;
using TurkcellBank.Domain.Enums;
using TurkcellBank.Infrastructure.Services.Interfaces;

namespace TurkcellBank.Web_API.Controllers
{
    [ApiController] 
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _users;
        private readonly IAccountRepository _accounts;
        private readonly ITransactionRepository _transactions;
        private readonly ICreditRepository _credits;
        private readonly IDisbursementService _disburse;
        private readonly IPaymentRepository _payments;

        private readonly IAuthService _authService;
        private readonly IGenerateIBAN _ibanGenerator;
        private readonly ITransactionService _transactionService;
        private readonly IPaymentService _paymentService;
        private readonly IFXFetcherService _fxFetcherService;

        public UserController(IUserRepository users, 
                              IAccountRepository accounts,
                              ITransactionRepository transactions,
                              ICreditRepository credits,
                              IAuthService authService,
                              IGenerateIBAN ibanGenerator,
                              ITransactionService transactionService,
                              IDisbursementService disburse,
                              IPaymentRepository payments,
                              IPaymentService paymentService,
                              IFXFetcherService fxFetcherService)
        {
            _users = users;
            _accounts = accounts;
            _credits = credits;
            _transactions = transactions;
            _authService = authService;
            _ibanGenerator = ibanGenerator;
            _transactionService = transactionService;
            _disburse = disburse;
            _payments = payments;
            _paymentService = paymentService;
            _fxFetcherService = fxFetcherService;
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
                return NotFound("Kullanıcı bulunamadı.");

            var account = user.Accounts.FirstOrDefault(a => a.ID == accountId);
            if (account == null)
                return NotFound("Hesap bulunamadı.");
            if (!account.IsActive)
                return BadRequest("Hesap zaten kapalı.");
            if (account.Balance > 0)
                return BadRequest("Hesabı kapatmak için bakiyenizin 0 olması gerekmektedir.");

            account.IsActive = false;
            await _users.SaveChangesAsync();

            return Ok(new { message = "Hesap kapatıldı" });
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
                    return NotFound("Kullanıcı bulunamadı");

                if (!Enum.IsDefined(typeof(AccountType), dto.AccountType))
                    return BadRequest(new { success = false, message = "Geçersiz hesap türü." });

                if (!Enum.IsDefined(typeof(CurrencyCode), dto.CurrencyCode))
                    return BadRequest(new { success = false, message = "Geçersiz para birimi." });

                if (dto.InitialDeposit < 0)
                    return BadRequest(new { success = false, message = "Başlangıç bakiyesi negatif olamaz." });

                var account = new Account
                {
                    UserID = userId,
                    AccountType = dto.AccountType,
                    CurrencyCode = dto.CurrencyCode,
                    Balance = dto.InitialDeposit,
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
                    AccountType = account.AccountType.ToString(),
                    CurrencyCode = account.CurrencyCode.ToString(),
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

        [Authorize]
        [HttpPost("credit-apply")]
        public async Task<IActionResult> ApplyForCredit([FromBody] CreditApplyDTO dto, CancellationToken ct = default)
        {
            if (dto == null)
                return BadRequest("Zorunlu tüm alanları doldurunuz.");
            if (dto.RequestedAmount <= 0)
                return BadRequest("Sıfırdan daha büyük bir kredi talebi giriniz.");
            if (dto.TermMonths <= 0)
                return BadRequest("Geri ödeme süresi sıfırdan büyük olmalıdır.");

            var userIdClaim = User.FindFirst("user_id")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                return Unauthorized(new { success = false, message = "Invalid token: missing or invalid user_id." });

            try
            {
                var apply = new CreditApplication
                {
                    UserID = userId,
                    RequestedAmount = dto.RequestedAmount,
                    AcceptedAmount = 0m, // admin will decide later
                    MonthlyIncome = dto.MonthlyIncome,
                    Occupation = dto.Occupation,
                    TermMonths = dto.TermMonths,
                    AnnualRate = dto.AnnualRate,
                    Status = CreditStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                };
                
                await _credits.AddApplicationAsync(apply);

                return Ok(new 
                { 
                    success = true, 
                    message = "Credit Application Submitted. Pending admin review.",
                    data = new
                    {
                        id = apply.ID,
                        requestedAmount = apply.RequestedAmount,
                        term = apply.TermMonths,
                        status = apply.Status.ToString()
                    }
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("credit-approve/{id:int}")]
        public async Task<IActionResult> ApproveCredit(int id, CreditApprovalDto dto, CancellationToken ct)
        {
            var userIdClaim = User.FindFirst("user_id")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                return Unauthorized(new { success = false, message = "Invalid token: missing or invalid user_id." });

            var app = await _credits.GetApplicationByIdAsync(id);
            if (app == null)
                return NotFound(new { success = false, message = "Credit application not found." });
            if (app.Status != CreditStatus.Pending)
                return BadRequest(new { success = false, message = "Only pending applications can be approved." });

            app.Status = CreditStatus.Approved;
            app.AcceptedAmount = dto.AcceptedAmount;
            app.TermMonths = dto.TermMonths;
            app.AnnualRate = dto.AnnualRate;
            app.DecidedAt = dto.DecidedAt ?? DateTime.UtcNow;
            app.DecisionBy = dto.DecisionBy ?? User.Identity?.Name;
            app.DecisionNote = dto.DecisionNote;

            await _credits.UpdateApplicationAsync(app);
            await _credits.SaveChangesAsync(ct);

            return Ok(new
            {
                success = true,
                message = "Credit application approved.",
                data = new
                {
                    app.ID,
                    app.AcceptedAmount,
                    app.TermMonths,
                    app.AnnualRate,
                    app.DecidedAt,
                    app.DecisionBy,
                    app.DecisionNote,
                    app.Status
                }
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("disburse/{id:int}")]
        public async Task<IActionResult> DisburseCredit(int id, CancellationToken ct = default)
        {
            var userIdClaim = User.FindFirst("user_id")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                return Unauthorized(new { success = false, message = "Invalid token: missing or invalid user_id." });
            try
            {
                var ok = await _disburse.DisburseAsync(id, ct);
                return Ok(new { success = ok, message = "Credit application disbursed successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("credit-get")]
        public async Task<IActionResult> GetAllCreditApplications()
        {
            try
            {
                var userIdClaim = User.FindFirst("user_id")?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                    return Unauthorized(new { success = false, message = "Invalid token: missing or invalid user_id." });

                var apps = await _credits.GetAllApplicationsAsync();
                var result = apps.Select(app => new GetCredAppsDTO
                {
                    ID = app.ID,
                    UserID = app.UserID,
                    RequestedAmount = app.RequestedAmount,
                    AcceptedAmount = app.AcceptedAmount,
                    MonthlyIncome = app.MonthlyIncome,
                    Occupation = app.Occupation,
                    Principle = app.Principle,
                    TermMonths = app.TermMonths,
                    AnnualRate = app.AnnualRate,
                    Status = app.Status,
                    CreatedAt = app.CreatedAt,
                    DecidedAt = app.DecidedAt,
                    DecisionBy = app.DecisionBy,
                    DecisionNote = app.DecisionNote,
                    DisbursedAt = app.DisbursedAt
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new 
                {
                    success = false,
                    message = "Unexpected error while retrieving credit applications.",
                    detail = ex.Message
                });
            }
        }

        [Authorize]
        [HttpPost("payment-request")]
        public async Task<ActionResult<PaymentResponseDTO>> CreatePaymentRequest([FromBody] PaymentRequestDTO dto)
        {
            if (dto is null) return BadRequest("Body is required.");

            var userIdClaim = User.FindFirst("user_id")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                return Unauthorized(new { success = false, message = "Invalid token: missing or invalid user_id." });
            
            var created = await _paymentService.InitiateAsync(dto);

            return Ok(created);
        }

        [Authorize]
        [HttpPost("payment-verify")]
        public async Task<ActionResult<PaymentResponseDTO>> Verify3DS([FromBody] ThreeDSVerifyDTO dto)
        {
            if (dto is null) return BadRequest("Body is required.");

            var userIdClaim = User.FindFirst("user_id")?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized(new { success = false, message = "Invalid or missing user_id claim." });

            var verified = await _paymentService.VerifyThreeDSAsync(dto);
            if (verified == null)
                return BadRequest("3DS verification failed.");

            return Ok(verified);
        }

        [Authorize]
        [HttpPost("execute-payment/{id:int}")]
        public async Task<ActionResult<PaymentResponseDTO>> CapturePayment(int id)
        {
            var userIdClaim = User.FindFirst("user_id")?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized(new { success = false, message = "Invalid or missing user_id claim." });

            var captured = await _paymentService.CaptureAsync(id);
            return Ok(captured);
        }

        [HttpGet("fxrates")]
        [AllowAnonymous]
        public async Task<ActionResult<FXRatesDTO>> GetLatestAsync(
            [FromQuery] string @base = "TRY",
            [FromQuery] string symbols = "USD,EUR")
        {
            var list = symbols.Split(',', StringSplitOptions.RemoveEmptyEntries |
                StringSplitOptions.TrimEntries);
            var dto = await _fxFetcherService.GetLatestAsync(@base, list);
            return Ok(dto);
        }
    }
}