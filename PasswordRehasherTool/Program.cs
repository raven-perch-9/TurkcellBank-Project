using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TurkcellBank.Infrastructure.Data;
using TurkcellBank.Application.User.Services;
using TurkcellBank.Domain; // Assuming User entity is here

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

        // Seed 100 normal users
        for (int i = 1; i <= 100; i++)
        {
            string username = $"user{i}";
            string password = $"pass{i}";
            if (!context.Users.Any(u => u.Username == username))
            {
                context.Users.Add(new User
                {
                    Username = username,
                    PasswordHash = passwordService.HashPassword(password),
                    Email = $"{username}@TurkcellBank.com",
                    FullName = $"User Name{i}",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                });
                Console.WriteLine($"Added: {username} / {password}");
            }
        }

        // Seed admin user
        if (!context.Users.Any(u => u.Username == "admin"))
        {
            context.Users.Add(new User
            {
                Username = "admin",
                PasswordHash = passwordService.HashPassword("adminpass"),
                Email = "admin@TurkcellBank.com",
                FullName = "Administrator",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            });
            Console.WriteLine("Added: admin / adminpass");
        }

        context.SaveChanges();
        Console.WriteLine("All test users have been inserted.");
    }
}