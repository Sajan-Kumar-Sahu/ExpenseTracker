using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.DTOs.Reminder;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Application.Repositories;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Application.Services
{
    public class ReminderService : IReminderService
    {
        private readonly IReminderRepository _reminderRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ReminderService(
            IReminderRepository reminderRepository,
            INotificationRepository notificationRepository,
            IUnitOfWork unitOfWork)
        {
            _reminderRepository = reminderRepository;
            _notificationRepository = notificationRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<ReminderResponse>> CreateAsync(Guid userId, CreateReminderRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Title))
                return Result<ReminderResponse>.Failure("Title is required.");

            if (request.Title.Trim().Length > 200)
                return Result<ReminderResponse>.Failure("Title cannot exceed 200 characters.");

            if (string.IsNullOrWhiteSpace(request.Message))
                return Result<ReminderResponse>.Failure("Message is required.");

            if (request.ReferenceId == Guid.Empty)
                return Result<ReminderResponse>.Failure("ReferenceId is required.");

            if (!Enum.IsDefined(typeof(ReminderType), request.ReminderType))
                return Result<ReminderResponse>.Failure("Invalid reminder type.");

            if (!Enum.IsDefined(typeof(ReferenceModule), request.ReferenceModule))
                return Result<ReminderResponse>.Failure("Invalid reference module.");

            if (!Enum.IsDefined(typeof(ReminderPriority), request.Priority))
                return Result<ReminderResponse>.Failure("Invalid priority.");

            if (request.RepeatType == RepeatType.Custom && request.RepeatInterval is null or <= 0)
                return Result<ReminderResponse>.Failure("RepeatInterval is required for custom repeat type.");

            var duplicate = await _reminderRepository.HasActiveReminderAsync(
                userId, request.ReferenceModule, request.ReferenceId, request.ReminderType);

            if (duplicate)
                return Result<ReminderResponse>.Failure(
                    "An active reminder already exists for this reference.");

            var reminder = new Reminder
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ReminderType = request.ReminderType,
                ReferenceModule = request.ReferenceModule,
                ReferenceId = request.ReferenceId,
                Title = request.Title.Trim(),
                Message = request.Message.Trim(),
                ScheduledDate = request.ScheduledDate,
                Priority = request.Priority,
                Status = ReminderStatus.Pending,
                RepeatType = request.RepeatType,
                RepeatInterval = request.RepeatInterval,
                IsPushNotificationEnabled = request.IsPushNotificationEnabled,
                IsInAppNotificationEnabled = request.IsInAppNotificationEnabled,
                ExpiresAt = request.ExpiresAt,
                Notes = request.Notes?.Trim(),
                IsActive = true
            };

            await _reminderRepository.AddAsync(reminder);
            await _unitOfWork.SaveChangesAsync();

            return Result<ReminderResponse>.Success(Map(reminder), "Reminder created successfully.");
        }

        public async Task<Result<ReminderResponse>> GetByIdAsync(Guid id, Guid userId)
        {
            var reminder = await _reminderRepository.GetByIdAsync(id);

            if (reminder is null)
                return Result<ReminderResponse>.Failure("Reminder not found.");

            if (reminder.UserId != userId)
                return Result<ReminderResponse>.Failure("Access denied.");

            return Result<ReminderResponse>.Success(Map(reminder));
        }

        public async Task<Result<List<ReminderListResponse>>> GetAllByUserAsync(Guid userId)
        {
            var reminders = await _reminderRepository.GetByUserIdAsync(userId);
            return Result<List<ReminderListResponse>>.Success(reminders.Select(MapToList).ToList());
        }

        public async Task<Result<List<ReminderListResponse>>> GetPendingAsync(Guid userId)
        {
            var reminders = await _reminderRepository.GetPendingByUserIdAsync(userId);
            return Result<List<ReminderListResponse>>.Success(reminders.Select(MapToList).ToList());
        }

        public async Task<Result<List<ReminderListResponse>>> GetTodayAsync(Guid userId)
        {
            var reminders = await _reminderRepository.GetTodayByUserIdAsync(userId);
            return Result<List<ReminderListResponse>>.Success(reminders.Select(MapToList).ToList());
        }

        public async Task<Result<List<ReminderListResponse>>> GetUpcomingAsync(Guid userId)
        {
            var reminders = await _reminderRepository.GetUpcomingByUserIdAsync(userId);
            return Result<List<ReminderListResponse>>.Success(reminders.Select(MapToList).ToList());
        }

        public async Task<Result<List<ReminderListResponse>>> GetOverdueAsync(Guid userId)
        {
            var reminders = await _reminderRepository.GetOverdueByUserIdAsync(userId);
            return Result<List<ReminderListResponse>>.Success(reminders.Select(MapToList).ToList());
        }

        public async Task<Result<ReminderResponse>> UpdateAsync(Guid id, Guid userId, UpdateReminderRequest request)
        {
            var reminder = await _reminderRepository.GetByIdAsync(id);

            if (reminder is null)
                return Result<ReminderResponse>.Failure("Reminder not found.");

            if (reminder.UserId != userId)
                return Result<ReminderResponse>.Failure("Access denied.");

            if (reminder.Status == ReminderStatus.Completed)
                return Result<ReminderResponse>.Failure("Cannot update a completed reminder.");

            if (reminder.Status == ReminderStatus.Cancelled)
                return Result<ReminderResponse>.Failure("Cannot update a cancelled reminder.");

            if (string.IsNullOrWhiteSpace(request.Title))
                return Result<ReminderResponse>.Failure("Title is required.");

            if (request.Title.Trim().Length > 200)
                return Result<ReminderResponse>.Failure("Title cannot exceed 200 characters.");

            if (string.IsNullOrWhiteSpace(request.Message))
                return Result<ReminderResponse>.Failure("Message is required.");

            if (request.RepeatType == RepeatType.Custom && request.RepeatInterval is null or <= 0)
                return Result<ReminderResponse>.Failure("RepeatInterval is required for custom repeat type.");

            reminder.Title = request.Title.Trim();
            reminder.Message = request.Message.Trim();
            reminder.ScheduledDate = request.ScheduledDate;
            reminder.Priority = request.Priority;
            reminder.RepeatType = request.RepeatType;
            reminder.RepeatInterval = request.RepeatInterval;
            reminder.IsPushNotificationEnabled = request.IsPushNotificationEnabled;
            reminder.IsInAppNotificationEnabled = request.IsInAppNotificationEnabled;
            reminder.ExpiresAt = request.ExpiresAt;
            reminder.Notes = request.Notes?.Trim();

            await _reminderRepository.UpdateAsync(reminder);
            await _unitOfWork.SaveChangesAsync();

            return Result<ReminderResponse>.Success(Map(reminder), "Reminder updated successfully.");
        }

        public async Task<Result> DeleteAsync(Guid id, Guid userId)
        {
            var reminder = await _reminderRepository.GetByIdAsync(id);

            if (reminder is null)
                return Result.Failure("Reminder not found.");

            if (reminder.UserId != userId)
                return Result.Failure("Access denied.");

            await _reminderRepository.DeleteAsync(reminder);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success("Reminder deleted successfully.");
        }

        public async Task<Result<ReminderDashboardResponse>> GetDashboardAsync(Guid userId)
        {
            var reminders = await _reminderRepository.GetByUserIdAsync(userId);
            var unreadCount = await _notificationRepository.GetUnreadCountAsync(userId);

            var now = DateTimeOffset.UtcNow;
            var todayStart = new DateTimeOffset(now.Date, TimeSpan.Zero);
            var todayEnd = todayStart.AddDays(1);
            var weekEnd = todayStart.AddDays(7);

            var dashboard = new ReminderDashboardResponse
            {
                TotalReminders = reminders.Count,
                PendingCount = reminders.Count(r => r.Status == ReminderStatus.Pending),
                CompletedCount = reminders.Count(r => r.Status == ReminderStatus.Completed),
                OverdueCount = reminders.Count(r =>
                    r.Status == ReminderStatus.Pending && r.ScheduledDate < now),
                TodayCount = reminders.Count(r =>
                    r.Status == ReminderStatus.Pending &&
                    r.ScheduledDate >= todayStart &&
                    r.ScheduledDate < todayEnd),
                UpcomingThisWeekCount = reminders.Count(r =>
                    r.Status == ReminderStatus.Pending &&
                    r.ScheduledDate >= now &&
                    r.ScheduledDate <= weekEnd),
                CriticalCount = reminders.Count(r =>
                    r.Status == ReminderStatus.Pending &&
                    r.Priority == ReminderPriority.Critical),
                UnreadNotificationsCount = unreadCount
            };

            return Result<ReminderDashboardResponse>.Success(dashboard);
        }

        private static ReminderResponse Map(Reminder reminder) => new()
        {
            Id = reminder.Id,
            UserId = reminder.UserId,
            ReminderType = reminder.ReminderType,
            ReferenceModule = reminder.ReferenceModule,
            ReferenceId = reminder.ReferenceId,
            Title = reminder.Title,
            Message = reminder.Message,
            ScheduledDate = reminder.ScheduledDate,
            Priority = reminder.Priority,
            Status = reminder.Status,
            RepeatType = reminder.RepeatType,
            RepeatInterval = reminder.RepeatInterval,
            IsPushNotificationEnabled = reminder.IsPushNotificationEnabled,
            IsInAppNotificationEnabled = reminder.IsInAppNotificationEnabled,
            LastTriggeredAt = reminder.LastTriggeredAt,
            NextTriggerAt = reminder.NextTriggerAt,
            CompletedAt = reminder.CompletedAt,
            ExpiresAt = reminder.ExpiresAt,
            Notes = reminder.Notes,
            IsActive = reminder.IsActive,
            CreatedAt = reminder.CreatedAt,
            UpdatedAt = reminder.UpdatedAt
        };

        private static ReminderListResponse MapToList(Reminder reminder) => new()
        {
            Id = reminder.Id,
            ReminderType = reminder.ReminderType,
            ReferenceModule = reminder.ReferenceModule,
            ReferenceId = reminder.ReferenceId,
            Title = reminder.Title,
            ScheduledDate = reminder.ScheduledDate,
            Priority = reminder.Priority,
            Status = reminder.Status,
            IsActive = reminder.IsActive,
            CreatedAt = reminder.CreatedAt
        };
    }
}
