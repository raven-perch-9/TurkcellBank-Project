using TurkcellBank.Application.Common.Services;
using TurkcellBank.Domain; 
using TurkcellBank.Domain.Enums;

namespace TurkcellBank.Application.Common.Services.Interfaces
{
    public interface IInstallmentService
    {
        Task<IEnumerable<CreditInstallment>> GenerateAsync(CreditApplication app, DateTime startDate);
    }
}
