using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Application.Repositories;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Application.Services
{
    public class ReminderProcessingService : IReminderProcessingService
    {
        private readonly IReminderRepository _reminderRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly IPushNotificationService _pushNotificationService;
        private readonly IUnitOfWork _unitOfWork;

        public ReminderProcessingService(
            IReminderRepository reminderRepository,
            INotificationRepository notificationRepository,
            IPushNotificationService pushNotificationService,
            IUnitOfWork unitOfWork)
        {
            _reminderRepository = reminderRepository;
            _notificationRepository = notificationRepository;
            _pushNotificationService = pushNotificationService;
            _unitOfWork = unitOfWork;
        }

        public async Task ProcessDueRemindersAsync(CancellationToken cancellationToken = default)
        {
            var dueReminders = await _reminderRepository.GetDueRemindersAsync();

            foreach (var reminder in dueReminders)
            {
                var notificationType = reminder.IsPushNotificationEnabled
                    ? NotificationType.Push
                    : NotificationType.InApp;

                var notification = new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = reminder.UserId,
                    ReminderId = reminder.Id,
                    Title = reminder.Title,
                    Body = reminder.Message,
                    NotificationType = notificationType,
                    ReferenceModule = reminder.ReferenceModule,
                    ReferenceId = reminder.ReferenceId,
                    SentAt = DateTimeOffset.UtcNow,
                    DeliveryStatus = DeliveryStatus.Pending,
                    IsRead = false,
                    IsClicked = false
                };

                bool pushSent = false;
                if (reminder.IsPushNotificationEnabled)
                {
                    pushSent = await _pushNotificationService.SendReminderAsync(reminder);
                }

                notification.DeliveryStatus = (reminder.IsInAppNotificationEnabled || pushSent)
                    ? DeliveryStatus.Sent
                    : DeliveryStatus.Failed;

                await _notificationRepository.AddAsync(notification);

                reminder.LastTriggeredAt = DateTimeOffset.UtcNow;

                if (reminder.RepeatType == RepeatType.None)
                {
                    reminder.Status = ReminderStatus.Completed;
                }
                else
                {
                    reminder.NextTriggerAt = CalculateNextTrigger(reminder);
                    reminder.ScheduledDate = reminder.NextTriggerAt.Value;
                }

                await _reminderRepository.UpdateAsync(reminder);
            }

            if (dueReminders.Count > 0)
                await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task ProcessExpiredRemindersAsync(CancellationToken cancellationToken = default)
        {
            var expiredReminders = await _reminderRepository.GetExpiredRemindersAsync();

            foreach (var reminder in expiredReminders)
            {
                reminder.Status = ReminderStatus.Expired;
                await _reminderRepository.UpdateAsync(reminder);
            }

            if (expiredReminders.Count > 0)
                await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        private static DateTimeOffset CalculateNextTrigger(Reminder reminder)
        {
            var baseDate = reminder.LastTriggeredAt ?? reminder.ScheduledDate;

            return reminder.RepeatType switch
            {
                RepeatType.Daily => baseDate.AddDays(1),
                RepeatType.Weekly => baseDate.AddDays(7),
                RepeatType.Monthly => baseDate.AddMonths(1),
                RepeatType.Yearly => baseDate.AddYears(1),
                RepeatType.Custom => baseDate.AddDays(reminder.RepeatInterval ?? 1),
                _ => baseDate
            };
        }
    }
}
