namespace TurkcellBank.Domain.Entities
{
    public class ExternalAccount
    {
        public int ID { get; set; }
        public string IBAN { get; set; } = null!;
        public int AccountType { get; set; }
        public int CurrencyCode { get; set; }
        public decimal Balance { get; set; }
        public string OwnerName { get; set; } = null!;
        public bool IsActive { get; set; } = true;
    }
}
