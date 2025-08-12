using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using TurkcellBank.Domain;                 // Account, User
using TurkcellBank.Infrastructure.Data;
using TurkcellBank.Infrastructure.Services; // GenerateIBAN

namespace AccountSeederTool
{
    internal class Program
    {
        private static readonly string TRY = "TRY";
        private static readonly string USD = "USD";
        private static readonly string EUR = "EUR";

        private const string TYPE_VADESIZ = "Vadesiz Mevduat";
        private const string TYPE_VADELİ = "Vadeli Mevduat";

        static void Main()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(config.GetConnectionString("DefaultConnection"))
                .Options;

            using var db = new AppDbContext(options);
            var ibanGen = new GenerateIBAN();
            var rng = new Random();

            // Get users ordered by ID so “first 50”, “next 30” etc. are deterministic
            var users = db.Users.AsNoTracking().OrderBy(u => u.ID).ToList();
            Console.WriteLine($"Users found: {users.Count}");

            // Helper local function
            decimal RandBalance() => Math.Round((decimal)rng.NextDouble() * 50000m, 2);

            void EnsureAccount(int userId, string accType, string currency)
            {
                // Idempotent: skip if the same type+currency already exists for user
                var exists = db.Accounts.Any(a => a.UserID == userId && a.AccountType == accType && a.CurrencyCode == currency);
                if (exists) return;

                var acc = new Account
                {
                    UserID = userId,
                    IBAN = "", // set after SaveChanges to get ID
                    AccountType = accType,
                    CurrencyCode = currency,
                    Balance = RandBalance(),
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };
                db.Accounts.Add(acc);
                db.SaveChanges(); // get acc.ID

                acc.IBAN = ibanGen.Generate(userId, acc.ID);
                db.Update(acc);
                db.SaveChanges();

                Console.WriteLine($"User#{userId}: + {accType} {currency} (ID {acc.ID}) IBAN={acc.IBAN} Balance={acc.Balance}");
            }

            // Ranges (1‑based wording → we use 0-based index over ordered list)
            // 1) First 50 users: 1 Vadesiz TRY
            foreach (var u in users.Take(50))
                EnsureAccount(u.ID, TYPE_VADESIZ, TRY);

            // 2) 51st–80th users: 1 Vadeli TRY + 1 Vadesiz TRY
            foreach (var u in users.Skip(50).Take(30))
            {
                EnsureAccount(u.ID, TYPE_VADELİ, TRY);
                EnsureAccount(u.ID, TYPE_VADESIZ, TRY);
            }

            // 3) Next 10 users (81st–90th): USD Vadesiz + TRY Vadesiz
            foreach (var u in users.Skip(80).Take(10))
            {
                EnsureAccount(u.ID, TYPE_VADESIZ, USD);
                EnsureAccount(u.ID, TYPE_VADESIZ, TRY);
            }

            // 4) Last 10 users (91st–100th): USD/TRY/EUR all Vadesiz + 1 Vadeli TRY
            foreach (var u in users.Skip(90).Take(10))
            {
                EnsureAccount(u.ID, TYPE_VADESIZ, USD);
                EnsureAccount(u.ID, TYPE_VADESIZ, TRY);
                EnsureAccount(u.ID, TYPE_VADESIZ, EUR);
                EnsureAccount(u.ID, TYPE_VADELİ, TRY);
            }

            Console.WriteLine(" Account seeding complete.");
        }
    }
}