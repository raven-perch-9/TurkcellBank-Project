using TurkcellBank.Application.User.DTOs;

namespace TurkcellBank.Application.User.Services.Interfaces
{
    public interface IAuthService
    {
        Task<string> RegisterAsync(RegisterDTO dto);
        Task<string?> LoginAsync(LoginDTO dto);
        string HashPassword(string password);
    }
}
