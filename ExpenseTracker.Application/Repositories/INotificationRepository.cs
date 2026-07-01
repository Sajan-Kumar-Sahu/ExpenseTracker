using ExpenseTracker.Domain.Entities;

namespace ExpenseTracker.Application.Repositories
{
    public interface INotificationRepository
    {
        Task<Notification?> GetByIdAsync(Guid id);

        Task<List<Notification>> GetByUserIdAsync(Guid userId);

        Task<List<Notification>> GetUnreadByUserIdAsync(Guid userId);

        Task<int> GetUnreadCountAsync(Guid userId);

        Task AddAsync(Notification notification);

        Task UpdateAsync(Notification notification);

        Task DeleteAsync(Notification notification);

        Task MarkAllReadAsync(Guid userId);
    }
}
