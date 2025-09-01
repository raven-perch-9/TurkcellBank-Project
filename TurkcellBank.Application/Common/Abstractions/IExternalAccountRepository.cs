using TurkcellBank.Domain.Entities;

namespace TurkcellBank.Application.Common.Abstractions
{
    public interface IExternalAccountRepository
    {
        Task<ExternalAccount?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<ExternalAccount?> GetByIbanAsync(string iban, CancellationToken ct = default);
        Task AddAsync(ExternalAccount account, CancellationToken ct = default);
        Task UpdateAsync(ExternalAccount account, CancellationToken ct = default);
        Task<int> SaveChangesAsync(CancellationToken ct = default);
    }
}
