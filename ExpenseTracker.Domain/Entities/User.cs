using ExpenseTracker.Domain.Common;
using ExpenseTracker.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExpenseTracker.Domain.Entities
{
    public class User : AuditableEntity
    {
        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public ICollection<Account> Accounts { get; set; } = new List<Account>();
        public ICollection<Category> Categories { get; set; } = new List<Category>();
        public ICollection<FinancialTransaction> Transactions { get; set; } = new List<FinancialTransaction>();
    }
}
