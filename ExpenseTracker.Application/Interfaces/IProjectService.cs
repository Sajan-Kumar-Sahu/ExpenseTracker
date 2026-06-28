using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.DTOs.Project;

namespace ExpenseTracker.Application.Interfaces
{
    public interface IProjectService
    {
        Task<Result<ProjectResponse>> CreateAsync(Guid userId, CreateProjectRequest request);

        Task<Result<ProjectResponse>> GetByIdAsync(Guid id, Guid userId);

        Task<Result<List<ProjectListResponse>>> GetAllByUserAsync(Guid userId);

        Task<Result<ProjectResponse>> UpdateAsync(Guid id, Guid userId, UpdateProjectRequest request);

        Task<Result> DeleteAsync(Guid id, Guid userId);
    }
}
