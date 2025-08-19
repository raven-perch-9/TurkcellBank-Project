namespace TurkcellBank.Application.User.Services.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(Domain.User user);
    }
}