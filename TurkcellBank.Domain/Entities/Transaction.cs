using System;
using TurkcellBank.Domain.Enums;

namespace TurkcellBank.Domain.Entities
{
    public class Transaction
    {
        public int ID { get; set; }
        public int FromAccountID { get; set; }
        public int ToAccountID { get; set; }
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public TransferType Type { get; set; }
        public TransactionStatus Status { get; set; } = TransactionStatus.Pending;

        public string ReferenceCode { get; set; } = Guid.NewGuid().ToString("N");

        public Account? FromAccount { get; set; }
        public Account? ToAccount { get; set; }
    }
}