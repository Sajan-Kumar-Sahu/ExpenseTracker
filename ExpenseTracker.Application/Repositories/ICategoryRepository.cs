using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExpenseTracker.Application.Repositories
{
    public interface ICategoryRepository
    {
        Task<Category?> GetByIdAsync(Guid id);

        Task<List<Category>> GetAllAsync();

        Task<List<Category>> GetByUserIdAsync(Guid userId);

        Task<bool> ExistsAsync(
            Guid userId,
            string name,
            CategoryType categoryType);

        Task AddAsync(Category category);

        Task UpdateAsync(Category category);

        Task DeleteAsync(Category category);
    }
}
