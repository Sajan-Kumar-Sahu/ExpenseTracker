using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.DTOs.Notification;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Application.Repositories;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IUnitOfWork _unitOfWork;

        public NotificationService(
            INotificationRepository notificationRepository,
            IUnitOfWork unitOfWork)
        {
            _notificationRepository = notificationRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<List<NotificationListResponse>>> GetAllByUserAsync(Guid userId)
        {
            var notifications = await _notificationRepository.GetByUserIdAsync(userId);
            return Result<List<NotificationListResponse>>.Success(notifications.Select(MapToList).ToList());
        }

        public async Task<Result<List<NotificationListResponse>>> GetUnreadAsync(Guid userId)
        {
            var notifications = await _notificationRepository.GetUnreadByUserIdAsync(userId);
            return Result<List<NotificationListResponse>>.Success(notifications.Select(MapToList).ToList());
        }

        public async Task<Result<List<NotificationListResponse>>> GetHistoryAsync(Guid userId)
        {
            var notifications = await _notificationRepository.GetByUserIdAsync(userId);
            return Result<List<NotificationListResponse>>.Success(notifications.Select(MapToList).ToList());
        }

        public async Task<Result<NotificationResponse>> GetByIdAsync(Guid id, Guid userId)
        {
            var notification = await _notificationRepository.GetByIdAsync(id);

            if (notification is null)
                return Result<NotificationResponse>.Failure("Notification not found.");

            if (notification.UserId != userId)
                return Result<NotificationResponse>.Failure("Access denied.");

            return Result<NotificationResponse>.Success(Map(notification));
        }

        public async Task<Result> MarkReadAsync(Guid id, Guid userId)
        {
            var notification = await _notificationRepository.GetByIdAsync(id);

            if (notification is null)
                return Result.Failure("Notification not found.");

            if (notification.UserId != userId)
                return Result.Failure("Access denied.");

            if (notification.IsRead)
                return Result.Success("Notification already marked as read.");

            notification.IsRead = true;
            notification.ReadAt = DateTimeOffset.UtcNow;
            notification.DeliveryStatus = DeliveryStatus.Opened;

            await _notificationRepository.UpdateAsync(notification);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success("Notification marked as read.");
        }

        public async Task<Result> MarkClickedAsync(Guid id, Guid userId)
        {
            var notification = await _notificationRepository.GetByIdAsync(id);

            if (notification is null)
                return Result.Failure("Notification not found.");

            if (notification.UserId != userId)
                return Result.Failure("Access denied.");

            if (!notification.IsRead)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTimeOffset.UtcNow;
            }

            notification.IsClicked = true;
            notification.ClickedAt = DateTimeOffset.UtcNow;
            notification.DeliveryStatus = DeliveryStatus.Opened;

            await _notificationRepository.UpdateAsync(notification);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success("Notification marked as clicked.");
        }

        public async Task<Result> MarkAllReadAsync(Guid userId)
        {
            await _notificationRepository.MarkAllReadAsync(userId);
            return Result.Success("All notifications marked as read.");
        }

        public async Task<Result> DeleteAsync(Guid id, Guid userId)
        {
            var notification = await _notificationRepository.GetByIdAsync(id);

            if (notification is null)
                return Result.Failure("Notification not found.");

            if (notification.UserId != userId)
                return Result.Failure("Access denied.");

            await _notificationRepository.DeleteAsync(notification);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success("Notification deleted.");
        }

        private static NotificationResponse Map(Notification notification) => new()
        {
            Id = notification.Id,
            UserId = notification.UserId,
            ReminderId = notification.ReminderId,
            Title = notification.Title,
            Body = notification.Body,
            NotificationType = notification.NotificationType,
            ReferenceModule = notification.ReferenceModule,
            ReferenceId = notification.ReferenceId,
            IsRead = notification.IsRead,
            ReadAt = notification.ReadAt,
            IsClicked = notification.IsClicked,
            ClickedAt = notification.ClickedAt,
            SentAt = notification.SentAt,
            DeliveryStatus = notification.DeliveryStatus,
            ActionUrl = notification.ActionUrl,
            CreatedAt = notification.CreatedAt
        };

        private static NotificationListResponse MapToList(Notification notification) => new()
        {
            Id = notification.Id,
            ReminderId = notification.ReminderId,
            Title = notification.Title,
            Body = notification.Body,
            NotificationType = notification.NotificationType,
            ReferenceModule = notification.ReferenceModule,
            ReferenceId = notification.ReferenceId,
            IsRead = notification.IsRead,
            SentAt = notification.SentAt,
            DeliveryStatus = notification.DeliveryStatus,
            ActionUrl = notification.ActionUrl
        };
    }
}
