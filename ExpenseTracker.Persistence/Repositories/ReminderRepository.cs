using ExpenseTracker.Application.Repositories;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Enums;
using ExpenseTracker.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Persistence.Repositories
{
    public class ReminderRepository : IReminderRepository
    {
        private readonly ExpenseTrackerDbContext _context;

        public ReminderRepository(ExpenseTrackerDbContext context)
        {
            _context = context;
        }

        public async Task<Reminder?> GetByIdAsync(Guid id)
        {
            return await _context.Reminders
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<List<Reminder>> GetByUserIdAsync(Guid userId)
        {
            return await _context.Reminders
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.ScheduledDate)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<Reminder>> GetPendingByUserIdAsync(Guid userId)
        {
            return await _context.Reminders
                .Where(r => r.UserId == userId && r.Status == ReminderStatus.Pending)
                .OrderBy(r => r.ScheduledDate)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<Reminder>> GetTodayByUserIdAsync(Guid userId)
        {
            var todayStart = new DateTimeOffset(
             DateTime.UtcNow.Date,
             TimeSpan.Zero);

            var todayEnd = todayStart.AddDays(1);

            return await _context.Reminders
                .Where(r =>
                    r.UserId == userId &&
                    r.Status == ReminderStatus.Pending &&
                    r.ScheduledDate >= todayStart &&
                    r.ScheduledDate < todayEnd)
                .OrderBy(r => r.ScheduledDate)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<Reminder>> GetUpcomingByUserIdAsync(Guid userId, int days = 7)
        {
            var now = DateTimeOffset.UtcNow;
            var cutoff = now.AddDays(days);

            return await _context.Reminders
                .Where(r =>
                    r.UserId == userId &&
                    r.Status == ReminderStatus.Pending &&
                    r.ScheduledDate >= now &&
                    r.ScheduledDate <= cutoff)
                .OrderBy(r => r.ScheduledDate)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<Reminder>> GetOverdueByUserIdAsync(Guid userId)
        {
            var now = DateTimeOffset.UtcNow;

            return await _context.Reminders
                .Where(r =>
                    r.UserId == userId &&
                    r.Status == ReminderStatus.Pending &&
                    r.ScheduledDate < now)
                .OrderBy(r => r.ScheduledDate)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<Reminder>> GetDueRemindersAsync()
        {
            var now = DateTimeOffset.UtcNow;

            return await _context.Reminders
                .Where(r =>
                    r.Status == ReminderStatus.Pending &&
                    r.IsActive &&
                    r.ScheduledDate <= now &&
                    (r.ExpiresAt == null || r.ExpiresAt > now) &&
                    (r.LastTriggeredAt == null || r.NextTriggerAt == null || r.NextTriggerAt <= now))
                .ToListAsync();
        }

        public async Task<List<Reminder>> GetExpiredRemindersAsync()
        {
            var now = DateTimeOffset.UtcNow;

            return await _context.Reminders
                .Where(r =>
                    r.Status == ReminderStatus.Pending &&
                    r.ExpiresAt != null &&
                    r.ExpiresAt <= now)
                .ToListAsync();
        }

        public async Task<Reminder?> GetActiveReminderByReferenceAsync(
            Guid userId, ReferenceModule module, Guid referenceId, ReminderType type)
        {
            return await _context.Reminders
                .FirstOrDefaultAsync(r =>
                    r.UserId == userId &&
                    r.ReferenceModule == module &&
                    r.ReferenceId == referenceId &&
                    r.ReminderType == type &&
                    r.Status == ReminderStatus.Pending);
        }

        public async Task<bool> HasActiveReminderAsync(
            Guid userId, ReferenceModule module, Guid referenceId, ReminderType type)
        {
            return await _context.Reminders
                .AnyAsync(r =>
                    r.UserId == userId &&
                    r.ReferenceModule == module &&
                    r.ReferenceId == referenceId &&
                    r.ReminderType == type &&
                    r.Status == ReminderStatus.Pending);
        }

        public async Task AddAsync(Reminder reminder)
        {
            await _context.Reminders.AddAsync(reminder);
        }

        public Task UpdateAsync(Reminder reminder)
        {
            _context.Reminders.Update(reminder);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Reminder reminder)
        {
            _context.Reminders.Remove(reminder);
            return Task.CompletedTask;
        }
    }
}
