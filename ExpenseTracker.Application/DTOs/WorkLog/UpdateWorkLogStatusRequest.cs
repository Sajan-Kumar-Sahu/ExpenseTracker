using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Application.DTOs.WorkLog
{
    public class UpdateWorkLogStatusRequest
    {
        public WorkLogStatus Status { get; set; }
    }
}
