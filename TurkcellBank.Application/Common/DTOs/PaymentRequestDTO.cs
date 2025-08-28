using TurkcellBank.Domain.Enums;

namespace TurkcellBank.Application.Common.DTOs
{
    public class PaymentRequestDTO
    {
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public CurrencyCode Currency { get; set; } = CurrencyCode.TRY; 

    }
}
