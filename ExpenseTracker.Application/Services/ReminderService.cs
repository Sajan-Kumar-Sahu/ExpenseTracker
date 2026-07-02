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
        private readonly IUnitOfWork _unitOfWork;

        public ReminderService(
            IReminderRepository reminderRepository,
            IUnitOfWork unitOfWork)
        {
            _reminderRepository = reminderRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<List<ReminderResponse>>> CreateAsync(Guid userId, CreateReminderRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Title))
                return Result<List<ReminderResponse>>.Failure("Title is required.");

            if (request.Title.Trim().Length > 200)
                return Result<List<ReminderResponse>>.Failure("Title cannot exceed 200 characters.");

            if (string.IsNullOrWhiteSpace(request.Message))
                return Result<List<ReminderResponse>>.Failure("Message is required.");

            if (request.ReferenceId == Guid.Empty)
                return Result<List<ReminderResponse>>.Failure("ReferenceId is required.");

            if (!Enum.IsDefined(typeof(ReminderType), request.ReminderType))
                return Result<List<ReminderResponse>>.Failure("Invalid reminder type.");

            if (!Enum.IsDefined(typeof(ReferenceModule), request.ReferenceModule))
                return Result<List<ReminderResponse>>.Failure("Invalid reference module.");

            if (!Enum.IsDefined(typeof(ReminderPriority), request.Priority))
                return Result<List<ReminderResponse>>.Failure("Invalid priority.");

            var hasSingle = request.ScheduledDate.HasValue;
            var hasBatch = request.ScheduledDates is { Count: > 0 };

            if (hasSingle == hasBatch)
                return Result<List<ReminderResponse>>.Failure(
                    "Provide either ScheduledDate or ScheduledDates, not both.");

            if (hasBatch && request.RepeatType != RepeatType.None)
                return Result<List<ReminderResponse>>.Failure(
                    "RepeatType must be None when creating multiple scheduled times.");

            if (request.RepeatType == RepeatType.Custom && request.RepeatInterval is null or <= 0)
                return Result<List<ReminderResponse>>.Failure("RepeatInterval is required for custom repeat type.");

            var duplicate = await _reminderRepository.HasActiveReminderAsync(
                userId, request.ReferenceModule, request.ReferenceId, request.ReminderType);

            if (duplicate)
                return Result<List<ReminderResponse>>.Failure(
                    "An active reminder already exists for this reference.");

            var dates = hasBatch ? request.ScheduledDates! : new List<DateTimeOffset> { request.ScheduledDate!.Value };
            var groupId = dates.Count > 1 ? Guid.NewGuid() : (Guid?)null;

            var reminders = dates.Select(date => new Reminder
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ReminderGroupId = groupId,
                ReminderType = request.ReminderType,
                ReferenceModule = request.ReferenceModule,
                ReferenceId = request.ReferenceId,
                Title = request.Title.Trim(),
                Message = request.Message.Trim(),
                ScheduledDate = date,
                Priority = request.Priority,
                Status = ReminderStatus.Pending,
                RepeatType = request.RepeatType,
                RepeatInterval = request.RepeatInterval,
                IsPushNotificationEnabled = request.IsPushNotificationEnabled,
                IsInAppNotificationEnabled = request.IsInAppNotificationEnabled,
                NextTriggerAt = date,
                ExpiresAt = request.ExpiresAt,
                Notes = request.Notes?.Trim(),
                IsActive = true
            }).ToList();

            foreach (var reminder in reminders)
                await _reminderRepository.AddAsync(reminder);

            await _unitOfWork.SaveChangesAsync();

            return Result<List<ReminderResponse>>.Success(
                reminders.Select(Map).ToList(), "Reminder(s) created successfully.");
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

            if (reminder.ReminderGroupId is { } deleteGroupId)
            {
                var siblings = await _reminderRepository.GetByGroupIdAsync(deleteGroupId, excludeId: reminder.Id);
                foreach (var sibling in siblings)
                    await _reminderRepository.DeleteAsync(sibling);
            }

            await _reminderRepository.DeleteAsync(reminder);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success("Reminder deleted successfully.");
        }

        public async Task<Result<ReminderResponse>> CompleteAsync(Guid id, Guid userId)
        {
            var reminder = await _reminderRepository.GetByIdAsync(id);

            if (reminder is null)
                return Result<ReminderResponse>.Failure("Reminder not found.");

            if (reminder.UserId != userId)
                return Result<ReminderResponse>.Failure("Access denied.");

            if (reminder.Status is ReminderStatus.Completed or ReminderStatus.Cancelled)
                return Result<ReminderResponse>.Failure($"Reminder is already {reminder.Status}.");

            var now = DateTimeOffset.UtcNow;
            reminder.Status = ReminderStatus.Completed;
            reminder.CompletedAt = now;
            reminder.LastTriggeredAt = now;
            reminder.NextTriggerAt = null;
            await _reminderRepository.UpdateAsync(reminder);

            if (reminder.ReminderGroupId is { } groupId)
            {
                var siblings = await _reminderRepository.GetPendingByGroupIdAsync(groupId, excludeId: reminder.Id);
                foreach (var sibling in siblings)
                {
                    sibling.Status = ReminderStatus.Cancelled;
                    sibling.LastTriggeredAt = now;
                    sibling.NextTriggerAt = null;
                    await _reminderRepository.UpdateAsync(sibling);
                }
            }

            await _unitOfWork.SaveChangesAsync();
            return Result<ReminderResponse>.Success(Map(reminder), "Reminder completed successfully.");
        }

        public async Task<Result<List<ReminderListResponse>>> GetActiveByUserAsync(Guid userId)
        {
            var reminders = await _reminderRepository.GetPendingByUserIdAsync(userId);
            return Result<List<ReminderListResponse>>.Success(reminders.Select(MapToList).ToList());
        }

        public async Task<Result<ReminderDashboardResponse>> GetDashboardAsync(Guid userId)
        {
            var reminders = await _reminderRepository.GetByUserIdAsync(userId);

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
                    r.Priority == ReminderPriority.Critical)
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
            ReminderGroupId = reminder.ReminderGroupId,
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
            UserId = reminder.UserId,
            ReminderType = reminder.ReminderType,
            ReferenceModule = reminder.ReferenceModule,
            ReferenceId = reminder.ReferenceId,
            ReminderGroupId = reminder.ReminderGroupId,
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
    }
}
