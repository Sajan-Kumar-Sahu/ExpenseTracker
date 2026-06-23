using ExpenseTracker.Domain.Common;
using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Domain.Entities
{
    public class FinancialTransaction : AuditableEntity
    {
        public Guid UserId { get; set; }

        public Guid AccountId { get; set; }

        public Guid? TransferAccountId { get; set; }

        public Guid? CategoryId { get; set; }

        public EntryType TransactionType { get; set; }

        public decimal Amount { get; set; }

        public DateTimeOffset TransactionDate { get; set; }

        public string? Party { get; set; }

        public string? Notes { get; set; }

        public User User { get; set; } = null!;

        public Account Account { get; set; } = null!;

        public Account? TransferAccount { get; set; }

        public Category? Category { get; set; }
    }
}
