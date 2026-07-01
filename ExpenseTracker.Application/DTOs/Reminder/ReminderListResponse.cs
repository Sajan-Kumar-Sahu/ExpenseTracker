using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Application.DTOs.Reminder
{
    public class ReminderListResponse
    {
        public Guid Id { get; set; }

        public ReminderType ReminderType { get; set; }

        public ReferenceModule ReferenceModule { get; set; }

        public Guid ReferenceId { get; set; }

        public string Title { get; set; } = string.Empty;

        public DateTimeOffset ScheduledDate { get; set; }

        public ReminderPriority Priority { get; set; }

        public ReminderStatus Status { get; set; }

        public bool IsActive { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
    }
}
