using TurkcellBank.Application.Common.Services.Interfaces;
using TurkcellBank.Application.Common.Abstractions;
using TurkcellBank.Domain;
using TurkcellBank.Domain.Enums;

/*namespace TurkcellBank.Application.Common.Services
{
    public sealed class InstallmentService : IInstallmentService
    {
        public Task<IEnumerable<CreditInstallment>> GenerateAsync(CreditApplication app, DateTime startDate)
        {
            var list = new List<CreditInstallment>();
            var firstDueDate = startDate.AddMonths(1);

            int months = 24;
            decimal principal = app.Prin
        }
    }
}
*/