namespace TurkcellBank.Domain.Enums
{
    public enum CreditStatus
    {
        Pending = 0, // Credit application is pending approval
        Approved = 1, // Credit application has been approved
        Rejected = 2, // Credit application has been rejected
        Disbursed = 3, // Credit has been disbursed to the customer
        Closed = 4, // Credit account is closed
        Defaulted = 5 // Customer has defaulted on the credit
    }
}