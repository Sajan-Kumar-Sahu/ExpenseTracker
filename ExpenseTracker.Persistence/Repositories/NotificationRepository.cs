using ExpenseTracker.Application.Repositories;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Persistence.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly ExpenseTrackerDbContext _context;

        public NotificationRepository(ExpenseTrackerDbContext context)
        {
            _context = context;
        }

        public async Task<Notification?> GetByIdAsync(Guid id)
        {
            return await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == id);
        }

        public async Task<List<Notification>> GetByUserIdAsync(Guid userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.SentAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<Notification>> GetUnreadByUserIdAsync(Guid userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .OrderByDescending(n => n.SentAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(Guid userId)
        {
            return await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        public async Task AddAsync(Notification notification)
        {
            await _context.Notifications.AddAsync(notification);
        }

        public Task UpdateAsync(Notification notification)
        {
            _context.Notifications.Update(notification);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Notification notification)
        {
            _context.Notifications.Remove(notification);
            return Task.CompletedTask;
        }

        public async Task MarkAllReadAsync(Guid userId)
        {
            await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(n => n.IsRead, true)
                    .SetProperty(n => n.ReadAt, DateTimeOffset.UtcNow));
        }
    }
}
