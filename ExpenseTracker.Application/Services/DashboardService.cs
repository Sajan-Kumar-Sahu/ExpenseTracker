using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.DTOs.Dashboard;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Application.Repositories;
using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Application.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IUserRepository _userRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ITransactionRepository _transactionRepository;

        public DashboardService(
            IUserRepository userRepository,
            IAccountRepository accountRepository,
            ICategoryRepository categoryRepository,
            ITransactionRepository transactionRepository)
        {
            _userRepository = userRepository;
            _accountRepository = accountRepository;
            _categoryRepository = categoryRepository;
            _transactionRepository = transactionRepository;
        }

        public async Task<Result<DashboardResponse>> GetDashboardAsync()
        {
            var user = await _userRepository.GetFirstAsync();

            if (user is null)
                return Result<DashboardResponse>
                    .Failure("No user found.");

            var userId = user.Id;

            var accounts = await _accountRepository.GetByUserIdAsync(userId);
            var categories = await _categoryRepository.GetByUserIdAsync(userId);
            var transactions = await _transactionRepository.GetByUserIdAsync(userId);

            var totalIncome = transactions
                .Where(t => t.TransactionType == EntryType.Income)
                .Sum(t => t.Amount);

            var totalSpent = transactions
                .Where(t => t.TransactionType == EntryType.Expense)
                .Sum(t => t.Amount);

            var totalSavings = totalIncome - totalSpent;

            var openingBalanceTotal = accounts.Sum(a => a.OpeningBalance);
            var totalBalance = openingBalanceTotal + totalIncome - totalSpent;

            var categoryMap = categories.ToDictionary(c => c.Id, c => c.Name);

            var expenseBreakdown = transactions
                .Where(t => t.TransactionType == EntryType.Expense && t.CategoryId.HasValue)
                .GroupBy(t => t.CategoryId!.Value)
                .Select(g => new CategoryBreakdownResponse
                {
                    CategoryId = g.Key,
                    CategoryName = categoryMap.TryGetValue(g.Key, out var name)
                        ? name
                        : "Unknown",
                    Amount = g.Sum(t => t.Amount),
                    Percentage = totalSpent > 0
                        ? Math.Round(g.Sum(t => t.Amount) / totalSpent * 100, 2)
                        : 0
                })
                .OrderByDescending(c => c.Amount)
                .ToList();

            return Result<DashboardResponse>.Success(new DashboardResponse
            {
                UserId = userId,
                TotalBalance = totalBalance,
                TotalIncome = totalIncome,
                TotalSpent = totalSpent,
                TotalSavings = totalSavings,
                ExpenseBreakdown = expenseBreakdown
            });
        }
    }
}
