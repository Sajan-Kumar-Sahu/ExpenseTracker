using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Application.DTOs.Settlement
{
    public class SettlementListResponse
    {
        public Guid Id { get; set; }

        public Guid ContactId { get; set; }

        public string ContactName { get; set; } = string.Empty;

        public SettlementType SettlementType { get; set; }

        public string Reason { get; set; } = string.Empty;

        public decimal OriginalAmount { get; set; }

        public decimal PendingAmount { get; set; }

        public SettlementStatus Status { get; set; }

        public DateTimeOffset? DueDate { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
    }
}
