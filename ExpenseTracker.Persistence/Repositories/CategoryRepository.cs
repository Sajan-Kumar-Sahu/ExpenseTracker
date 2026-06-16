using ExpenseTracker.Application.Repositories;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Enums;
using ExpenseTracker.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExpenseTracker.Persistence.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly ExpenseTrackerDbContext _context;

        public CategoryRepository(ExpenseTrackerDbContext context)
        {
            _context = context;
        }

        public async Task<Category?> GetByIdAsync(Guid id)
        {
            return await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<List<Category>> GetAllAsync()
        {
            return await _context.Categories
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<Category>> GetByUserIdAsync(Guid userId)
        {
            return await _context.Categories
                .Where(c => c.UserId == userId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<bool> ExistsAsync(
            Guid userId,
            string name,
            CategoryType categoryType)
        {
            return await _context.Categories.AnyAsync(c =>
                c.UserId == userId &&
                c.CategoryType == categoryType &&
                c.Name.ToLower() == name.ToLower());
        }

        public async Task AddAsync(Category category)
        {
            await _context.Categories.AddAsync(category);
        }

        public Task UpdateAsync(Category category)
        {
            _context.Categories.Update(category);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Category category)
        {
            _context.Categories.Remove(category);
            return Task.CompletedTask;
        }
    }
}
