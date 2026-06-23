using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.DTOs.Account;

namespace ExpenseTracker.Application.Interfaces
{
    public interface IAccountService
    {
        Task<Result<AccountResponse>> CreateAsync(Guid userId, CreateAccountRequest request);

        Task<Result<AccountResponse>> GetByIdAsync(Guid id, Guid userId);

        Task<Result<List<AccountResponse>>> GetAllByUserAsync(Guid userId);

        Task<Result<AccountResponse>> UpdateAsync(Guid userId, UpdateAccountRequest request);

        Task<Result> DeleteAsync(Guid id, Guid userId);
    }
}
