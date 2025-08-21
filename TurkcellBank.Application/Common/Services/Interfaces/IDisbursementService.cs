namespace TurkcellBank.Application.Common.Services.Interfaces
{
    public interface IDisbursementService
    {
        Task<bool> DisburseAsync(int creditApplicationId, CancellationToken ct = default);
    }
}
