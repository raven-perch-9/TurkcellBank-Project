using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkcellBank.Application.Common.DTOs
{
    public sealed class CreditApprovalDto
    {
        public decimal AcceptedAmount { get; set; }
        public int TermMonths { get; set; }
        public decimal AnnualRate { get; set; }

        // Optional (nullable)
        public DateTime? DecidedAt { get; set; }
        public string? DecisionBy { get; set; }
        public string? DecisionNote { get; set; }
    }
}
