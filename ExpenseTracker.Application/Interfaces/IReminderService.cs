using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.DTOs.Reminder;

namespace ExpenseTracker.Application.Interfaces
{
    public interface IReminderService
    {
        Task<Result<ReminderResponse>> CreateAsync(Guid userId, CreateReminderRequest request);

        Task<Result<ReminderResponse>> GetByIdAsync(Guid id, Guid userId);

        Task<Result<List<ReminderListResponse>>> GetAllByUserAsync(Guid userId);

        Task<Result<List<ReminderListResponse>>> GetPendingAsync(Guid userId);

        Task<Result<List<ReminderListResponse>>> GetTodayAsync(Guid userId);

        Task<Result<List<ReminderListResponse>>> GetUpcomingAsync(Guid userId);

        Task<Result<List<ReminderListResponse>>> GetOverdueAsync(Guid userId);

        Task<Result<ReminderResponse>> UpdateAsync(Guid id, Guid userId, UpdateReminderRequest request);

        Task<Result> DeleteAsync(Guid id, Guid userId);

        Task<Result<ReminderDashboardResponse>> GetDashboardAsync(Guid userId);
    }
}
