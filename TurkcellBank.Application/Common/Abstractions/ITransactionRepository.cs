using System.Transactions;
using TurkcellBank.Domain;
using TurkcellBank.Domain.Enums;

namespace TurkcellBank.Application.Common.Abstractions
{
    public interface ITransactionRepository
    {
        Task<TurkcellBank.Domain.Transaction?> GetByIdAsync(int id, CancellationToken ct = default);

        Task<IReadOnlyList<TurkcellBank.Domain.Transaction>> GetByAccountIdAsync(
            int accountId,
            CancellationToken ct = default);

        Task<IReadOnlyList<TurkcellBank.Domain.Transaction>> GetByAccountIdRangeAsync(
            int accountId,
            DateTime fromUtc,
            DateTime toUtc,
            CancellationToken ct = default);

        Task<IReadOnlyList<TurkcellBank.Domain.Transaction>> GetLatestForAccountAsync(
            int accountId,
            int take = 50,
            CancellationToken ct = default);

        Task<IReadOnlyList<TurkcellBank.Domain.Transaction>> GetByStatusAsync(
            TurkcellBank.Domain.Enums.TransactionStatus status,
            CancellationToken ct = default);

        Task<IReadOnlyList<TurkcellBank.Domain.Transaction>> GetByTypeAsync(
            TurkcellBank.Domain.Enums.TransferType type,
            CancellationToken ct = default);

        Task AddAsync(TurkcellBank.Domain.Transaction tx, CancellationToken ct = default);
        Task AddRangeAsync(IEnumerable<TurkcellBank.Domain.Transaction> txs, CancellationToken ct = default);
        Task UpdateAsync(TurkcellBank.Domain.Transaction tx, CancellationToken ct = default);
        Task<int> SaveChangesAsync(CancellationToken ct = default);
    }
}
