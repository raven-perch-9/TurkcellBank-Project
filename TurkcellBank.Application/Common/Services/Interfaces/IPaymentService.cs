using TurkcellBank.Application.Common.DTOs;


namespace TurkcellBank.Application.Common.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentResponseDTO> InitiateAsync(PaymentRequestDTO req);
        Task<PaymentResponseDTO> VerifyThreeDSAsync(ThreeDSVerifyDTO req);
        Task<PaymentResponseDTO> CaptureAsync(int paymentId);

        Task<IReadOnlyList<PaymentResponseDTO>> ListByUserAsync(int userId);
        Task<PaymentResponseDTO> GetAsync(int paymentId);
    }
}
