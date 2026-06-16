using ExpenseTracker.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExpenseTracker.Application.DTOs.FinancialTransaction
{
    public class TransactionResponse
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public Guid AccountId { get; set; }

        public Guid? TransferAccountId { get; set; }

        public Guid? CategoryId { get; set; }

        public EntryType TransactionType { get; set; }

        public decimal Amount { get; set; }

        public DateTimeOffset TransactionDate { get; set; }

        public string? PaidTo { get; set; }

        public string? Notes { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
    }
}
