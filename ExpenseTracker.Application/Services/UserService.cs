using BCrypt.Net;
using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.DTOs.User;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Application.Repositories;
using ExpenseTracker.Domain.Entities;

namespace ExpenseTracker.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UserService(IUserRepository userRepository, IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<UserResponse>> CreateAsync(CreateUserRequest request)
        {
            var existingUser = await _userRepository.GetByEmailAsync(request.Email);

            if (existingUser is not null)
                return Result<UserResponse>.Failure("A user with this email already exists.");

            var user = new User
            {
                Id = Guid.NewGuid(),
                FullName = request.FullName,
                Email = request.Email,
                MobileNumber = request.MobileNumber,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
            };

            await _userRepository.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return Result<UserResponse>.Success(Map(user), "User created successfully.");
        }

        public async Task<Result<UserResponse>> GetByIdAsync(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);

            if (user is null)
                return Result<UserResponse>.Failure("User not found.");

            return Result<UserResponse>.Success(Map(user));
        }

        public async Task<Result<List<UserResponse>>> GetAllAsync()
        {
            var users = await _userRepository.GetAllAsync();

            return Result<List<UserResponse>>.Success(users.Select(Map).ToList());
        }

        public async Task<Result<UserResponse>> UpdateAsync(Guid id, UpdateUserRequest request)
        {
            var user = await _userRepository.GetByIdAsync(id);

            if (user is null)
                return Result<UserResponse>.Failure("User not found.");

            user.FullName = request.FullName;
            user.Email = request.Email;
            user.MobileNumber = request.MobileNumber;

            await _userRepository.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return Result<UserResponse>.Success(Map(user), "User updated successfully.");
        }

        public async Task<Result> UpdateDeviceTokenAsync(Guid userId, string deviceToken)
        {
            var user = await _userRepository.GetByIdAsync(userId);

            if (user is null)
                return Result.Failure("User not found.");

            user.DeviceToken = deviceToken;

            await _userRepository.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success("Device token updated.");
        }

        public async Task<Result> DeleteAsync(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);

            if (user is null)
                return Result.Failure("User not found.");

            await _userRepository.DeleteAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success("User deleted successfully.");
        }

        private static UserResponse Map(User user) => new()
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            MobileNumber = user.MobileNumber,
            CreatedAt = user.CreatedAt
        };
    }
}
