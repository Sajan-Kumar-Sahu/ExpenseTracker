using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.DTOs.User;

namespace ExpenseTracker.Application.Interfaces
{
    public interface IUserService
    {
        Task<Result<UserResponse>> CreateAsync(CreateUserRequest request);

        Task<Result<UserResponse>> GetByIdAsync(Guid id);

        Task<Result<List<UserResponse>>> GetAllAsync();

        Task<Result<UserResponse>> UpdateAsync(Guid id, UpdateUserRequest request);

        Task<Result> UpdateDeviceTokenAsync(Guid userId, string deviceToken);

        Task<Result> DeleteAsync(Guid id);
    }
}
