using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Application.DTOs.Settlement
{
    public class CreateSettlementRequest
    {
        public Guid ContactId { get; set; }

        public SettlementType SettlementType { get; set; }

        public string Reason { get; set; } = string.Empty;

        public decimal OriginalAmount { get; set; }

        public DateTimeOffset? DueDate { get; set; }

        public string? Notes { get; set; }
    }
}
