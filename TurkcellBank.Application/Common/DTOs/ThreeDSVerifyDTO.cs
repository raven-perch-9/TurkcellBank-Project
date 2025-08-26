using TurkcellBank.Domain.Enums;
using TurkcellBank.Domain.Entities;


namespace TurkcellBank.Application.Common.DTOs
{
    public class ThreeDSVerifyDTO
    {
        public int PaymentId { get; set; }
        public string Code { get; set; } = string.Empty;
    }
}
