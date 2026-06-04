using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.DTOs.Account;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Application.Repositories;
using ExpenseTracker.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExpenseTracker.Application.Services
{
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;

        public AccountService(
            IAccountRepository accountRepository,
            IUserRepository userRepository,
            IUnitOfWork unitOfWork)
        {
            _accountRepository = accountRepository;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<AccountResponse>> CreateAsync(CreateAccountRequest request)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId);

            if (user is null)
                return Result<AccountResponse>.Failure("User not found.");

            var accountExists = await _accountRepository
                .ExistsAsync(request.UserId, request.Name);

            if (accountExists)
                return Result<AccountResponse>.Failure(
                    "An account with this name already exists.");

            var account = new Account
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                Name = request.Name,
                Description = request.Description,
                OpeningBalance = request.OpeningBalance,
                AccountType = request.AccountType,
                IsActive = true
            };

            await _accountRepository.AddAsync(account);
            await _unitOfWork.SaveChangesAsync();

            return Result<AccountResponse>.Success(
                Map(account),
                "Account created successfully.");
        }

        public async Task<Result<AccountResponse>> GetByIdAsync(Guid id)
        {
            var account = await _accountRepository.GetByIdAsync(id);

            if (account is null)
                return Result<AccountResponse>.Failure("Account not found.");

            return Result<AccountResponse>.Success(Map(account));
        }

        public async Task<Result<List<AccountResponse>>> GetAllAsync()
        {
            var accounts = await _accountRepository.GetAllAsync();

            return Result<List<AccountResponse>>
                .Success(accounts.Select(Map).ToList());
        }

        public async Task<Result<AccountResponse>> UpdateAsync(
            Guid id,
            UpdateAccountRequest request)
        {
            var account = await _accountRepository.GetByIdAsync(id);

            if (account is null)
                return Result<AccountResponse>.Failure("Account not found.");

            account.Name = request.Name;
            account.Description = request.Description;
            account.AccountType = request.AccountType;
            account.IsActive = request.IsActive;

            await _accountRepository.UpdateAsync(account);
            await _unitOfWork.SaveChangesAsync();

            return Result<AccountResponse>.Success(
                Map(account),
                "Account updated successfully.");
        }

        public async Task<Result> DeleteAsync(Guid id)
        {
            var account = await _accountRepository.GetByIdAsync(id);

            if (account is null)
                return Result.Failure("Account not found.");

            await _accountRepository.DeleteAsync(account);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success("Account deleted successfully.");
        }

        private static AccountResponse Map(Account account)
        {
            return new AccountResponse
            {
                Id = account.Id,
                UserId = account.UserId,
                Name = account.Name,
                Description = account.Description,
                OpeningBalance = account.OpeningBalance,
                IsActive = account.IsActive,
                AccountType = account.AccountType,
                CreatedAt = account.CreatedAt
            };
        }
    }
}
