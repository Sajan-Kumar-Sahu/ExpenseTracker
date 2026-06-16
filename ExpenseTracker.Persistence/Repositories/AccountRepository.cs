using ExpenseTracker.Application.Repositories;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExpenseTracker.Persistence.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        private readonly ExpenseTrackerDbContext _context;

        public AccountRepository(ExpenseTrackerDbContext context)
        {
            _context = context;
        }

        public async Task<Account?> GetByIdAsync(Guid id)
        {
            return await _context.Accounts
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<List<Account>> GetAllAsync()
        {
            return await _context.Accounts
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<Account>> GetByUserIdAsync(Guid userId)
        {
            return await _context.Accounts
                .Where(a => a.UserId == userId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<bool> ExistsAsync(Guid userId, string accountName)
        {
            return await _context.Accounts
                .AnyAsync(a =>
                    a.UserId == userId &&
                    a.Name.ToLower() == accountName.ToLower());
        }

        public async Task AddAsync(Account account)
        {
            await _context.Accounts.AddAsync(account);
        }

        public Task UpdateAsync(Account account)
        {
            _context.Accounts.Update(account);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Account account)
        {
            _context.Accounts.Remove(account);
            return Task.CompletedTask;
        }
    }
}
