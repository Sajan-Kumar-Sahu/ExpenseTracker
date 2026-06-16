using ExpenseTracker.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExpenseTracker.Application.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid id);

        Task<User?> GetFirstAsync();

        Task<User?> GetByEmailAsync(string email);

        Task<User?> GetByRefreshTokenAsync(string refreshToken);

        Task<List<User>> GetAllAsync();

        Task AddAsync(User user);

        Task UpdateAsync(User user);

        Task DeleteAsync(User user);
    }
}
