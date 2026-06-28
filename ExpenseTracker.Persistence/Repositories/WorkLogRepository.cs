using ExpenseTracker.Application.Repositories;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Enums;
using ExpenseTracker.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Persistence.Repositories
{
    public class WorkLogRepository : IWorkLogRepository
    {
        private readonly ExpenseTrackerDbContext _context;

        public WorkLogRepository(ExpenseTrackerDbContext context)
        {
            _context = context;
        }

        public async Task<WorkLog?> GetByIdAsync(Guid id)
        {
            return await _context.WorkLogs
                .Include(w => w.Project)
                .FirstOrDefaultAsync(w => w.Id == id);
        }

        public async Task<List<WorkLog>> GetByUserIdAsync(Guid userId)
        {
            return await _context.WorkLogs
                .Include(w => w.Project)
                .Where(w => w.UserId == userId)
                .AsNoTracking()
                .OrderByDescending(w => w.WorkDate)
                .ToListAsync();
        }

        public async Task<List<WorkLog>> GetByStatusAsync(Guid userId, WorkLogStatus status)
        {
            return await _context.WorkLogs
                .Include(w => w.Project)
                .Where(w => w.UserId == userId && w.Status == status)
                .AsNoTracking()
                .OrderByDescending(w => w.WorkDate)
                .ToListAsync();
        }

        public async Task<List<WorkLog>> GetByWorkTypeAsync(Guid userId, WorkType workType)
        {
            return await _context.WorkLogs
                .Include(w => w.Project)
                .Where(w => w.UserId == userId && w.WorkType == workType)
                .AsNoTracking()
                .OrderByDescending(w => w.WorkDate)
                .ToListAsync();
        }

        public async Task<List<WorkLog>> GetByProjectIdAsync(Guid projectId, Guid userId)
        {
            return await _context.WorkLogs
                .Include(w => w.Project)
                .Where(w => w.ProjectId == projectId && w.UserId == userId)
                .AsNoTracking()
                .OrderByDescending(w => w.WorkDate)
                .ToListAsync();
        }

        public async Task<List<WorkLog>> GetByMonthAsync(Guid userId, int year, int month)
        {
            var startDate = new DateOnly(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            return await _context.WorkLogs
                .Include(w => w.Project)
                .Where(w => w.UserId == userId && w.WorkDate >= startDate && w.WorkDate <= endDate)
                .AsNoTracking()
                .OrderByDescending(w => w.WorkDate)
                .ToListAsync();
        }

        public async Task<List<WorkLog>> GetPendingAsync(Guid userId)
        {
            return await _context.WorkLogs
                .Include(w => w.Project)
                .Where(w => w.UserId == userId && w.Status != WorkLogStatus.Paid)
                .AsNoTracking()
                .OrderByDescending(w => w.WorkDate)
                .ToListAsync();
        }

        public async Task<List<WorkLog>> GetAppliedNotPaidAsync(Guid userId)
        {
            return await _context.WorkLogs
                .Include(w => w.Project)
                .Where(w => w.UserId == userId &&
                            (w.Status == WorkLogStatus.Applied || w.Status == WorkLogStatus.Approved))
                .AsNoTracking()
                .OrderByDescending(w => w.WorkDate)
                .ToListAsync();
        }

        public async Task AddAsync(WorkLog workLog)
        {
            await _context.WorkLogs.AddAsync(workLog);
        }

        public Task UpdateAsync(WorkLog workLog)
        {
            _context.WorkLogs.Update(workLog);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(WorkLog workLog)
        {
            _context.WorkLogs.Remove(workLog);
            return Task.CompletedTask;
        }
    }
}
