using TurkcellBank.Domain.Entities;
using TurkcellBank.Domain.Enums;

namespace TurkcellBank.Application.Common.DTOs
{
    public class PaymentResponseDTO
    {
        public int PaymentId { get; set; }
        public PaymentStatus Status { get; set; }
        public string CardMask { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? AuthorizedAt { get; set; }
        public DateTime? CapturedAt { get; set; }
    }
}
