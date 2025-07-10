//Used for updating user information on the system.

namespace TurkcellBank.Application.DTOs
{
    public class UpdateDTO
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string Username { get; set; }
        public string IsActive { get; set; }
    }
}