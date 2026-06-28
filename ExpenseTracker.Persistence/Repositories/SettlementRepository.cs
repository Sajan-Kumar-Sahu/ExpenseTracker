using ExpenseTracker.Application.Repositories;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Enums;
using ExpenseTracker.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Persistence.Repositories
{
    public class SettlementRepository : ISettlementRepository
    {
        private readonly ExpenseTrackerDbContext _context;

        public SettlementRepository(ExpenseTrackerDbContext context)
        {
            _context = context;
        }

        public async Task<Settlement?> GetByIdAsync(Guid id)
        {
            return await _context.Settlements
                .Include(s => s.Contact)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<List<Settlement>> GetByUserIdAsync(Guid userId)
        {
            return await _context.Settlements
                .Include(s => s.Contact)
                .Where(s => s.UserId == userId)
                .AsNoTracking()
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Settlement>> GetReceivablesByUserIdAsync(Guid userId)
        {
            return await _context.Settlements
                .Include(s => s.Contact)
                .Where(s => s.UserId == userId &&
                            s.SettlementType == SettlementType.Receivable &&
                            s.Status != SettlementStatus.Completed &&
                            s.Status != SettlementStatus.Cancelled)
                .AsNoTracking()
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Settlement>> GetPayablesByUserIdAsync(Guid userId)
        {
            return await _context.Settlements
                .Include(s => s.Contact)
                .Where(s => s.UserId == userId &&
                            s.SettlementType == SettlementType.Payable &&
                            s.Status != SettlementStatus.Completed &&
                            s.Status != SettlementStatus.Cancelled)
                .AsNoTracking()
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> HasPaymentHistoryAsync(Guid settlementId)
        {
            return await _context.FinancialTransactions
                .AnyAsync(t => t.SettlementId == settlementId);
        }

        public async Task AddAsync(Settlement settlement)
        {
            await _context.Settlements.AddAsync(settlement);
        }

        public Task UpdateAsync(Settlement settlement)
        {
            _context.Settlements.Update(settlement);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Settlement settlement)
        {
            _context.Settlements.Remove(settlement);
            return Task.CompletedTask;
        }
    }
}
