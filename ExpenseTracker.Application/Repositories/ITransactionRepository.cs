using ExpenseTracker.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExpenseTracker.Application.Repositories
{
    public interface ITransactionRepository
    {
        Task<FinancialTransaction?> GetByIdAsync(Guid id);

        Task<List<FinancialTransaction>> GetAllAsync();

        Task<List<FinancialTransaction>> GetByUserIdAsync(Guid userId);

        Task AddAsync(FinancialTransaction transaction);

        Task UpdateAsync(FinancialTransaction transaction);

        Task DeleteAsync(FinancialTransaction transaction);
    }
}
