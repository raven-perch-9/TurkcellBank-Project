using TurkcellBank.Application.Common.DTOs;

namespace TurkcellBank.Application.Common.Services.Interfaces
{
    public interface IFXFetcherService
    {
        Task<FXRatesDTO> GetLatestAsync(string @base, string[] symbols);
    }
}
