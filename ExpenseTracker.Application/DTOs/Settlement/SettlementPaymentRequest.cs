namespace ExpenseTracker.Application.DTOs.Settlement
{
    public class SettlementPaymentRequest
    {
        public decimal Amount { get; set; }

        public Guid AccountId { get; set; }

        public Guid? CategoryId { get; set; }

        public DateTimeOffset TransactionDate { get; set; }

        public string? Notes { get; set; }
    }
}
