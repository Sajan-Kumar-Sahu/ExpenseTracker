using ExpenseTracker.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExpenseTracker.Application.DTOs.Account
{
    public class CreateAccountRequest
    {
        public Guid UserId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public decimal OpeningBalance { get; set; }

        public AccountType AccountType { get; set; }
    }
}
