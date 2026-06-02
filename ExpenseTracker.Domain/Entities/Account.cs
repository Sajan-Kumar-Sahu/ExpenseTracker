using ExpenseTracker.Domain.Common;
using ExpenseTracker.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExpenseTracker.Domain.Entities
{
    public class Account : AuditableEntity
    {
        public Guid UserId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public decimal OpeningBalance { get; set; }

        public bool IsActive { get; set; } = true;

        public AccountType AccountType { get; set; }

        public User User { get; set; } = null!;
        public ICollection<FinancialTransaction> Transactions { get; set; } = new List<FinancialTransaction>();
    }
}
