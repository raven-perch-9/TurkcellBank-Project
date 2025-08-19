using TurkcellBank.Application.Common.Abstractions;
using TurkcellBank.Application.User.DTOs;
using TurkcellBank.Application.User.Services.Interfaces;
using TurkcellBank.Domain;


namespace TurkcellBank.Application.User.Services
{
    public sealed class AuthService : IAuthService
    {
        private readonly IPasswordService _pwd;
        private readonly IJwtService _jwt;
        private readonly IUserRepository _users;
        public AuthService(IPasswordService pwd, IJwtService jwt, IUserRepository users)
        {
            _pwd = pwd;
            _jwt = jwt;
            _users = users;
        }

        public async Task<string?> LoginAsync(LoginDTO dto)
        {
            try
            {
                var user = await _users.GetByEmailOrUsernameAsync(dto.Identifier);
                if (user is null)
                    throw new ArgumentException("Invalid email/username or password.");
                var valid = _pwd.VerifyPassword(dto.Password, user.PasswordHash);
                if (!valid)
                    throw new ArgumentException("Invalid email/username or password.");

                return _jwt.GenerateToken(user);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred during login.", ex);
            }
        }
        public async Task<string> RegisterAsync(RegisterDTO dto)
        {
            try
            {
                if (await _users.EmailExistsAsync(dto.Email))
                    throw new ArgumentException("Email already exists.", nameof(dto.Email));
                if (string.IsNullOrWhiteSpace(dto.Email))
                    throw new ArgumentException("Email cannot be empty.", nameof(dto.Email));
                if (string.IsNullOrWhiteSpace(dto.Password))
                    throw new ArgumentException("Password cannot be empty.", nameof(dto.Password));

                var user = new TurkcellBank.Domain.User
                {
                    FullName = dto.FullName?.Trim() ?? string.Empty,
                    Email = dto.Email.Trim().ToLowerInvariant() ?? string.Empty,
                    Username = dto.Username?.Trim() ?? string.Empty,
                    PasswordHash = _pwd.HashPassword(dto.Password) ?? string.Empty,
                    CreatedAt = DateTime.UtcNow
                };

                await _users.AddAsync(user);
                await _users.SaveChangesAsync();

                var token = _jwt.GenerateToken(user);
                return token;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred during registration.", ex);
            }
        }

        public string HashPassword(string password)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(password))
                    throw new ArgumentException("Password cannot be empty.", nameof(password));
                return _pwd.HashPassword(password);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Internal Error", ex);
            }
        }
    }
}