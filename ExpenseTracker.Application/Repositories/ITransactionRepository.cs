using ExpenseTracker.Domain.Entities;

namespace ExpenseTracker.Application.Repositories
{
    public interface ITransactionRepository
    {
        Task<FinancialTransaction?> GetByIdAsync(Guid id);

        Task<List<FinancialTransaction>> GetAllAsync();

        Task<List<FinancialTransaction>> GetByUserIdAsync(Guid userId);

        Task<(List<FinancialTransaction> Items, int TotalCount)> GetByUserIdPagedAsync(
            Guid userId, int page, int pageSize);

        Task AddAsync(FinancialTransaction transaction);

        Task UpdateAsync(FinancialTransaction transaction);

        Task DeleteAsync(FinancialTransaction transaction);
    }
}
