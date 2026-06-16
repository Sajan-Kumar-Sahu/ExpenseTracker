using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.DTOs.Auth;

namespace ExpenseTracker.Application.Interfaces
{
    public interface IAuthService
    {
        Task<Result<AuthResponse>> LoginAsync(LoginRequest request);

        Task<Result<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request);

        Task<Result> LogoutAsync(Guid userId);
    }
}
