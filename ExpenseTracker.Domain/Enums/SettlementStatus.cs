using System;
using System.Collections.Generic;
using System.Text;

namespace ExpenseTracker.Domain.Enums
{
    public enum SettlementStatus
    {
        Pending = 1,
        Partial = 2,
        Completed = 3,
        Cancelled = 4
    }
}
