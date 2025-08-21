using Microsoft.EntityFrameworkCore;
using TurkcellBank.Application.Common.Abstractions;
using TurkcellBank.Domain;
using TurkcellBank.Domain.Enums;

namespace TurkcellBank.Infrastructure.Data.Repositories
{
    public sealed class TransactionRepository : ITransactionRepository
    {
        private readonly AppDbContext _db;
        public TransactionRepository(AppDbContext db) => _db = db;

        public async Task<Transaction?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _db.Transactions
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.ID == id, ct);
        }

        public async Task<IReadOnlyList<Transaction>> GetByAccountIdAsync(int accountId, CancellationToken ct = default)
        {
            return await _db.Transactions
                .AsNoTracking()
                .Where(t => t.FromAccountID == accountId || t.ToAccountID == accountId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<Transaction>> GetByAccountIdRangeAsync(
            int accountId,
            DateTime fromUtc,
            DateTime toUtc,
            CancellationToken ct = default)
        {
            return await _db.Transactions
                .AsNoTracking()
                .Where(t => (t.FromAccountID == accountId || t.ToAccountID == accountId) &&
                            t.CreatedAt >= fromUtc && t.CreatedAt <= toUtc)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<Transaction>> GetLatestForAccountAsync(
            int accountId,
            int take = 50,
            CancellationToken ct = default)
        {
            return await _db.Transactions
                .AsNoTracking()
                .Where(t => t.FromAccountID == accountId || t.ToAccountID == accountId)
                .OrderByDescending(t => t.CreatedAt)
                .Take(take)
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<Transaction>> GetByStatusAsync(TransactionStatus status, CancellationToken ct = default)
        {
            return await _db.Transactions
                .AsNoTracking()
                .Where(t => t.Status == status)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync(ct);
        }

        public async Task<IReadOnlyList<Transaction>> GetByTypeAsync(TransferType type, CancellationToken ct = default)
        {
            return await _db.Transactions
           .AsNoTracking()
           .Where(t => t.Type == type)
           .OrderByDescending(t => t.CreatedAt)
           .ToListAsync(ct);
        }

        public async Task AddAsync(Transaction tx, CancellationToken ct = default)
        {
            await _db.Transactions.AddAsync(tx, ct);
        }

        public async Task AddRangeAsync(IEnumerable<Transaction> txs, CancellationToken ct = default)
        {
            await _db.Transactions.AddRangeAsync(txs, ct);
        }

        public async Task UpdateAsync(Transaction tx, CancellationToken ct = default)
        {
            _db.Transactions.Update(tx);
            await Task.CompletedTask;
        }

        public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            return await _db.SaveChangesAsync(ct);
        }
    }
}
