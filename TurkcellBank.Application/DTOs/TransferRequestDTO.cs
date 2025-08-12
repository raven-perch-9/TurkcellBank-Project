using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkcellBank.Application.DTOs
{
    public class TransferRequestDTO
    {
        public string FromAccountIBAN { get; set; } = null!;
        public string ToAccountIBAN { get; set; } = null!;
        public decimal Amount { get; set; }
        public string? Description { get; set; }
    }
}
