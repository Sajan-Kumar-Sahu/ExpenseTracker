using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Application.Repositories
{
    public interface IReminderRepository
    {
        Task<Reminder?> GetByIdAsync(Guid id);

        Task<List<Reminder>> GetByUserIdAsync(Guid userId);

        Task<List<Reminder>> GetPendingByUserIdAsync(Guid userId);

        Task<List<Reminder>> GetTodayByUserIdAsync(Guid userId);

        Task<List<Reminder>> GetUpcomingByUserIdAsync(Guid userId, int days = 7);

        Task<List<Reminder>> GetOverdueByUserIdAsync(Guid userId);

        Task<List<Reminder>> GetDueRemindersAsync();

        Task<List<Reminder>> GetExpiredRemindersAsync();

        Task<Reminder?> GetActiveReminderByReferenceAsync(
            Guid userId, ReferenceModule module, Guid referenceId, ReminderType type);

        Task<bool> HasActiveReminderAsync(
            Guid userId, ReferenceModule module, Guid referenceId, ReminderType type);

        Task AddAsync(Reminder reminder);

        Task UpdateAsync(Reminder reminder);

        Task DeleteAsync(Reminder reminder);
    }
}
