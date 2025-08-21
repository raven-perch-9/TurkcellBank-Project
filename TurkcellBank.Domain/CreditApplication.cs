using TurkcellBank.Domain.Enums;

namespace TurkcellBank.Domain
{
    public class CreditApplication
    {
        public int ID { get; set; }
        public int UserID { get; set; }
        public decimal RequestedAmount { get; set; }
        public decimal AcceptedAmount { get; set; }

        // User Economic Information
        public decimal MonthlyIncome { get; set; }
        public string Occupation { get; set; } = string.Empty;
        public decimal Principle { get; set; }
        public int TermMonths { get; set; }
        public decimal AnnualRate { get; set; }

        // WorkFlow
        public CreditStatus Status { get; set; } = CreditStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DecidedAt { get; set; }
        public string? DecisionBy { get; set; }
        public string? DecisionNote { get; set; }
        public DateTime? DisbursedAt { get; set; }

        // Navigation
        public List<CreditInstallment> Schedule { get; set; } = new();
    }
}
