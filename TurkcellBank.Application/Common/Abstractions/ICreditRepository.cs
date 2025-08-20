using TurkcellBank.Domain;
using TurkcellBank.Domain.Enums;

namespace TurkcellBank.Application.Common.Abstractions
{
    public interface ICreditRepository
    {
        Task<CreditApplication?> GetApplicationByIdAsync(int id);
        Task<IReadOnlyList<CreditApplication>> GetByUserAsync(int userId);

        Task<IReadOnlyList<CreditInstallment>> GetInstallmentsByCreditIdAsync(int creditId);
        Task<IReadOnlyList<CreditInstallment>> GetOverdueInstallmentsAsync(DateTime asOf);

        Task AddApplicationAsync(CreditApplication app);
        Task AddInstallmentsAsync(IEnumerable<CreditInstallment> items);
        Task UpdateInstallmentsAsync(CreditInstallment item);
    }
}
