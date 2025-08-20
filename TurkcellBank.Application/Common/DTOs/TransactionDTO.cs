using TurkcellBank.Domain.Enums;

namespace TurkcellBank.Application.Common.DTOs
{
    public class TransactionDTO
    {
        public int ID { get; set; }
        public string ReferenceCode { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public string FromAccountIBAN { get; set; } = null!;
        public string ToAccountIBAN { get; set; } = null!;
        public decimal Amount { get; set; }
        public string? Description { get; set; }

        public TransferType Type{ get; set; }
        public TransactionStatus Status { get; set; }
    }
}
