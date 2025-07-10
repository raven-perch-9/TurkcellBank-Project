using TurkcellBank.Application.DTOs;

namespace TurkcellBank.Application
{
    public interface IAuthService
    {
        Task<string> Register(RegisterDTO dto);
        Task<string> Login(LoginDTO dto);
        Task<string> UpdateProfile(UpdateDTO dto);
        Task<string> UserProfile(UserProfileDTO dto);
    }
}
