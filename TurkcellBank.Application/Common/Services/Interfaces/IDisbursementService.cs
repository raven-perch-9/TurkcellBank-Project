namespace TurkcellBank.Application.Common.Services.Interfaces
{
    internal interface IDisbursementService
    {
        Task DisburseAsync(int creidtApplicationId, CancellationToken ct = default);
    }
}
