//Used for updating user information on the system.

namespace TurkcellBank.Application.DTOs;

public class UserProfileDTO
{
    public string ID { get; set; } = null!;
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
}