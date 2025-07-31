//User is logged into the system here.

namespace TurkcellBank.Application.DTOs
{
    public class LoginDTO
    {
        // Identifier refers to either the username or the email adress.
        public string Identifier { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
