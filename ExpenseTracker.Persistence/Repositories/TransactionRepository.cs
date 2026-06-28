using ExpenseTracker.Application.Repositories;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Enums;
using ExpenseTracker.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Persistence.Repositories
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly ExpenseTrackerDbContext _context;

        public TransactionRepository(ExpenseTrackerDbContext context)
        {
            _context = context;
        }

        public async Task<FinancialTransaction?> GetByIdAsync(Guid id)
        {
            return await _context.FinancialTransactions
                .Include(t => t.Account)
                .Include(t => t.TransferAccount)
                .Include(t => t.Category)
                .Include(t => t.Settlement)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<List<FinancialTransaction>> GetAllAsync()
        {
            return await _context.FinancialTransactions
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<FinancialTransaction>> GetByUserIdAsync(Guid userId)
        {
            return await _context.FinancialTransactions
                .Where(t => t.UserId == userId)
                .AsNoTracking()
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();
        }

        public async Task<decimal> GetCurrentBalanceByAccountIdAsync(Guid accountId, decimal openingBalance)
        {
            var inflow = await _context.FinancialTransactions
                .Where(t => (t.AccountId == accountId && t.TransactionType == EntryType.Income) ||
                            (t.TransferAccountId == accountId && t.TransactionType == EntryType.Transfer))
                .SumAsync(t => (decimal?)t.Amount) ?? 0m;

            var outflow = await _context.FinancialTransactions
                .Where(t => t.AccountId == accountId &&
                            (t.TransactionType == EntryType.Expense || t.TransactionType == EntryType.Transfer))
                .SumAsync(t => (decimal?)t.Amount) ?? 0m;

            return openingBalance + inflow - outflow;
        }

        public async Task<(List<FinancialTransaction> Items, int TotalCount)> GetByUserIdPagedAsync(
            Guid userId, int page, int pageSize)
        {
            var query = _context.FinancialTransactions
                .Where(t => t.UserId == userId)
                .AsNoTracking()
                .OrderByDescending(t => t.TransactionDate);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task AddAsync(FinancialTransaction transaction)
        {
            await _context.FinancialTransactions.AddAsync(transaction);
        }

        public Task UpdateAsync(FinancialTransaction transaction)
        {
            _context.FinancialTransactions.Update(transaction);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(FinancialTransaction transaction)
        {
            _context.FinancialTransactions.Remove(transaction);
            return Task.CompletedTask;
        }
    }
}
