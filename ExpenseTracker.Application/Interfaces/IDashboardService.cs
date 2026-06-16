using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.DTOs.Dashboard;

namespace ExpenseTracker.Application.Interfaces
{
    public interface IDashboardService
    {
        Task<Result<DashboardResponse>> GetDashboardAsync();
    }
}
