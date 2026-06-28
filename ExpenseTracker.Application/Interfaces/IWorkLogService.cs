using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.DTOs.WorkLog;
using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Application.Interfaces
{
    public interface IWorkLogService
    {
        Task<Result<WorkLogResponse>> CreateAsync(Guid userId, CreateWorkLogRequest request);

        Task<Result<WorkLogResponse>> GetByIdAsync(Guid id, Guid userId);

        Task<Result<List<WorkLogListResponse>>> GetAllByUserAsync(Guid userId);

        Task<Result<WorkLogResponse>> UpdateAsync(Guid id, Guid userId, UpdateWorkLogRequest request);

        Task<Result> DeleteAsync(Guid id, Guid userId);

        Task<Result<List<WorkLogListResponse>>> GetByStatusAsync(Guid userId, WorkLogStatus status);

        Task<Result<List<WorkLogListResponse>>> GetByWorkTypeAsync(Guid userId, WorkType workType);

        Task<Result<List<WorkLogListResponse>>> GetByProjectAsync(Guid projectId, Guid userId);

        Task<Result<List<WorkLogListResponse>>> GetByMonthAsync(Guid userId, int year, int month);

        Task<Result<List<WorkLogListResponse>>> GetPendingAsync(Guid userId);

        Task<Result<List<WorkLogListResponse>>> GetAppliedNotPaidAsync(Guid userId);

        Task<Result<WorkLogDashboardResponse>> GetDashboardAsync(Guid userId);

        Task<Result<WorkLogYearlySummaryResponse>> GetYearlySummaryAsync(Guid userId, int year);

        Task<Result<WorkLogResponse>> ApplyAsync(Guid id, Guid userId);

        Task<Result<WorkLogResponse>> ApproveAsync(Guid id, Guid userId);

        Task<Result<WorkLogResponse>> RejectAsync(Guid id, Guid userId);

        Task<Result<WorkLogResponse>> MarkPaidAsync(Guid id, Guid userId, MarkPaidRequest request);
    }
}
