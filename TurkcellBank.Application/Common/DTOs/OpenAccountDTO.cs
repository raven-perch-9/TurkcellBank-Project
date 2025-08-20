// Data Type of the OpenAccount Request
// This DTO is used for opening a new account.

namespace TurkcellBank.Application.Common.DTOs
{
    public class OpenAccountDTO
    {
        public string AccountType { get; set; } = null!;
        public string CurrencyCode { get; set; } = null!;
        public decimal InitialDeposit { get; set; } = 0m;
    }
}
