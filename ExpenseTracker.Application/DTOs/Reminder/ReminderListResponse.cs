using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Application.DTOs.Reminder
{
    public class ReminderListResponse
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public ReminderType ReminderType { get; set; }

        public ReferenceModule ReferenceModule { get; set; }

        public Guid ReferenceId { get; set; }

        public Guid? ReminderGroupId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public DateTimeOffset ScheduledDate { get; set; }

        public ReminderPriority Priority { get; set; }

        public ReminderStatus Status { get; set; }

        public RepeatType RepeatType { get; set; }

        public int? RepeatInterval { get; set; }

        public bool IsPushNotificationEnabled { get; set; }

        public bool IsInAppNotificationEnabled { get; set; }

        public DateTimeOffset? LastTriggeredAt { get; set; }

        public DateTimeOffset? NextTriggerAt { get; set; }

        public DateTimeOffset? CompletedAt { get; set; }

        public DateTimeOffset? ExpiresAt { get; set; }

        public string? Notes { get; set; }

        public bool IsActive { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset? UpdatedAt { get; set; }
    }
}
