using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.DTOs.Notification;

namespace ExpenseTracker.Application.Interfaces
{
    public interface INotificationService
    {
        Task<Result<List<NotificationListResponse>>> GetAllByUserAsync(Guid userId);

        Task<Result<List<NotificationListResponse>>> GetUnreadAsync(Guid userId);

        Task<Result<List<NotificationListResponse>>> GetHistoryAsync(Guid userId);

        Task<Result<NotificationResponse>> GetByIdAsync(Guid id, Guid userId);

        Task<Result> MarkReadAsync(Guid id, Guid userId);

        Task<Result> MarkClickedAsync(Guid id, Guid userId);

        Task<Result> MarkAllReadAsync(Guid userId);

        Task<Result> DeleteAsync(Guid id, Guid userId);
    }
}
