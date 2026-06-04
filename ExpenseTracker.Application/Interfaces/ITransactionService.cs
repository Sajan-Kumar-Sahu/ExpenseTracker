using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.DTOs.FinancialTransaction;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExpenseTracker.Application.Interfaces
{
    public interface ITransactionService
    {
        Task<Result<TransactionResponse>> CreateAsync(
            CreateTransactionRequest request);

        Task<Result<TransactionResponse>> GetByIdAsync(Guid id);

        Task<Result<List<TransactionResponse>>> GetAllAsync();

        Task<Result<List<TransactionResponse>>> GetByUserIdAsync(Guid userId);

        Task<Result<TransactionResponse>> UpdateAsync(
            Guid id,
            UpdateTransactionRequest request);

        Task<Result> DeleteAsync(Guid id);
    }
}
