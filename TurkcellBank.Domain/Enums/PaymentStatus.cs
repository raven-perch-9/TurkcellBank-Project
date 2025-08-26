namespace TurkcellBank.Domain.Enums
{
    public enum PaymentStatus
    {
        Initiated = 0,
        Requires3DS = 1,
        Authorized = 2,
        Captured = 3,
        Failed = 4
    }
}
