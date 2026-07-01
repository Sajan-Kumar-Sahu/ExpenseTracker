using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Application.DTOs.Reminder
{
    public class UpdateReminderRequest
    {
        public string Title { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public DateTimeOffset ScheduledDate { get; set; }

        public ReminderPriority Priority { get; set; }

        public RepeatType RepeatType { get; set; }

        public int? RepeatInterval { get; set; }

        public bool IsPushNotificationEnabled { get; set; }

        public bool IsInAppNotificationEnabled { get; set; }

        public DateTimeOffset? ExpiresAt { get; set; }

        public string? Notes { get; set; }
    }
}
