using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Persistence.Context;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExpenseTracker.Persistence.Common
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ExpenseTrackerDbContext _context;

        public UnitOfWork(ExpenseTrackerDbContext context)
        {
            _context = context;
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(
                cancellationToken);
        }
    }
}
