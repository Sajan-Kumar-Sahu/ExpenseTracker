using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.DTOs.WorkLog;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Application.Repositories;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Application.Services
{
    public class WorkLogService : IWorkLogService
    {
        private readonly IWorkLogRepository _workLogRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IReminderRepository _reminderRepository;
        private readonly IUnitOfWork _unitOfWork;

        public WorkLogService(
            IWorkLogRepository workLogRepository,
            IProjectRepository projectRepository,
            IReminderRepository reminderRepository,
            IUnitOfWork unitOfWork)
        {
            _workLogRepository = workLogRepository;
            _projectRepository = projectRepository;
            _reminderRepository = reminderRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<WorkLogResponse>> CreateAsync(Guid userId, CreateWorkLogRequest request)
        {
            var validationError = await ValidateWorkLogRequestAsync(
                userId, request.ProjectId, request.WorkDate, request.WorkType,
                request.TaskTitle, request.StartTime, request.EndTime,
                request.WorkedHours, request.ExpectedAmount);

            if (validationError is not null)
                return Result<WorkLogResponse>.Failure(validationError);

            var project = await _projectRepository.GetByIdAsync(request.ProjectId);

            var workedHours = ResolveWorkedHours(request.StartTime, request.EndTime, request.WorkedHours);

            var workLog = new WorkLog
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ProjectId = request.ProjectId,
                WorkDate = request.WorkDate,
                WorkType = request.WorkType,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                WorkedHours = workedHours,
                TaskTitle = request.TaskTitle.Trim(),
                Description = request.Description?.Trim(),
                ExpectedAmount = request.ExpectedAmount,
                Status = WorkLogStatus.Draft,
                ReferenceNumber = request.ReferenceNumber?.Trim(),
                Notes = request.Notes?.Trim(),
                IsActive = true
            };

            await _workLogRepository.AddAsync(workLog);

            var reminder = new Reminder
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ReminderType = ReminderType.WorkLogApply,
                ReferenceModule = ReferenceModule.WorkLog,
                ReferenceId = workLog.Id,
                Title = "Apply Overtime",
                Message = "Don't forget to apply your overtime.",
                ScheduledDate = new DateTimeOffset(
                    workLog.WorkDate.AddDays(5).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc)),
                Priority = ReminderPriority.Medium,
                Status = ReminderStatus.Pending,
                RepeatType = RepeatType.None,
                IsPushNotificationEnabled = true,
                IsInAppNotificationEnabled = true,
                IsActive = true
            };

            await _reminderRepository.AddAsync(reminder);
            await _unitOfWork.SaveChangesAsync();

            workLog.Project = project!;

            return Result<WorkLogResponse>.Success(Map(workLog), "Work log created successfully.");
        }

        public async Task<Result<WorkLogResponse>> GetByIdAsync(Guid id, Guid userId)
        {
            var workLog = await _workLogRepository.GetByIdAsync(id);

            if (workLog is null)
                return Result<WorkLogResponse>.Failure("Work log not found.");

            if (workLog.UserId != userId)
                return Result<WorkLogResponse>.Failure("Access denied.");

            return Result<WorkLogResponse>.Success(Map(workLog));
        }

        public async Task<Result<List<WorkLogListResponse>>> GetAllByUserAsync(Guid userId)
        {
            var workLogs = await _workLogRepository.GetByUserIdAsync(userId);
            return Result<List<WorkLogListResponse>>.Success(workLogs.Select(MapToList).ToList());
        }

        public async Task<Result<WorkLogResponse>> UpdateAsync(Guid id, Guid userId, UpdateWorkLogRequest request)
        {
            var workLog = await _workLogRepository.GetByIdAsync(id);

            if (workLog is null)
                return Result<WorkLogResponse>.Failure("Work log not found.");

            if (workLog.UserId != userId)
                return Result<WorkLogResponse>.Failure("Access denied.");

            if (workLog.Status == WorkLogStatus.Paid)
            {
                workLog.Notes = request.Notes?.Trim();
                await _workLogRepository.UpdateAsync(workLog);
                await _unitOfWork.SaveChangesAsync();
                return Result<WorkLogResponse>.Success(Map(workLog), "Notes updated successfully.");
            }

            var validationError = await ValidateWorkLogRequestAsync(
                userId, request.ProjectId, request.WorkDate, request.WorkType,
                request.TaskTitle, request.StartTime, request.EndTime,
                request.WorkedHours, request.ExpectedAmount);

            if (validationError is not null)
                return Result<WorkLogResponse>.Failure(validationError);

            var project = await _projectRepository.GetByIdAsync(request.ProjectId);

            var workedHours = ResolveWorkedHours(request.StartTime, request.EndTime, request.WorkedHours);

            workLog.ProjectId = request.ProjectId;
            workLog.WorkDate = request.WorkDate;
            workLog.WorkType = request.WorkType;
            workLog.StartTime = request.StartTime;
            workLog.EndTime = request.EndTime;
            workLog.WorkedHours = workedHours;
            workLog.TaskTitle = request.TaskTitle.Trim();
            workLog.Description = request.Description?.Trim();
            workLog.ExpectedAmount = request.ExpectedAmount;
            workLog.ReferenceNumber = request.ReferenceNumber?.Trim();
            workLog.Notes = request.Notes?.Trim();
            workLog.Project = project!;

            await _workLogRepository.UpdateAsync(workLog);
            await _unitOfWork.SaveChangesAsync();

            return Result<WorkLogResponse>.Success(Map(workLog), "Work log updated successfully.");
        }

        public async Task<Result> DeleteAsync(Guid id, Guid userId)
        {
            var workLog = await _workLogRepository.GetByIdAsync(id);

            if (workLog is null)
                return Result.Failure("Work log not found.");

            if (workLog.UserId != userId)
                return Result.Failure("Access denied.");

            if (workLog.Status == WorkLogStatus.Paid)
                return Result.Failure("Cannot delete a paid work log.");

            await _workLogRepository.DeleteAsync(workLog);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success("Work log deleted successfully.");
        }

        public async Task<Result<List<WorkLogListResponse>>> GetByStatusAsync(Guid userId, WorkLogStatus status)
        {
            if (!Enum.IsDefined(typeof(WorkLogStatus), status))
                return Result<List<WorkLogListResponse>>.Failure("Invalid status value.");

            var workLogs = await _workLogRepository.GetByStatusAsync(userId, status);
            return Result<List<WorkLogListResponse>>.Success(workLogs.Select(MapToList).ToList());
        }

        public async Task<Result<List<WorkLogListResponse>>> GetByWorkTypeAsync(Guid userId, WorkType workType)
        {
            if (!Enum.IsDefined(typeof(WorkType), workType))
                return Result<List<WorkLogListResponse>>.Failure("Invalid work type value.");

            var workLogs = await _workLogRepository.GetByWorkTypeAsync(userId, workType);
            return Result<List<WorkLogListResponse>>.Success(workLogs.Select(MapToList).ToList());
        }

        public async Task<Result<List<WorkLogListResponse>>> GetByProjectAsync(Guid projectId, Guid userId)
        {
            var project = await _projectRepository.GetByIdAsync(projectId);

            if (project is null)
                return Result<List<WorkLogListResponse>>.Failure("Project not found.");

            if (project.UserId != userId)
                return Result<List<WorkLogListResponse>>.Failure("Access denied.");

            var workLogs = await _workLogRepository.GetByProjectIdAsync(projectId, userId);
            return Result<List<WorkLogListResponse>>.Success(workLogs.Select(MapToList).ToList());
        }

        public async Task<Result<List<WorkLogListResponse>>> GetByMonthAsync(Guid userId, int year, int month)
        {
            if (year < 2000 || year > 2100)
                return Result<List<WorkLogListResponse>>.Failure("Invalid year.");

            if (month < 1 || month > 12)
                return Result<List<WorkLogListResponse>>.Failure("Invalid month. Must be between 1 and 12.");

            var workLogs = await _workLogRepository.GetByMonthAsync(userId, year, month);
            return Result<List<WorkLogListResponse>>.Success(workLogs.Select(MapToList).ToList());
        }

        public async Task<Result<List<WorkLogListResponse>>> GetPendingAsync(Guid userId)
        {
            var workLogs = await _workLogRepository.GetPendingAsync(userId);
            return Result<List<WorkLogListResponse>>.Success(workLogs.Select(MapToList).ToList());
        }

        public async Task<Result<List<WorkLogListResponse>>> GetAppliedNotPaidAsync(Guid userId)
        {
            var workLogs = await _workLogRepository.GetAppliedNotPaidAsync(userId);
            return Result<List<WorkLogListResponse>>.Success(workLogs.Select(MapToList).ToList());
        }

        public async Task<Result<WorkLogDashboardResponse>> GetDashboardAsync(Guid userId)
        {
            var workLogs = await _workLogRepository.GetByUserIdAsync(userId);

            var dashboard = new WorkLogDashboardResponse
            {
                TotalEntries = workLogs.Count,
                DraftCount = workLogs.Count(w => w.Status == WorkLogStatus.Draft),
                AppliedCount = workLogs.Count(w => w.Status == WorkLogStatus.Applied),
                ApprovedCount = workLogs.Count(w => w.Status == WorkLogStatus.Approved),
                PaidCount = workLogs.Count(w => w.Status == WorkLogStatus.Paid),
                RejectedCount = workLogs.Count(w => w.Status == WorkLogStatus.Rejected),
                CancelledCount = workLogs.Count(w => w.Status == WorkLogStatus.Cancelled),
                TotalWorkedHours = workLogs.Sum(w => w.WorkedHours),
                TotalExpectedAmount = workLogs.Sum(w => w.ExpectedAmount ?? 0),
                TotalReceivedAmount = workLogs
                    .Where(w => w.Status == WorkLogStatus.Paid)
                    .Sum(w => w.ActualAmount ?? 0),
                TotalPendingAmount = workLogs
                    .Where(w => w.Status != WorkLogStatus.Paid
                             && w.Status != WorkLogStatus.Cancelled
                             && w.Status != WorkLogStatus.Rejected)
                    .Sum(w => w.ExpectedAmount ?? 0),
                TotalWeekendEntries = workLogs.Count(w => w.WorkType == WorkType.Weekend),
                TotalHolidayEntries = workLogs.Count(w => w.WorkType == WorkType.PublicHoliday),
                TotalOnCallEntries = workLogs.Count(w => w.WorkType == WorkType.OnCallSupport),
                TotalProductionSupportEntries = workLogs.Count(w => w.WorkType == WorkType.ProductionSupport)
            };

            return Result<WorkLogDashboardResponse>.Success(dashboard);
        }

        public async Task<Result<WorkLogYearlySummaryResponse>> GetYearlySummaryAsync(Guid userId, int year)
        {
            if (year < 2000 || year > 2100)
                return Result<WorkLogYearlySummaryResponse>.Failure("Invalid year.");

            var allWorkLogs = await _workLogRepository.GetByUserIdAsync(userId);
            var yearlyLogs = allWorkLogs.Where(w => w.WorkDate.Year == year).ToList();

            var summary = new WorkLogYearlySummaryResponse
            {
                Year = year,
                TotalWorkedDays = yearlyLogs.Select(w => w.WorkDate).Distinct().Count(),
                TotalWorkedHours = yearlyLogs.Sum(w => w.WorkedHours),
                TotalExpectedAmount = yearlyLogs.Sum(w => w.ExpectedAmount ?? 0),
                TotalReceivedAmount = yearlyLogs
                    .Where(w => w.Status == WorkLogStatus.Paid)
                    .Sum(w => w.ActualAmount ?? 0),
                PendingAmount = yearlyLogs
                    .Where(w => w.Status != WorkLogStatus.Paid
                             && w.Status != WorkLogStatus.Cancelled
                             && w.Status != WorkLogStatus.Rejected)
                    .Sum(w => w.ExpectedAmount ?? 0),
                WeekendCount = yearlyLogs.Count(w => w.WorkType == WorkType.Weekend),
                HolidayCount = yearlyLogs.Count(w => w.WorkType == WorkType.PublicHoliday),
                OnCallCount = yearlyLogs.Count(w => w.WorkType == WorkType.OnCallSupport),
                LateNightCount = yearlyLogs.Count(w => w.WorkType == WorkType.LateNightDeployment),
                ProductionSupportCount = yearlyLogs.Count(w => w.WorkType == WorkType.ProductionSupport)
            };

            return Result<WorkLogYearlySummaryResponse>.Success(summary);
        }

        public async Task<Result<WorkLogResponse>> ApplyAsync(Guid id, Guid userId)
        {
            var workLog = await _workLogRepository.GetByIdAsync(id);

            if (workLog is null)
                return Result<WorkLogResponse>.Failure("Work log not found.");

            if (workLog.UserId != userId)
                return Result<WorkLogResponse>.Failure("Access denied.");

            if (workLog.Status != WorkLogStatus.Draft)
                return Result<WorkLogResponse>.Failure("Only Draft work logs can be applied.");

            workLog.Status = WorkLogStatus.Applied;
            workLog.AppliedDate = DateOnly.FromDateTime(DateTime.UtcNow);

            var reminder = await _reminderRepository.GetActiveReminderByReferenceAsync(
                workLog.UserId, ReferenceModule.WorkLog, workLog.Id, ReminderType.WorkLogApply);

            if (reminder is not null)
            {
                reminder.Status = ReminderStatus.Completed;
                reminder.CompletedAt = DateTimeOffset.UtcNow;
                await _reminderRepository.UpdateAsync(reminder);
            }

            await _workLogRepository.UpdateAsync(workLog);
            await _unitOfWork.SaveChangesAsync();

            return Result<WorkLogResponse>.Success(Map(workLog), "Work log applied successfully.");
        }

        public async Task<Result<WorkLogResponse>> ApproveAsync(Guid id, Guid userId)
        {
            var workLog = await _workLogRepository.GetByIdAsync(id);

            if (workLog is null)
                return Result<WorkLogResponse>.Failure("Work log not found.");

            if (workLog.UserId != userId)
                return Result<WorkLogResponse>.Failure("Access denied.");

            if (workLog.Status != WorkLogStatus.Applied)
                return Result<WorkLogResponse>.Failure("Only Applied work logs can be approved.");

            workLog.Status = WorkLogStatus.Approved;
            workLog.ApprovedDate = DateOnly.FromDateTime(DateTime.UtcNow);

            await _workLogRepository.UpdateAsync(workLog);
            await _unitOfWork.SaveChangesAsync();

            return Result<WorkLogResponse>.Success(Map(workLog), "Work log approved successfully.");
        }

        public async Task<Result<WorkLogResponse>> RejectAsync(Guid id, Guid userId)
        {
            var workLog = await _workLogRepository.GetByIdAsync(id);

            if (workLog is null)
                return Result<WorkLogResponse>.Failure("Work log not found.");

            if (workLog.UserId != userId)
                return Result<WorkLogResponse>.Failure("Access denied.");

            if (workLog.Status != WorkLogStatus.Applied && workLog.Status != WorkLogStatus.Approved)
                return Result<WorkLogResponse>.Failure("Only Applied or Approved work logs can be rejected.");

            workLog.Status = WorkLogStatus.Rejected;

            await _workLogRepository.UpdateAsync(workLog);
            await _unitOfWork.SaveChangesAsync();

            return Result<WorkLogResponse>.Success(Map(workLog), "Work log rejected.");
        }

        public async Task<Result<WorkLogResponse>> MarkPaidAsync(Guid id, Guid userId, MarkPaidRequest request)
        {
            var workLog = await _workLogRepository.GetByIdAsync(id);

            if (workLog is null)
                return Result<WorkLogResponse>.Failure("Work log not found.");

            if (workLog.UserId != userId)
                return Result<WorkLogResponse>.Failure("Access denied.");

            if (workLog.Status != WorkLogStatus.Approved)
                return Result<WorkLogResponse>.Failure("Only Approved work logs can be marked as Paid.");

            if (request.ActualAmount < 0)
                return Result<WorkLogResponse>.Failure("Actual amount cannot be negative.");

            if (string.IsNullOrWhiteSpace(request.PaymentMonth))
                return Result<WorkLogResponse>.Failure("Payment month is required.");

            workLog.Status = WorkLogStatus.Paid;
            workLog.PaidDate = DateOnly.FromDateTime(DateTime.UtcNow);
            workLog.ActualAmount = request.ActualAmount;
            workLog.PaymentMonth = request.PaymentMonth.Trim();

            await _workLogRepository.UpdateAsync(workLog);
            await _unitOfWork.SaveChangesAsync();

            return Result<WorkLogResponse>.Success(Map(workLog), "Work log marked as paid.");
        }

        private async Task<string?> ValidateWorkLogRequestAsync(
            Guid userId,
            Guid projectId,
            DateOnly workDate,
            WorkType workType,
            string taskTitle,
            TimeOnly startTime,
            TimeOnly endTime,
            decimal? workedHours,
            decimal? expectedAmount)
        {
            if (workDate > DateOnly.FromDateTime(DateTime.UtcNow))
                return "Work date cannot be in the future.";

            if (!Enum.IsDefined(typeof(WorkType), workType))
                return "Invalid work type.";

            if (string.IsNullOrWhiteSpace(taskTitle))
                return "Task title is required.";

            if (taskTitle.Trim().Length > 300)
                return "Task title cannot exceed 300 characters.";

            if (endTime <= startTime)
                return "End time must be after start time.";

            if (workedHours.HasValue && workedHours.Value < 0)
                return "Worked hours cannot be negative.";

            if (expectedAmount.HasValue && expectedAmount.Value < 0)
                return "Expected amount cannot be negative.";

            var project = await _projectRepository.GetByIdAsync(projectId);

            if (project is null)
                return "Project not found.";

            if (project.UserId != userId)
                return "Project does not belong to this user.";

            if (!project.IsActive)
                return "Project is inactive.";

            return null;
        }

        private static decimal ResolveWorkedHours(TimeOnly startTime, TimeOnly endTime, decimal? requestedHours)
        {
            if (requestedHours.HasValue && requestedHours.Value > 0)
                return requestedHours.Value;

            var duration = endTime - startTime;
            return Math.Round((decimal)duration.TotalHours, 2);
        }

        private static WorkLogResponse Map(WorkLog workLog) => new()
        {
            Id = workLog.Id,
            UserId = workLog.UserId,
            ProjectId = workLog.ProjectId,
            ProjectName = workLog.Project?.Name ?? string.Empty,
            WorkDate = workLog.WorkDate,
            WorkType = workLog.WorkType,
            StartTime = workLog.StartTime,
            EndTime = workLog.EndTime,
            WorkedHours = workLog.WorkedHours,
            TaskTitle = workLog.TaskTitle,
            Description = workLog.Description,
            ExpectedAmount = workLog.ExpectedAmount,
            ActualAmount = workLog.ActualAmount,
            Status = workLog.Status,
            ReferenceNumber = workLog.ReferenceNumber,
            AppliedDate = workLog.AppliedDate,
            ApprovedDate = workLog.ApprovedDate,
            PaidDate = workLog.PaidDate,
            PaymentMonth = workLog.PaymentMonth,
            Notes = workLog.Notes,
            IsActive = workLog.IsActive,
            CreatedAt = workLog.CreatedAt,
            UpdatedAt = workLog.UpdatedAt
        };

        private static WorkLogListResponse MapToList(WorkLog workLog) => new()
        {
            Id = workLog.Id,
            ProjectId = workLog.ProjectId,
            ProjectName = workLog.Project?.Name ?? string.Empty,
            WorkDate = workLog.WorkDate,
            WorkType = workLog.WorkType,
            WorkedHours = workLog.WorkedHours,
            TaskTitle = workLog.TaskTitle,
            ExpectedAmount = workLog.ExpectedAmount,
            ActualAmount = workLog.ActualAmount,
            Status = workLog.Status,
            CreatedAt = workLog.CreatedAt
        };
    }
}
