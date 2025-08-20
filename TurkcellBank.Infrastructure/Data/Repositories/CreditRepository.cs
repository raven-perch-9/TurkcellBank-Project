using Microsoft.EntityFrameworkCore;
using TurkcellBank.Application.Common.Abstractions;
using TurkcellBank.Domain;
using TurkcellBank.Domain.Enums;
using TurkcellBank.Infrastructure.Data.Repositories;

namespace TurkcellBank.Infrastructure.Data.Repositories
{
    public sealed class CreditRepository : ICreditRepository
    {
        private readonly AppDbContext _db;
        public CreditRepository(AppDbContext db) => _db = db;

        // Get single application (with its schedule if needed)
        public Task<CreditApplication?> GetApplicationByIdAsync(int id) =>
            _db.CreditApplications
                .Include(c => c.Schedule)
                .FirstOrDefaultAsync(c => c.ID == id);

        // Get all applications by user
        public async Task<IReadOnlyList<CreditApplication>> GetByUserAsync(int userId) =>
            await _db.CreditApplications
                .Where(c => c.UserID == userId)
                .Include(c => c.Schedule)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

        // Get installments for a given credit application
        public async Task<IReadOnlyList<CreditInstallment>> GetInstallmentsByCreditIdAsync(int creditId) =>
            await _db.CreditInstallments
                .Where(i => i.CreditApplicationID == creditId)
                .OrderBy(i => i.No)
                .ToListAsync();

        // Get overdue installments (not paid and due before asOf)
        public async Task<IReadOnlyList<CreditInstallment>> GetOverdueInstallmentsAsync(DateTime asOf) =>
            await _db.CreditInstallments
                .Where(i => !i.IsPaid && i.DueDate < asOf)
                .OrderBy(i => i.DueDate)
                .ToListAsync();

        // Add new credit application
        public Task AddApplicationAsync(CreditApplication app)
        {
            _db.CreditApplications.Add(app);
            return _db.SaveChangesAsync();
        }

        // Add multiple installments
        public Task AddInstallmentsAsync(IEnumerable<CreditInstallment> items)
        {
            _db.CreditInstallments.AddRange(items);
            return _db.SaveChangesAsync();
        }

        // Update single installment
        public Task UpdateInstallmentsAsync(CreditInstallment item)
        {
            _db.CreditInstallments.Update(item);
            return _db.SaveChangesAsync();
        }
    }
}
