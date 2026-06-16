using ExpenseTracker.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExpenseTracker.Application.Repositories
{
    public interface IAccountRepository
    {
        Task<Account?> GetByIdAsync(Guid id);

        Task<List<Account>> GetAllAsync();

        Task<List<Account>> GetByUserIdAsync(Guid userId);

        Task<bool> ExistsAsync(Guid userId, string accountName);

        Task AddAsync(Account account);

        Task UpdateAsync(Account account);

        Task DeleteAsync(Account account);
    }
}
