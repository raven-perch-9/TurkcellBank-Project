using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TurkcellBank.Application.Common.Abstractions;
using TurkcellBank.Domain.Entities;
using TurkcellBank.Domain.Enums;

namespace TurkcellBank.Infrastructure.Data.Repositories
{
    public sealed class TransactionRepository : ITransactionRepository
    {
        private readonly AppDbContext _db;
        private readonly ILogger<TransactionRepository>? _logger;

        public TransactionRepository(AppDbContext db, ILogger<TransactionRepository>? logger = null)
        {
            _db = db;
            _logger = logger;
        }

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
            try
            {
                await _db.Transactions.AddAsync(tx, ct);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "AddAsync failed for Transaction. FromAccountID={FromAccountID}," +
                                      " ToAccountID={ToAccountID}, Amount={Amount}, Type={Type}, Status={Status}",
                                      tx.FromAccountID, tx.ToAccountID, tx.Amount, tx.Type, tx.Status);
                throw new InvalidOperationException(
                    $"TransactionRepository.AddAsync failed: {ex.Message}", ex);
            }
        }

        public async Task AddRangeAsync(IEnumerable<Transaction> txs, CancellationToken ct = default)
        {
            await _db.Transactions.AddRangeAsync(txs, ct);
        }

        public async Task UpdateAsync(Transaction tx, CancellationToken ct = default)
        {
            try
            { 
                _db.Transactions.Update(tx);
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "UpdateAsync failed for Transaction ID={TransactionID}", tx.ID);
                throw new InvalidOperationException(
                    $"TransactionRepository.UpdateAsync failed: {ex.Message}", ex);
            }
        }

        public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            try
            {
                return await _db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex)
            {
                var inner = ex.InnerException?.Message ?? "(no inner exception)";
                _logger?.LogError(ex, "SaveChangesAsync failed: Inner: {InnerMessage}", inner);

                throw new InvalidOperationException(
                    $"TransactionRepository.SaveChanges failed. SQL/EF detail: {inner}", ex);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "SaveChangesAsync failed: {Message}", ex.Message);
                throw new InvalidOperationException(
                    $"TransactionRepository.SaveChanges failed: {ex.Message}", ex);
            }
        }
    }
}
