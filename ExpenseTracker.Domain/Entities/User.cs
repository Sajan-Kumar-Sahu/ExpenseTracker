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

        public string MobileNumber { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public string? RefreshToken { get; set; }

        public DateTime? RefreshTokenExpiry { get; set; }

        public ICollection<Account> Accounts { get; set; } = new List<Account>();
        public ICollection<Category> Categories { get; set; } = new List<Category>();
        public ICollection<FinancialTransaction> Transactions { get; set; } = new List<FinancialTransaction>();
        public ICollection<Contact> Contacts { get; set; } = new List<Contact>();
        public ICollection<Settlement> Settlements { get; set; } = new List<Settlement>();
        public ICollection<Project> Projects { get; set; } = new List<Project>();
        public ICollection<WorkLog> WorkLogs { get; set; } = new List<WorkLog>();
        public ICollection<Reminder> Reminders { get; set; } = new List<Reminder>();
    }
}
