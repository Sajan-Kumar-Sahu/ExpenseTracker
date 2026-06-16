using ExpenseTracker.Domain.Entities;

namespace ExpenseTracker.Application.Interfaces
{
    public interface ITokenService
    {
        string GenerateAccessToken(User user);

        string GenerateRefreshToken();
    }
}
