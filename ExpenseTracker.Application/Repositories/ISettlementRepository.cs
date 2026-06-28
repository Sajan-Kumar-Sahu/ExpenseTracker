using ExpenseTracker.Domain.Entities;

namespace ExpenseTracker.Application.Repositories
{
    public interface ISettlementRepository
    {
        Task<Settlement?> GetByIdAsync(Guid id);

        Task<List<Settlement>> GetByUserIdAsync(Guid userId);

        Task<List<Settlement>> GetReceivablesByUserIdAsync(Guid userId);

        Task<List<Settlement>> GetPayablesByUserIdAsync(Guid userId);

        Task<bool> HasPaymentHistoryAsync(Guid settlementId);

        Task AddAsync(Settlement settlement);

        Task UpdateAsync(Settlement settlement);

        Task DeleteAsync(Settlement settlement);
    }
}
