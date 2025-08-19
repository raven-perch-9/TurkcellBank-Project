using BCrypt.Net;
using TurkcellBank.Application.User.Services.Interfaces;

namespace TurkcellBank.Application.User.Services
{
    public class PasswordService : IPasswordService
    {
     public string HashPassword(string password)
           => BCrypt.Net.BCrypt.HashPassword(password);

      public bool VerifyPassword(string password, string hashedPassword)
        => BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }
}