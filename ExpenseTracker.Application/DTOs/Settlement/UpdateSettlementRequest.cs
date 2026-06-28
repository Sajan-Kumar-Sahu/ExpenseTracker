namespace ExpenseTracker.Application.DTOs.Settlement
{
    public class UpdateSettlementRequest
    {
        public Guid ContactId { get; set; }

        public string Reason { get; set; } = string.Empty;

        public decimal OriginalAmount { get; set; }

        public DateTimeOffset? DueDate { get; set; }

        public string? Notes { get; set; }
    }
}
