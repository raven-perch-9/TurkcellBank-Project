using TurkcellBank.Domain.Enums;

namespace TurkcellBank.Domain.Entities
{
    public class Payment
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int UserId { get; set; }
        
        public decimal Amount { get; set; }
        public CurrencyCode Currency { get; set; } = CurrencyCode.TRY;
        
        public string CardMask { get; set; } = string.Empty;
        public string CardFingerprint { get; set; } = string.Empty;

        public string? ThreeDSChallengeID { get; set; }
        public string? ThreeDSCodeHash { get; set; }
        public DateTime? ThreeDSExpiresAt { get; set; }

        public PaymentStatus Status { get; set; } = PaymentStatus.Initiated;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? AuthorizedAt { get; set; }
        public DateTime? CapturedAt { get; set; }

    }
}
