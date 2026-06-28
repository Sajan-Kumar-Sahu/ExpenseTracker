namespace ExpenseTracker.Application.DTOs.Settlement
{
    public class SettlementSummaryResponse
    {
        public decimal TotalReceivable { get; set; }

        public decimal TotalPayable { get; set; }

        public int PendingReceivableCount { get; set; }

        public int PendingPayableCount { get; set; }

        public int CompletedSettlementCount { get; set; }
    }
}
