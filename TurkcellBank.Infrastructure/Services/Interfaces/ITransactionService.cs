using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TurkcellBank.Application.Common.DTOs;

namespace TurkcellBank.Infrastructure.Services.Interfaces
{
    public interface ITransactionService
    {
        Task<TransactionDTO> TransferAsync(int userID, TransferRequestDTO req, CancellationToken ct = default);
        Task<IReadOnlyList<TransactionDTO>> GetHistorysAsync(int userID, string? IBAN = null,
            DateTime? from = null, DateTime? to = null, CancellationToken ct = default);
    }
}
