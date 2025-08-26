using System.Transactions;
using TurkcellBank.Domain.Enums;

namespace TurkcellBank.Application.Common.Abstractions
{
    public interface ITransactionRepository
    {
        Task<Domain.Entities.Transaction?> GetByIdAsync(int id, CancellationToken ct = default);

        Task<IReadOnlyList<Domain.Entities.Transaction>> GetByAccountIdAsync(
            int accountId,
            CancellationToken ct = default);

        Task<IReadOnlyList<Domain.Entities.Transaction>> GetByAccountIdRangeAsync(
            int accountId,
            DateTime fromUtc,
            DateTime toUtc,
            CancellationToken ct = default);

        Task<IReadOnlyList<Domain.Entities.Transaction>> GetLatestForAccountAsync(
            int accountId,
            int take = 50,
            CancellationToken ct = default);

        Task<IReadOnlyList<Domain.Entities.Transaction>> GetByStatusAsync(
            TurkcellBank.Domain.Enums.TransactionStatus status,
            CancellationToken ct = default);

        Task<IReadOnlyList<Domain.Entities.Transaction>> GetByTypeAsync(
            TurkcellBank.Domain.Enums.TransferType type,
            CancellationToken ct = default);

        Task AddAsync(Domain.Entities.Transaction tx, CancellationToken ct = default);
        Task AddRangeAsync(IEnumerable<Domain.Entities.Transaction> txs, CancellationToken ct = default);
        Task UpdateAsync(Domain.Entities.Transaction tx, CancellationToken ct = default);
        Task<int> SaveChangesAsync(CancellationToken ct = default);
    }
}
