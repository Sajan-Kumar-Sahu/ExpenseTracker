using ExpenseTracker.Domain.Common;
using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Domain.Entities
{
    public class WorkLog : AuditableEntity
    {
        public Guid UserId { get; set; }

        public Guid ProjectId { get; set; }

        public DateOnly WorkDate { get; set; }

        public WorkType WorkType { get; set; }

        public TimeOnly StartTime { get; set; }

        public TimeOnly EndTime { get; set; }

        public decimal WorkedHours { get; set; }

        public string TaskTitle { get; set; } = string.Empty;

        public string? Description { get; set; }

        public decimal? ExpectedAmount { get; set; }

        public decimal? ActualAmount { get; set; }

        public WorkLogStatus Status { get; set; } = WorkLogStatus.Draft;

        public string? ReferenceNumber { get; set; }

        public DateOnly? AppliedDate { get; set; }

        public DateOnly? ApprovedDate { get; set; }

        public DateOnly? PaidDate { get; set; }

        public string? PaymentMonth { get; set; }

        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public User User { get; set; } = null!;
        public Project Project { get; set; } = null!;
    }
}
