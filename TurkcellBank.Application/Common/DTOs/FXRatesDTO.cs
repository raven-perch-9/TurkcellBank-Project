namespace TurkcellBank.Application.Common.DTOs
{
    public sealed record FXRatesDTO(
        string Base,
        DateTimeOffset AsOf,
        IDictionary<string,decimal> Rates
    );
}