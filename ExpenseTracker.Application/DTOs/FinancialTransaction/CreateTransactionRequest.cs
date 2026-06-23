using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Application.DTOs.FinancialTransaction
{
    public class CreateTransactionRequest
    {
        public Guid AccountId { get; set; }

        public Guid? TransferAccountId { get; set; }

        public Guid? CategoryId { get; set; }

        public EntryType TransactionType { get; set; }

        public decimal Amount { get; set; }

        public DateTimeOffset TransactionDate { get; set; }

        public string? PaidTo { get; set; }

        public string? Notes { get; set; }
    }
}
