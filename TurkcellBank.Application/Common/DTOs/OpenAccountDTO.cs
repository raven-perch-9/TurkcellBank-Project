using TurkcellBank.Domain.Enums;

namespace TurkcellBank.Application.Common.DTOs
{
    public class OpenAccountDTO
    {
        public AccountType AccountType { get; set; }
        public CurrencyCode CurrencyCode { get; set; }
        public decimal InitialDeposit { get; set; } = 0m;
    }
}
