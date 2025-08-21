using TurkcellBank.Application.Common.Abstractions;
using TurkcellBank.Domain;
using TurkcellBank.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TurkcellBank.Infrastructure.Data.Repositories
{
    public sealed class AccountRepository : IAccountRepository
    {
        private readonly AppDbContext _db;
        public AccountRepository(AppDbContext db) => _db = db;

        public Task<Account?> GetByIdAsync(int id, CancellationToken ct = default) =>
            _db.Accounts
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.ID == id, ct);

        public async Task<IReadOnlyList<Account>> GetByUserIdAsync(int userId, CancellationToken ct = default) =>
            await _db.Accounts
                .AsNoTracking()
                .Where(a => a.UserID == userId)
                .ToListAsync(ct);

        public async Task<Account?> GetMainTryAccountAsync(int userId, CancellationToken ct = default) =>
            await _db.Accounts
            .AsNoTracking()
            .Where(a => a.UserID == userId
                     && a.AccountType == "Vadesiz"
                     && a.IsActive)
            .FirstOrDefaultAsync(ct);

        public async Task AddAsync(Account account, CancellationToken ct = default)
        {
            await _db.Accounts.AddAsync(account, ct);
            await _db.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(Account account, CancellationToken ct = default)
        {
            _db.Accounts.Update(account);
            await _db.SaveChangesAsync(ct);
        }
        public Task<int> SaveChangesAsync(CancellationToken ct = default) =>
            _db.SaveChangesAsync(ct);
    }
}
