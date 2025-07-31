//Used for retrieveing user information on the database by an API.
//No password is exchanged due to security measures.

namespace TurkcellBank.Application.DTOs;

public class UserProfileDTO
{
    public string ID { get; set; } = null!;
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public DateTime CreatedAt { get; set; } 

    // Account Data goes below
    public List<string> AccountIDs { get; set; } = null!;
    public List<string> AccountTypes { get; set; } = null!;
    public List<string> IBANs { get; set; } = null!;
    public List<decimal> Balances  { get; set; } = null!;
    public List<DateTime> AccountCreatedDates { get; set; } = null!;
}