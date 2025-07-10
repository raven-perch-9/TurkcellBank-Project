//Data Transfer Object Class is the API defines the data type to be sent to the server by the client.
//User is Registered by this class.

namespace TurkcellBank.Application.DTOs
{
    public class RegisterDTO
    {
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string FullName { get; set; } = null!;
    }
}
