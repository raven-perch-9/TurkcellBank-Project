using TurkcellBank.Application.Common.Services.Interfaces;
using TurkcellBank.Application.Common.Abstractions;
using TurkcellBank.Domain.Entities;
using TurkcellBank.Domain.Enums;

namespace TurkcellBank.Application.Common.Services
{
    public sealed class DisbursementService : IDisbursementService
    {
        private readonly ICreditRepository _credits;
        private readonly IAccountRepository _accounts;
        private readonly ITransactionRepository _transactions;
        private readonly IGenerateIBAN _iban;

        public DisbursementService(ICreditRepository credits,
                                   IAccountRepository accounts,
                                   ITransactionRepository transactions,
                                   IGenerateIBAN iban)
        {
            _credits = credits;
            _accounts = accounts;
            _transactions = transactions;
            _iban = iban;
        }

        public async Task<bool> DisburseAsync(int creditApplicationId, CancellationToken ct = default)
        {
            const int bankSourceAccountId = 2005;
            var app = await _credits.GetApplicationByIdAsync(creditApplicationId);
            if (app == null)
            {
                throw new InvalidOperationException($"Credit application not found");
            }
            if (app.Status != CreditStatus.Approved)
            {
                throw new InvalidOperationException($"Credit application must be approved before disbursement");
            }
            var main = await _accounts.GetMainTryAccountAsync(app.UserID, ct);
            if (main == null)
            {
                main = new Account
                {
                    UserID = app.UserID,
                    AccountType = AccountType.VadesizMevduat,
                    CurrencyCode = CurrencyCode.TRY,
                    Balance = 0,
                    IsActive = true
                };
                await _accounts.AddAsync(main, ct);
                await _accounts.SaveChangesAsync(ct);

                main.IBAN = _iban.Generate(main.UserID, main.ID);
                await _accounts.UpdateAsync(main, ct);
                await _accounts.SaveChangesAsync(ct);
            }
            main.Balance += app.AcceptedAmount;
            await _accounts.UpdateAsync(main, ct);
            await _accounts.SaveChangesAsync(ct);

            var txn = new Transaction
            {
                FromAccountID = bankSourceAccountId,
                ToAccountID = main.ID,
                Amount = app.AcceptedAmount,
                Type = TransferType.CreditDisbursement,
                Status = TransactionStatus.Completed,
                CreatedAt = DateTime.UtcNow,
                Description = $"Kredi Yatırıldı (Başvuru #{app.ID})"
            };

            await _transactions.AddAsync(txn, ct);
            await _transactions.SaveChangesAsync(ct);

            app.Status = CreditStatus.Disbursed;
            app.DisbursedAt = DateTime.UtcNow;
            await _credits.UpdateApplicationAsync(app);
            await _credits.SaveChangesAsync(ct);

            return true;
        }
    }
}