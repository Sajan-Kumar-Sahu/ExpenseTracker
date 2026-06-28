using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Application.DTOs.WorkLog
{
    public class CreateWorkLogRequest
    {
        public Guid ProjectId { get; set; }

        public DateOnly WorkDate { get; set; }

        public WorkType WorkType { get; set; }

        public TimeOnly StartTime { get; set; }

        public TimeOnly EndTime { get; set; }

        public decimal? WorkedHours { get; set; }

        public string TaskTitle { get; set; } = string.Empty;

        public string? Description { get; set; }

        public decimal? ExpectedAmount { get; set; }

        public string? ReferenceNumber { get; set; }

        public string? Notes { get; set; }
    }
}
