namespace TurkcellBank.Application.Common.DTOs
{
    public class CreditApplyDTO
    {
        public decimal RequestedAmount { get; set; }
        public int TermMonths { get; set; }
        public decimal MonthlyIncome { get; set; }
        public string Occupation { get; set; } = string.Empty;
        public decimal AnnualRate { get; set; }
    }
}
