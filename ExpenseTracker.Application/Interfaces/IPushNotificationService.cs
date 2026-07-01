using ExpenseTracker.Domain.Entities;

namespace ExpenseTracker.Application.Interfaces
{
    public interface IPushNotificationService
    {
        Task<bool> SendAsync(
            string deviceToken,
            string title,
            string body,
            Dictionary<string, string>? data = null);

        Task<bool> SendToUserAsync(
            Guid userId,
            string title,
            string body,
            Dictionary<string, string>? data = null);

        Task<bool> SendReminderAsync(Reminder reminder);
    }
}
