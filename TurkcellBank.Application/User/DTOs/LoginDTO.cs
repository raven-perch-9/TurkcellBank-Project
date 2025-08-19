namespace TurkcellBank.Application.User.DTOs
{
    public class LoginDTO
    {
        // Identifier refers to either the username or the email adress.
        public string Identifier { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
