using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Application.DTOs.Account
{
    public class CreateAccountRequest
    {
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public decimal OpeningBalance { get; set; }

        public AccountType AccountType { get; set; }
    }
}
