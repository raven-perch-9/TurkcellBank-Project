// This is basically a state flag for each transfer record.
// Tells us what happened in the transfer process.

namespace TurkcellBank.Domain.Enums
{
    public enum TransactionStatus
    {
        Pending = 1, // The transaction is pending and has not yet been processed.
        Completed = 2, // The transaction has been successfully completed.
        Failed = 3, // The transaction has failed.
        Cancelled = 4, // The transaction has been cancelled by the user or system.
        Refunded = 5 // The transaction has been refunded to the user.
    }
}
