using Microsoft.EntityFrameworkCore;
using TurkcellBank.Application.Common.Abstractions;
using TurkcellBank.Domain.Entities;

namespace TurkcellBank.Infrastructure.Data.Repositories
{
    public sealed class ExternalAccountRepository : IExternalAccountRepository
    {
        private readonly AppDbContext _db;
        public ExternalAccountRepository(AppDbContext db) => _db = db;

        public Task<ExternalAccount?> GetByIdAsync(int id, CancellationToken ct = default) =>
            _db.ExternalAccounts
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.ID == id, ct);

        public Task<ExternalAccount?> GetByIbanAsync(string iban, CancellationToken ct = default) =>
            _db.ExternalAccounts
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.IBAN == iban, ct);

        public async Task AddAsync(ExternalAccount account, CancellationToken ct = default)
        {
            await _db.ExternalAccounts.AddAsync(account, ct);
            await _db.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(ExternalAccount account, CancellationToken ct = default)
        {
            _db.ExternalAccounts.Update(account);
            await _db.SaveChangesAsync(ct);
        }
        public Task<int> SaveChangesAsync(CancellationToken ct = default) =>
            _db.SaveChangesAsync(ct);
    }
}
