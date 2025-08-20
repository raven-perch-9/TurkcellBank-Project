using TurkcellBank.Domain;
using TurkcellBank.Domain.Enums;

namespace TurkcellBank.Application.Common.Abstractions
{
    public interface IAccountRepository
    {
        Task<Account?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<IReadOnlyList<Account>> GetByUserAsync(int userId, CancellationToken ct = default);

        Task<Account?> GetMainTryAccountAsync(int userId, CancellationToken ct = default);

        Task AddAsync(Account account, CancellationToken ct = default);
        Task UpdateAsync(Account account, CancellationToken ct = default);
        Task<int> SaveChangesAsync(CancellationToken ct = default);
    }
}
