using ExpenseTracker.Domain.Entities;

namespace ExpenseTracker.Application.Repositories
{
    public interface IProjectRepository
    {
        Task<Project?> GetByIdAsync(Guid id);

        Task<List<Project>> GetByUserIdAsync(Guid userId);

        Task<bool> ExistsAsync(Guid userId, string name);

        Task AddAsync(Project project);

        Task UpdateAsync(Project project);

        Task DeleteAsync(Project project);
    }
}
