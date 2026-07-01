using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Application.DTOs.Notification
{
    public class NotificationListResponse
    {
        public Guid Id { get; set; }

        public Guid ReminderId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Body { get; set; } = string.Empty;

        public NotificationType NotificationType { get; set; }

        public ReferenceModule ReferenceModule { get; set; }

        public Guid ReferenceId { get; set; }

        public bool IsRead { get; set; }

        public DateTimeOffset SentAt { get; set; }

        public DeliveryStatus DeliveryStatus { get; set; }

        public string? ActionUrl { get; set; }
    }
}
