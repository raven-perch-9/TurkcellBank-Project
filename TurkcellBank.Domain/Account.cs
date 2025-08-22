using TurkcellBank.Domain.Enums;

namespace TurkcellBank.Domain
{
    public class Account
    {
        public int ID { get; set; }
        public int UserID { get; set; }
        public string IBAN { get; set; } = null!;
        public AccountType AccountType { get; set; }
        public CurrencyCode CurrencyCode { get; set; }
        public decimal Balance { get; set; }
        public DateTime CreatedAt { get; set; }
        public User User { get; set; } = null!;
        public bool IsActive { get; set; } = true;
    }
}
