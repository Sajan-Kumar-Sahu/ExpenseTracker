using BCrypt.Net;
using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.DTOs.Auth;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Application.Repositories;
using Microsoft.Extensions.Configuration;

namespace ExpenseTracker.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenService _tokenService;
        private readonly int _accessTokenExpiryMinutes;
        private readonly int _refreshTokenExpiryDays;

        public AuthService(
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            ITokenService tokenService,
            IConfiguration configuration)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _tokenService = tokenService;
            _accessTokenExpiryMinutes = configuration.GetValue<int>("JwtSettings:AccessTokenExpiryMinutes", 15);
            _refreshTokenExpiryDays = configuration.GetValue<int>("JwtSettings:RefreshTokenExpiryDays", 7);
        }

        public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email);

            if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return Result<AuthResponse>.Failure("Invalid email or password.");

            if (!user.IsActive)
                return Result<AuthResponse>.Failure("Account is inactive.");

            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(_refreshTokenExpiryDays);

            await _userRepository.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return Result<AuthResponse>.Success(new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_accessTokenExpiryMinutes),
                FullName = user.FullName,
                Email = user.Email
            }, "Login successful.");
        }

        public async Task<Result<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request)
        {
            var user = await _userRepository.GetByRefreshTokenAsync(request.RefreshToken);

            if (user is null || user.RefreshTokenExpiry <= DateTime.UtcNow)
                return Result<AuthResponse>.Failure("Invalid or expired refresh token.");

            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(_refreshTokenExpiryDays);

            await _userRepository.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return Result<AuthResponse>.Success(new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_accessTokenExpiryMinutes),
                FullName = user.FullName,
                Email = user.Email
            }, "Token refreshed successfully.");
        }

        public async Task<Result> LogoutAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);

            if (user is null)
                return Result.Failure("User not found.");

            user.RefreshToken = null;
            user.RefreshTokenExpiry = null;

            await _userRepository.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success("Logged out successfully.");
        }
    }
}
