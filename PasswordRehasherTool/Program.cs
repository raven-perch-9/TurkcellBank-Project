using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TurkcellBank.Infrastructure.Data; 
using TurkcellBank.Infrastructure.Services; 

class Program
{
    static void Main()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(config.GetConnectionString("DefaultConnection"))
            .Options;

        using var context = new AppDbContext(options);
        var passwordService = new PasswordService();

        var users = context.Users.ToList();
        foreach (var user in users)
        {
            // Check if already hashed (BCrypt hashes start with $2)
            if (!user.PasswordHash.StartsWith("$2"))
            {
                user.PasswordHash = passwordService.HashPassword(user.PasswordHash);
                Console.WriteLine($"Rehashed password for User ID: {user.ID}");
            }
        }
        context.SaveChanges();
        Console.WriteLine("All passwords have been rehashed with BCrypt.");
    }
}

