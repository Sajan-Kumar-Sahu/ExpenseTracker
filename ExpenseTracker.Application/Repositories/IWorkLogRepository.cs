using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Application.Repositories
{
    public interface IWorkLogRepository
    {
        Task<WorkLog?> GetByIdAsync(Guid id);

        Task<List<WorkLog>> GetByUserIdAsync(Guid userId);

        Task<List<WorkLog>> GetByStatusAsync(Guid userId, WorkLogStatus status);

        Task<List<WorkLog>> GetByWorkTypeAsync(Guid userId, WorkType workType);

        Task<List<WorkLog>> GetByProjectIdAsync(Guid projectId, Guid userId);

        Task<List<WorkLog>> GetByMonthAsync(Guid userId, int year, int month);

        Task<List<WorkLog>> GetPendingAsync(Guid userId);

        Task<List<WorkLog>> GetAppliedNotPaidAsync(Guid userId);

        Task AddAsync(WorkLog workLog);

        Task UpdateAsync(WorkLog workLog);

        Task DeleteAsync(WorkLog workLog);
    }
}
