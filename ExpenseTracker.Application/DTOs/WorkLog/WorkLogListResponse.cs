using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Application.DTOs.WorkLog
{
    public class WorkLogListResponse
    {
        public Guid Id { get; set; }

        public Guid ProjectId { get; set; }

        public string ProjectName { get; set; } = string.Empty;

        public DateOnly WorkDate { get; set; }

        public WorkType WorkType { get; set; }

        public decimal WorkedHours { get; set; }

        public string TaskTitle { get; set; } = string.Empty;

        public decimal? ExpectedAmount { get; set; }

        public decimal? ActualAmount { get; set; }

        public WorkLogStatus Status { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
    }
}
