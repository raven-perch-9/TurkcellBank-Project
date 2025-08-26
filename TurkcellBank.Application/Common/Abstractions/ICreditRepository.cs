using TurkcellBank.Domain.Entities;
using TurkcellBank.Domain.Enums;

namespace TurkcellBank.Application.Common.Abstractions
{
    public interface ICreditRepository
    {
        Task<CreditApplication?> GetApplicationByIdAsync(int id);
        Task<IReadOnlyList<CreditApplication>> GetByUserAsync(int userId);
        Task<IReadOnlyList<CreditApplication>> GetAllApplicationsAsync();

        Task<IReadOnlyList<CreditInstallment>> GetInstallmentsByCreditIdAsync(int creditId);
        Task<IReadOnlyList<CreditInstallment>> GetOverdueInstallmentsAsync(DateTime asOf);

        Task AddApplicationAsync(CreditApplication app);
        Task UpdateApplicationAsync(CreditApplication app);
        Task AddInstallmentsAsync(IEnumerable<CreditInstallment> items);
        Task UpdateInstallmentsAsync(CreditInstallment item);
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}
