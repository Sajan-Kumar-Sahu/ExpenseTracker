using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.DTOs.Account;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Application.Repositories;
using ExpenseTracker.Domain.Entities;

namespace ExpenseTracker.Application.Services
{
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IUnitOfWork _unitOfWork;

        public AccountService(
            IAccountRepository accountRepository,
            ITransactionRepository transactionRepository,
            IUnitOfWork unitOfWork)
        {
            _accountRepository = accountRepository;
            _transactionRepository = transactionRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<AccountResponse>> CreateAsync(Guid userId, CreateAccountRequest request)
        {
            var accountExists = await _accountRepository.ExistsAsync(userId, request.Name);

            if (accountExists)
                return Result<AccountResponse>.Failure("An account with this name already exists.");

            var account = new Account
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = request.Name,
                Description = request.Description,
                OpeningBalance = request.OpeningBalance,
                AccountType = request.AccountType,
                IsActive = true
            };

            await _accountRepository.AddAsync(account);
            await _unitOfWork.SaveChangesAsync();

            return Result<AccountResponse>.Success(Map(account, account.OpeningBalance), "Account created successfully.");
        }

        public async Task<Result<AccountResponse>> GetByIdAsync(Guid id, Guid userId)
        {
            var account = await _accountRepository.GetByIdAsync(id);

            if (account is null)
                return Result<AccountResponse>.Failure("Account not found.");

            if (account.UserId != userId)
                return Result<AccountResponse>.Failure("Access denied.");

            var currentBalance = await _transactionRepository.GetCurrentBalanceByAccountIdAsync(account.Id, account.OpeningBalance);
            return Result<AccountResponse>.Success(Map(account, currentBalance));
        }

        public async Task<Result<List<AccountResponse>>> GetAllByUserAsync(Guid userId)
        {
            var accounts = await _accountRepository.GetByUserIdAsync(userId);

            var responses = new List<AccountResponse>();
            foreach (var account in accounts)
            {
                var currentBalance = await _transactionRepository.GetCurrentBalanceByAccountIdAsync(account.Id, account.OpeningBalance);
                responses.Add(Map(account, currentBalance));
            }

            return Result<List<AccountResponse>>.Success(responses);
        }

        public async Task<Result<AccountResponse>> UpdateAsync(Guid userId, UpdateAccountRequest request)
        {
            var account = await _accountRepository.GetByIdAsync(request.Id);

            if (account is null)
                return Result<AccountResponse>.Failure("Account not found.");

            if (account.UserId != userId)
                return Result<AccountResponse>.Failure("Access denied.");

            account.Name = request.Name;
            account.Description = request.Description;
            account.AccountType = request.AccountType;
            account.IsActive = request.IsActive;

            await _accountRepository.UpdateAsync(account);
            await _unitOfWork.SaveChangesAsync();

            var currentBalance = await _transactionRepository.GetCurrentBalanceByAccountIdAsync(account.Id, account.OpeningBalance);
            return Result<AccountResponse>.Success(Map(account, currentBalance), "Account updated successfully.");
        }

        public async Task<Result> DeleteAsync(Guid id, Guid userId)
        {
            var account = await _accountRepository.GetByIdAsync(id);

            if (account is null)
                return Result.Failure("Account not found.");

            if (account.UserId != userId)
                return Result.Failure("Access denied.");

            await _accountRepository.DeleteAsync(account);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success("Account deleted successfully.");
        }

        private static AccountResponse Map(Account account, decimal currentBalance) => new()
        {
            Id = account.Id,
            UserId = account.UserId,
            Name = account.Name,
            Description = account.Description,
            OpeningBalance = account.OpeningBalance,
            CurrentBalance = currentBalance,
            IsActive = account.IsActive,
            AccountType = account.AccountType,
            CreatedAt = account.CreatedAt
        };
    }
}
