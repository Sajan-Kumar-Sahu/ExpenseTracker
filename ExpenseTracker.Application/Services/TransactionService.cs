using ExpenseTracker.Application.Common;
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
        private readonly IUserRepository _userRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IUnitOfWork _unitOfWork;

        public TransactionService(
            ITransactionRepository transactionRepository,
            IUserRepository userRepository,
            IAccountRepository accountRepository,
            ICategoryRepository categoryRepository,
            IUnitOfWork unitOfWork)
        {
            _transactionRepository = transactionRepository;
            _userRepository = userRepository;
            _accountRepository = accountRepository;
            _categoryRepository = categoryRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<TransactionResponse>> CreateAsync(
            CreateTransactionRequest request)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId);

            if (user is null)
                return Result<TransactionResponse>
                    .Failure("User not found.");

            var account = await _accountRepository
                .GetByIdAsync(request.AccountId);

            if (account is null)
                return Result<TransactionResponse>
                    .Failure("Account not found.");

            if (request.Amount <= 0)
                return Result<TransactionResponse>
                    .Failure("Amount must be greater than zero.");

            if (request.TransactionType == EntryType.Transfer)
            {
                if (!request.TransferAccountId.HasValue)
                    return Result<TransactionResponse>
                        .Failure("Transfer account is required.");

                if (request.TransferAccountId == request.AccountId)
                    return Result<TransactionResponse>
                        .Failure("Source and destination accounts cannot be the same.");

                var transferAccount = await _accountRepository
                    .GetByIdAsync(request.TransferAccountId.Value);

                if (transferAccount is null)
                    return Result<TransactionResponse>
                        .Failure("Transfer account not found.");
            }
            else
            {
                if (!request.CategoryId.HasValue)
                    return Result<TransactionResponse>
                        .Failure("Category is required.");

                var category = await _categoryRepository
                    .GetByIdAsync(request.CategoryId.Value);

                if (category is null)
                    return Result<TransactionResponse>
                        .Failure("Category not found.");
            }

            var transaction = new FinancialTransaction
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
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

            return Result<TransactionResponse>.Success(
                Map(transaction),
                "Transaction created successfully.");
        }

        public async Task<Result<TransactionResponse>> GetByIdAsync(Guid id)
        {
            var transaction = await _transactionRepository
                .GetByIdAsync(id);

            if (transaction is null)
                return Result<TransactionResponse>
                    .Failure("Transaction not found.");

            return Result<TransactionResponse>
                .Success(Map(transaction));
        }

        public async Task<Result<List<TransactionResponse>>> GetAllAsync()
        {
            var transactions = await _transactionRepository
                .GetAllAsync();

            return Result<List<TransactionResponse>>
                .Success(transactions.Select(Map).ToList());
        }

        public async Task<Result<List<TransactionResponse>>> GetByUserIdAsync(
            Guid userId)
        {
            var transactions = await _transactionRepository
                .GetByUserIdAsync(userId);

            return Result<List<TransactionResponse>>
                .Success(transactions.Select(Map).ToList());
        }

        public async Task<Result<TransactionResponse>> UpdateAsync(
            Guid id,
            UpdateTransactionRequest request)
        {
            var transaction = await _transactionRepository
                .GetByIdAsync(id);

            if (transaction is null)
                return Result<TransactionResponse>
                    .Failure("Transaction not found.");

            var account = await _accountRepository
                .GetByIdAsync(request.AccountId);

            if (account is null)
                return Result<TransactionResponse>
                    .Failure("Account not found.");

            if (request.Amount <= 0)
                return Result<TransactionResponse>
                    .Failure("Amount must be greater than zero.");

            if (transaction.TransactionType == EntryType.Transfer)
            {
                if (!request.TransferAccountId.HasValue)
                    return Result<TransactionResponse>
                        .Failure("Transfer account is required.");

                if (request.TransferAccountId == request.AccountId)
                    return Result<TransactionResponse>
                        .Failure("Source and destination accounts cannot be the same.");
            }
            else
            {
                if (!request.CategoryId.HasValue)
                    return Result<TransactionResponse>
                        .Failure("Category is required.");
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

            return Result<TransactionResponse>.Success(
                Map(transaction),
                "Transaction updated successfully.");
        }

        public async Task<Result> DeleteAsync(Guid id)
        {
            var transaction = await _transactionRepository
                .GetByIdAsync(id);

            if (transaction is null)
                return Result.Failure("Transaction not found.");

            await _transactionRepository.DeleteAsync(transaction);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success("Transaction deleted successfully.");
        }

        private static TransactionResponse Map(
            FinancialTransaction transaction)
        {
            return new TransactionResponse
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
}
