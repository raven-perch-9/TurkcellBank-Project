using TurkcellBank.Application.Common.Services.Interfaces;
using TurkcellBank.Application.Common.Abstractions;
using TurkcellBank.Domain;
using TurkcellBank.Domain.Enums;

namespace TurkcellBank.Application.Common.Services
{
    public sealed class DisbursementService : IDisbursementService
    {
        private readonly ICreditRepository _credits;

        public DisbursementService(ICreditRepository credits)
        {
            _credits = credits;
        }
    }
}