using ExpenseTracker.Application.Repositories;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExpenseTracker.Persistence.Repositories
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly ExpenseTrackerDbContext _context;

        public TransactionRepository(
            ExpenseTrackerDbContext context)
        {
            _context = context;
        }

        public async Task<FinancialTransaction?> GetByIdAsync(Guid id)
        {
            return await _context.FinancialTransactions
                .Include(t => t.Account)
                .Include(t => t.TransferAccount)
                .Include(t => t.Category)
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

        public async Task AddAsync(FinancialTransaction transaction)
        {
            await _context.FinancialTransactions
                .AddAsync(transaction);
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
