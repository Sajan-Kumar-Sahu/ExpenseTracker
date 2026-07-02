using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Application.DTOs.Reminder
{
    public class CreateReminderRequest
    {
        public ReminderType ReminderType { get; set; }

        public ReferenceModule ReferenceModule { get; set; }

        public Guid ReferenceId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public DateTimeOffset? ScheduledDate { get; set; }

        public List<DateTimeOffset>? ScheduledDates { get; set; }

        public ReminderPriority Priority { get; set; }

        public RepeatType RepeatType { get; set; }

        public int? RepeatInterval { get; set; }

        public bool IsPushNotificationEnabled { get; set; }

        public bool IsInAppNotificationEnabled { get; set; } = true;

        public DateTimeOffset? ExpiresAt { get; set; }

        public string? Notes { get; set; }
    }
}
