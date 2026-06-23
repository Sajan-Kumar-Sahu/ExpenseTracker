using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.DTOs.Common;
using ExpenseTracker.Application.DTOs.FinancialTransaction;

namespace ExpenseTracker.Application.Interfaces
{
    public interface ITransactionService
    {
        Task<Result<TransactionResponse>> CreateAsync(Guid userId, CreateTransactionRequest request);

        Task<Result<TransactionResponse>> GetByIdAsync(Guid id, Guid userId);

        Task<Result<PagedResult<TransactionResponse>>> GetAllByUserAsync(Guid userId, PaginationRequest pagination);

        Task<Result<TransactionResponse>> UpdateAsync(Guid id, Guid userId, UpdateTransactionRequest request);

        Task<Result> DeleteAsync(Guid id, Guid userId);
    }
}
