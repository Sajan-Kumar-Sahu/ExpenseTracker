using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.DTOs.Account;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExpenseTracker.Application.Interfaces
{
    public interface IAccountService
    {
        Task<Result<AccountResponse>> CreateAsync(CreateAccountRequest request);

        Task<Result<AccountResponse>> GetByIdAsync(Guid id);

        Task<Result<List<AccountResponse>>> GetAllAsync();

        Task<Result<AccountResponse>> UpdateAsync(UpdateAccountRequest request);

        Task<Result> DeleteAsync(Guid id);
    }
}
