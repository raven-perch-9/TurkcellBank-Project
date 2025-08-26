using TurkcellBank.Domain.Enums;

namespace TurkcellBank.Application.Common.DTOs
{
    public class PaymentRequestDTO
    {
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public decimal Amount { get; set; }
        public CurrencyCode Currency { get; set; } = CurrencyCode.TRY; 
        public PaymentStatus Status { get; set; } = PaymentStatus.Initiated;
        public string CardMask { get; set; } = string.Empty;
        public string CardFingerprint { get; set; } = string.Empty;
        public string? ThreeDSChallengeID { get; set; }
        public string? ThreeDSCodeHash { get; set; }
        public DateTime? ThreeDSExpiresAt { get; set; }
    }
}
