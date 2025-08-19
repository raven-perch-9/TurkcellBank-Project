namespace TurkcellBank.Domain
{
    public class CreditInstallment
    {
        public int ID { get; set; }
        public int CreditApplicationID { get; set; }

        public int No { get; set; } // Term Months
        public decimal PrincipalPortion { get; set; }
        public decimal InterestPortion { get; set; }
        public decimal PayementAmount { get; set; } // Principal + Interest
        public decimal RemainingPrincipal { get; set; }
        public DateTime DueDate { get; set; }

        public bool IsPaid { get; set; }
        public DateTime? PaidAt { get; set; }

        public CreditApplication? CreditApplication { get; set; }
    }
}
