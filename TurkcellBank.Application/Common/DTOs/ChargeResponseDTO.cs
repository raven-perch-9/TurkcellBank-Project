using TurkcellBank.Domain.Enums;

namespace TurkcellBank.Application.Common.DTOs
{
    public class ChargeResponseDTO
    {
        public int OrderId { get; set; }
        public PaymentStatus Status { get; set; }
        public CurrencyCode Currency { get; set; } = CurrencyCode.TRY;
        public string CardMask { get; set; } = string.Empty;
        public string? ThreeDSChallengeID { get; set; }
        public string? ThreeDSCodeHash { get; set; }
        public DateTime? ThreeDSExpiresAt { get; set; }
    }
}
