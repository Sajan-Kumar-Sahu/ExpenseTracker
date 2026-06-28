using ExpenseTracker.Domain.Common;
using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Domain.Entities
{
    public class Settlement : AuditableEntity
    {
        public Guid UserId { get; set; }

        public Guid ContactId { get; set; }

        public SettlementType SettlementType { get; set; }

        public string Reason { get; set; } = string.Empty;

        public decimal OriginalAmount { get; set; }

        public decimal PendingAmount { get; set; }

        public SettlementStatus Status { get; set; }

        public DateTimeOffset? DueDate { get; set; }

        public bool IsActive { get; set; } = true;

        public string? Notes { get; set; }

        public User User { get; set; } = null!;
        public Contact Contact { get; set; } = null!;
        public ICollection<FinancialTransaction> Transactions { get; set; } = new List<FinancialTransaction>();
    }
}
