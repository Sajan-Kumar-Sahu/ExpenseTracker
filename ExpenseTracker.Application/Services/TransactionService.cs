using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.DTOs.Common;
using ExpenseTracker.Application.DTOs.FinancialTransaction;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Application.Repositories;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Application.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IUnitOfWork _unitOfWork;

        public TransactionService(
            ITransactionRepository transactionRepository,
            IAccountRepository accountRepository,
            ICategoryRepository categoryRepository,
            IUnitOfWork unitOfWork)
        {
            _transactionRepository = transactionRepository;
            _accountRepository = accountRepository;
            _categoryRepository = categoryRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<TransactionResponse>> CreateAsync(Guid userId, CreateTransactionRequest request)
        {
            var account = await _accountRepository.GetByIdAsync(request.AccountId);

            if (account is null)
                return Result<TransactionResponse>.Failure("Account not found.");

            if (account.UserId != userId)
                return Result<TransactionResponse>.Failure("Account does not belong to this user.");

            if (request.Amount <= 0)
                return Result<TransactionResponse>.Failure("Amount must be greater than zero.");

            if (request.TransactionType == EntryType.Transfer)
            {
                if (!request.TransferAccountId.HasValue)
                    return Result<TransactionResponse>.Failure("Transfer account is required.");

                if (request.TransferAccountId == request.AccountId)
                    return Result<TransactionResponse>.Failure("Source and destination accounts cannot be the same.");

                var transferAccount = await _accountRepository.GetByIdAsync(request.TransferAccountId.Value);

                if (transferAccount is null)
                    return Result<TransactionResponse>.Failure("Transfer account not found.");

                if (transferAccount.UserId != userId)
                    return Result<TransactionResponse>.Failure("Transfer account does not belong to this user.");
            }
            else
            {
                if (!request.CategoryId.HasValue)
                    return Result<TransactionResponse>.Failure("Category is required.");

                var category = await _categoryRepository.GetByIdAsync(request.CategoryId.Value);

                if (category is null)
                    return Result<TransactionResponse>.Failure("Category not found.");

                if (category.UserId != userId)
                    return Result<TransactionResponse>.Failure("Category does not belong to this user.");
            }

            var transaction = new FinancialTransaction
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                AccountId = request.AccountId,
                TransferAccountId = request.TransferAccountId,
                CategoryId = request.CategoryId,
                TransactionType = request.TransactionType,
                Amount = request.Amount,
                TransactionDate = request.TransactionDate,
                PaidTo = request.PaidTo,
                Notes = request.Notes
            };

            await _transactionRepository.AddAsync(transaction);
            await _unitOfWork.SaveChangesAsync();

            return Result<TransactionResponse>.Success(Map(transaction), "Transaction created successfully.");
        }

        public async Task<Result<TransactionResponse>> GetByIdAsync(Guid id, Guid userId)
        {
            var transaction = await _transactionRepository.GetByIdAsync(id);

            if (transaction is null)
                return Result<TransactionResponse>.Failure("Transaction not found.");

            if (transaction.UserId != userId)
                return Result<TransactionResponse>.Failure("Access denied.");

            return Result<TransactionResponse>.Success(Map(transaction));
        }

        public async Task<Result<PagedResult<TransactionResponse>>> GetAllByUserAsync(
            Guid userId, PaginationRequest pagination)
        {
            var (items, totalCount) = await _transactionRepository
                .GetByUserIdPagedAsync(userId, pagination.Page, pagination.PageSize);

            return Result<PagedResult<TransactionResponse>>.Success(new PagedResult<TransactionResponse>
            {
                Items = items.Select(Map).ToList(),
                TotalCount = totalCount,
                Page = pagination.Page,
                PageSize = pagination.PageSize
            });
        }

        public async Task<Result<TransactionResponse>> UpdateAsync(
            Guid id, Guid userId, UpdateTransactionRequest request)
        {
            var transaction = await _transactionRepository.GetByIdAsync(id);

            if (transaction is null)
                return Result<TransactionResponse>.Failure("Transaction not found.");

            if (transaction.UserId != userId)
                return Result<TransactionResponse>.Failure("Access denied.");

            var account = await _accountRepository.GetByIdAsync(request.AccountId);

            if (account is null)
                return Result<TransactionResponse>.Failure("Account not found.");

            if (account.UserId != userId)
                return Result<TransactionResponse>.Failure("Account does not belong to this user.");

            if (request.Amount <= 0)
                return Result<TransactionResponse>.Failure("Amount must be greater than zero.");

            if (transaction.TransactionType == EntryType.Transfer)
            {
                if (!request.TransferAccountId.HasValue)
                    return Result<TransactionResponse>.Failure("Transfer account is required.");

                if (request.TransferAccountId == request.AccountId)
                    return Result<TransactionResponse>.Failure("Source and destination accounts cannot be the same.");
            }
            else
            {
                if (!request.CategoryId.HasValue)
                    return Result<TransactionResponse>.Failure("Category is required.");
            }

            transaction.AccountId = request.AccountId;
            transaction.TransferAccountId = request.TransferAccountId;
            transaction.CategoryId = request.CategoryId;
            transaction.Amount = request.Amount;
            transaction.TransactionDate = request.TransactionDate;
            transaction.PaidTo = request.PaidTo;
            transaction.Notes = request.Notes;

            await _transactionRepository.UpdateAsync(transaction);
            await _unitOfWork.SaveChangesAsync();

            return Result<TransactionResponse>.Success(Map(transaction), "Transaction updated successfully.");
        }

        public async Task<Result> DeleteAsync(Guid id, Guid userId)
        {
            var transaction = await _transactionRepository.GetByIdAsync(id);

            if (transaction is null)
                return Result.Failure("Transaction not found.");

            if (transaction.UserId != userId)
                return Result.Failure("Access denied.");

            await _transactionRepository.DeleteAsync(transaction);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success("Transaction deleted successfully.");
        }

        private static TransactionResponse Map(FinancialTransaction transaction) => new()
        {
            Id = transaction.Id,
            UserId = transaction.UserId,
            AccountId = transaction.AccountId,
            TransferAccountId = transaction.TransferAccountId,
            CategoryId = transaction.CategoryId,
            TransactionType = transaction.TransactionType,
            Amount = transaction.Amount,
            TransactionDate = transaction.TransactionDate,
            PaidTo = transaction.PaidTo,
            Notes = transaction.Notes,
            CreatedAt = transaction.CreatedAt
        };
    }
}
