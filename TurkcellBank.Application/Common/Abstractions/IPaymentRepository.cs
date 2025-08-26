using TurkcellBank.Domain.Entities;
using TurkcellBank.Domain.Enums;

namespace TurkcellBank.Application.Common.Abstractions
{
    public interface IPaymentRepository
    {
        Task AddAsync(Payment payment);
        Task<Payment?> GetByThreeDSAsync(int userId, string challengeId);
        Task<Payment> GetByIdAsync(int paymentId);
        Task<IReadOnlyList<Payment?>> ListByUserAsync(int userId);
        Task<List<Payment>> GetHistoryAsync(int userId);
        Task<int> SaveChangesAsync();
    }
}
