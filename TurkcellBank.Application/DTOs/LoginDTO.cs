//User is logged into the system here.

namespace TurkcellBank.Application.DTOs
{
    public class LoginDTO
    {
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
    }
}
