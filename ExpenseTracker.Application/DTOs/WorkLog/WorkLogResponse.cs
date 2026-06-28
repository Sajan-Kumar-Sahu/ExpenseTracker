using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Application.DTOs.WorkLog
{
    public class WorkLogResponse
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public Guid ProjectId { get; set; }

        public string ProjectName { get; set; } = string.Empty;

        public DateOnly WorkDate { get; set; }

        public WorkType WorkType { get; set; }

        public TimeOnly StartTime { get; set; }

        public TimeOnly EndTime { get; set; }

        public decimal WorkedHours { get; set; }

        public string TaskTitle { get; set; } = string.Empty;

        public string? Description { get; set; }

        public decimal? ExpectedAmount { get; set; }

        public decimal? ActualAmount { get; set; }

        public WorkLogStatus Status { get; set; }

        public string? ReferenceNumber { get; set; }

        public DateOnly? AppliedDate { get; set; }

        public DateOnly? ApprovedDate { get; set; }

        public DateOnly? PaidDate { get; set; }

        public string? PaymentMonth { get; set; }

        public string? Notes { get; set; }

        public bool IsActive { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset? UpdatedAt { get; set; }
    }
}
