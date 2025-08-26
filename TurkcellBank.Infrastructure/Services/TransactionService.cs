using System.Data;
using Microsoft.EntityFrameworkCore;
using TurkcellBank.Application.Common.DTOs;
using TurkcellBank.Domain.Entities;
using TurkcellBank.Domain.Enums;
using TurkcellBank.Infrastructure.Data;


namespace TurkcellBank.Infrastructure.Services
{
    //Temporary Interface 
    public interface ITransactionService
    {
        Task<TransactionDTO> TransferAsync(int userID, TransferRequestDTO req, CancellationToken ct = default);
        Task<IReadOnlyList<TransactionDTO>> GetHistorysAsync(int userID, string? IBAN = null,
            DateTime? from = null, DateTime? to = null, CancellationToken ct = default);
    }

    //Actual Service
    public class TransactionService : ITransactionService
    {
        private readonly AppDbContext _db;
        public TransactionService(AppDbContext db)
        {
            _db = db;
        }
        public async Task<TransactionDTO> TransferAsync(int userID, TransferRequestDTO req, CancellationToken ct = default)
        {
            // Validate request
            if (req.Amount <= 0)
                throw new ArgumentException("Transfer amount must be greater than zero.", nameof(req.Amount));
            if (string.Equals(req.FromAccountIBAN, req.ToAccountIBAN, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Cannot transfer to the same account.", nameof(req.ToAccountIBAN));

            //Load Accounts
            var from = await _db.Accounts
                .AsTracking()
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.UserID == userID && a.IBAN == req.FromAccountIBAN, ct)
                ?? throw new ArgumentException("Account is not found", nameof(req.FromAccountIBAN));

            if (from.UserID != userID)
                throw new UnauthorizedAccessException("You do not have permission to access this account.");
            if (!from.IsActive) throw new InvalidOperationException("Source account is not active.");

            var to = await _db.Accounts
                .AsTracking()
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.IBAN == req.ToAccountIBAN, ct)
                ?? throw new ArgumentException("Destination account is not found", nameof(req.ToAccountIBAN));

            if (!to.IsActive) throw new InvalidOperationException("Destination account is not active.");

            //Business logic checks
            if (from.CurrencyCode != to.CurrencyCode)
                throw new InvalidOperationException("Cross currency transfers are not allowed.");
            if (from.Balance < req.Amount)
                throw new InvalidOperationException("Insufficient balance in source account.");
            var type = (from.UserID == to.UserID) ? TransferType.Havale : TransferType.EFT;

            // Atomic balance update + transaction insertion
            await using var tx = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);

            from.Balance -= req.Amount;
            to.Balance += req.Amount;

            var entity = new Transaction
            {
                FromAccountID = from.ID,
                ToAccountID = to.ID,
                Amount = req.Amount,
                Description = req.Description,
                CreatedAt = DateTime.UtcNow,
                ReferenceCode = Guid.NewGuid().ToString("N"),
                Type = type,
                Status = TransactionStatus.Completed
            };

            _db.Transactions.Add(entity);
            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            return new TransactionDTO
            {
                ID = entity.ID,
                ReferenceCode = entity.ReferenceCode,
                CreatedAt = entity.CreatedAt,
                FromAccountIBAN = from.IBAN,
                ToAccountIBAN = to.IBAN,
                Amount = entity.Amount,
                Description = entity.Description,
                Type = entity.Type,
                Status = entity.Status
            };
        }
        public async Task<IReadOnlyList<TransactionDTO>> GetHistorysAsync(
            int userID, string? IBAN = null,
            DateTime? from = null, DateTime? to = null, CancellationToken ct = default)
        {
            var userAccountIDs = await _db.Accounts
                .Where(a => a.UserID == userID)
                .Select(a => new { a.ID, a.IBAN })
                .ToListAsync(ct);

            if (!userAccountIDs.Any())
                return Array.Empty<TransactionDTO>();

            int? filterAccountID = null;
            if (!string.IsNullOrWhiteSpace(IBAN))
            {
                filterAccountID = userAccountIDs.FirstOrDefault(x => x.IBAN == IBAN)?.ID
                      ?? throw new ArgumentException("Authorization Error", nameof(IBAN));
            }

            var q = _db.Transactions
                .AsNoTracking()
                .Include(t => t.FromAccount)
                .Include(t => t.ToAccount)
                .Where(t => (userAccountIDs.Select(x => x.ID).Contains(t.FromAccountID) ||
                             userAccountIDs.Select(x => x.ID).Contains(t.ToAccountID)));

            if (filterAccountID.HasValue)
            {
                q = q.Where(t => t.FromAccountID == filterAccountID.Value || t.ToAccountID == filterAccountID.Value);
            }
            if (from.HasValue) q = q.Where(t => t.CreatedAt >= from.Value);
            if (to.HasValue) q = q.Where(t => t.CreatedAt <= to.Value);

            var list = await q.OrderByDescending(t => t.CreatedAt)
                .Select(t => new TransactionDTO
                {
                    ID = t.ID,
                    ReferenceCode = t.ReferenceCode,
                    CreatedAt = t.CreatedAt,
                    FromAccountIBAN = t.FromAccount.IBAN,
                    ToAccountIBAN = t.ToAccount.IBAN,
                    Amount = t.Amount,
                    Description = t.Description,
                    Type = t.Type,
                    Status = t.Status
                })
                .ToListAsync(ct);
            return list;
        }
    }
}