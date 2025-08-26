using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using TurkcellBank.Application.Common.Abstractions;
using TurkcellBank.Domain.Entities;


namespace TurkcellBank.Infrastructure.Data.Repositories
{
    public sealed class PaymentRepository : IPaymentRepository
    {
        private readonly AppDbContext _db;
        public PaymentRepository(AppDbContext db) => _db = db;

        public Task AddAsync(Payment payment) => _db.Payments.AddAsync(payment).AsTask();

        public Task<Payment?> GetByThreeDSAsync(int userId, string challengeId) =>
            _db.Payments.FirstOrDefaultAsync(p => p.UserId == userId && p.ThreeDSChallengeID == challengeId);

        public async Task<IReadOnlyList<Payment?>> ListByUserAsync(int userId)
        {
            return await _db.Payments
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public Task<Payment> GetByIdAsync(int paymentId) => 
            _db.Payments.FirstAsync(p => p.OrderId == paymentId);
        public Task<List<Payment>> GetHistoryAsync(int userId) => _db.Payments
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        public Task<int> SaveChangesAsync() => _db.SaveChangesAsync();
    }
}
