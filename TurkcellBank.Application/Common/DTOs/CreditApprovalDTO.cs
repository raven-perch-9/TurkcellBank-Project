using TurkcellBank.Domain.Enums;

namespace TurkcellBank.Application.Common.DTOs
{
    public sealed class CreditApprovalDto
    {
        public decimal AcceptedAmount { get; set; }
        public int TermMonths { get; set; }
        public decimal AnnualRate { get; set; }
        public CreditStatus Status { get; set; }

        // Optional (nullable)
        public DateTime? DecidedAt { get; set; }
        public string? DecisionBy { get; set; }
        public string? DecisionNote { get; set; }
    }
}
