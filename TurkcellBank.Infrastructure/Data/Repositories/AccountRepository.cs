using TurkcellBank.Application.Common.Abstractions;
using TurkcellBank.Domain;
using TurkcellBank.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace TurkcellBank.Infrastructure.Data.Repositories
{
    public sealed class AccountRepository : IAccountRepository
    {
        private readonly AppDbContext _db;
        public AccountRepository(AppDbContext db) => _db = db;
        //// TO BE CTD
        /////// TO BE CTD
    }
}
