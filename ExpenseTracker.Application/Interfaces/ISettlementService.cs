using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.DTOs.Settlement;

namespace ExpenseTracker.Application.Interfaces
{
    public interface ISettlementService
    {
        Task<Result<SettlementResponse>> CreateAsync(Guid userId, CreateSettlementRequest request);

        Task<Result<SettlementResponse>> GetByIdAsync(Guid id, Guid userId);

        Task<Result<List<SettlementListResponse>>> GetAllByUserAsync(Guid userId);

        Task<Result<List<SettlementListResponse>>> GetReceivablesAsync(Guid userId);

        Task<Result<List<SettlementListResponse>>> GetPayablesAsync(Guid userId);

        Task<Result<SettlementSummaryResponse>> GetSummaryAsync(Guid userId);

        Task<Result<SettlementResponse>> UpdateAsync(Guid id, Guid userId, UpdateSettlementRequest request);

        Task<Result> DeleteAsync(Guid id, Guid userId);

        Task<Result<SettlementResponse>> ReceivePaymentAsync(Guid id, Guid userId, SettlementPaymentRequest request);

        Task<Result<SettlementResponse>> PayAsync(Guid id, Guid userId, SettlementPaymentRequest request);

        Task<Result<SettlementResponse>> CancelAsync(Guid id, Guid userId);
    }
}
