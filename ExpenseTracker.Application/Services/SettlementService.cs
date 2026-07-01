using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.DTOs.Settlement;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Application.Repositories;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Application.Services
{
    public class SettlementService : ISettlementService
    {
        private readonly ISettlementRepository _settlementRepository;
        private readonly IContactRepository _contactRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IReminderRepository _reminderRepository;
        private readonly IUnitOfWork _unitOfWork;

        public SettlementService(
            ISettlementRepository settlementRepository,
            IContactRepository contactRepository,
            IAccountRepository accountRepository,
            ITransactionRepository transactionRepository,
            IReminderRepository reminderRepository,
            IUnitOfWork unitOfWork)
        {
            _settlementRepository = settlementRepository;
            _contactRepository = contactRepository;
            _accountRepository = accountRepository;
            _transactionRepository = transactionRepository;
            _reminderRepository = reminderRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<SettlementResponse>> CreateAsync(Guid userId, CreateSettlementRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Reason))
                return Result<SettlementResponse>.Failure("Reason is required.");

            if (request.Reason.Trim().Length > 500)
                return Result<SettlementResponse>.Failure("Reason cannot exceed 500 characters.");

            if (!Enum.IsDefined(typeof(SettlementType), request.SettlementType))
                return Result<SettlementResponse>.Failure("Invalid settlement type.");

            if (request.OriginalAmount <= 0)
                return Result<SettlementResponse>.Failure("Original amount must be greater than zero.");

            var contact = await _contactRepository.GetByIdAsync(request.ContactId);

            if (contact is null)
                return Result<SettlementResponse>.Failure("Contact not found.");

            if (contact.UserId != userId)
                return Result<SettlementResponse>.Failure("Contact does not belong to this user.");

            if (!contact.IsActive)
                return Result<SettlementResponse>.Failure("Contact is inactive.");

            var settlement = new Settlement
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ContactId = request.ContactId,
                SettlementType = request.SettlementType,
                Reason = request.Reason.Trim(),
                OriginalAmount = request.OriginalAmount,
                PendingAmount = request.OriginalAmount,
                Status = SettlementStatus.Pending,
                DueDate = request.DueDate,
                IsActive = true,
                Notes = request.Notes?.Trim()
            };

            await _settlementRepository.AddAsync(settlement);

            if (settlement.DueDate.HasValue)
            {
                var reminderType = settlement.SettlementType == SettlementType.Receivable
                    ? ReminderType.SettlementReceivable
                    : ReminderType.SettlementPayable;

                var reminder = new Reminder
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    ReminderType = reminderType,
                    ReferenceModule = ReferenceModule.Settlement,
                    ReferenceId = settlement.Id,
                    Title = settlement.SettlementType == SettlementType.Receivable
                        ? "Collect Settlement Payment"
                        : "Pay Settlement",
                    Message = settlement.SettlementType == SettlementType.Receivable
                        ? $"Remember to collect {settlement.OriginalAmount:F2} from {contact.Name}."
                        : $"Remember to pay {settlement.OriginalAmount:F2} to {contact.Name}.",
                    ScheduledDate = settlement.DueDate.Value,
                    Priority = ReminderPriority.High,
                    Status = ReminderStatus.Pending,
                    RepeatType = RepeatType.None,
                    IsPushNotificationEnabled = true,
                    IsInAppNotificationEnabled = true,
                    IsActive = true
                };

                await _reminderRepository.AddAsync(reminder);
            }

            await _unitOfWork.SaveChangesAsync();

            settlement.Contact = contact;

            return Result<SettlementResponse>.Success(Map(settlement), "Settlement created successfully.");
        }

        public async Task<Result<SettlementResponse>> GetByIdAsync(Guid id, Guid userId)
        {
            var settlement = await _settlementRepository.GetByIdAsync(id);

            if (settlement is null)
                return Result<SettlementResponse>.Failure("Settlement not found.");

            if (settlement.UserId != userId)
                return Result<SettlementResponse>.Failure("Access denied.");

            return Result<SettlementResponse>.Success(Map(settlement));
        }

        public async Task<Result<List<SettlementListResponse>>> GetAllByUserAsync(Guid userId)
        {
            var settlements = await _settlementRepository.GetByUserIdAsync(userId);
            return Result<List<SettlementListResponse>>.Success(settlements.Select(MapToList).ToList());
        }

        public async Task<Result<List<SettlementListResponse>>> GetReceivablesAsync(Guid userId)
        {
            var settlements = await _settlementRepository.GetReceivablesByUserIdAsync(userId);
            return Result<List<SettlementListResponse>>.Success(settlements.Select(MapToList).ToList());
        }

        public async Task<Result<List<SettlementListResponse>>> GetPayablesAsync(Guid userId)
        {
            var settlements = await _settlementRepository.GetPayablesByUserIdAsync(userId);
            return Result<List<SettlementListResponse>>.Success(settlements.Select(MapToList).ToList());
        }

        public async Task<Result<SettlementSummaryResponse>> GetSummaryAsync(Guid userId)
        {
            var settlements = await _settlementRepository.GetByUserIdAsync(userId);

            var summary = new SettlementSummaryResponse
            {
                TotalReceivable = settlements
                    .Where(s => s.SettlementType == SettlementType.Receivable &&
                                s.Status != SettlementStatus.Completed &&
                                s.Status != SettlementStatus.Cancelled)
                    .Sum(s => s.PendingAmount),

                TotalPayable = settlements
                    .Where(s => s.SettlementType == SettlementType.Payable &&
                                s.Status != SettlementStatus.Completed &&
                                s.Status != SettlementStatus.Cancelled)
                    .Sum(s => s.PendingAmount),

                PendingReceivableCount = settlements
                    .Count(s => s.SettlementType == SettlementType.Receivable &&
                                s.Status == SettlementStatus.Pending),

                PendingPayableCount = settlements
                    .Count(s => s.SettlementType == SettlementType.Payable &&
                                s.Status == SettlementStatus.Pending),

                CompletedSettlementCount = settlements
                    .Count(s => s.Status == SettlementStatus.Completed)
            };

            return Result<SettlementSummaryResponse>.Success(summary);
        }

        public async Task<Result<SettlementResponse>> UpdateAsync(Guid id, Guid userId, UpdateSettlementRequest request)
        {
            var settlement = await _settlementRepository.GetByIdAsync(id);

            if (settlement is null)
                return Result<SettlementResponse>.Failure("Settlement not found.");

            if (settlement.UserId != userId)
                return Result<SettlementResponse>.Failure("Access denied.");

            if (settlement.Status == SettlementStatus.Cancelled)
                return Result<SettlementResponse>.Failure("Cannot update a cancelled settlement.");

            if (string.IsNullOrWhiteSpace(request.Reason))
                return Result<SettlementResponse>.Failure("Reason is required.");

            if (request.Reason.Trim().Length > 500)
                return Result<SettlementResponse>.Failure("Reason cannot exceed 500 characters.");

            if (request.OriginalAmount <= 0)
                return Result<SettlementResponse>.Failure("Original amount must be greater than zero.");

            if (settlement.Status == SettlementStatus.Completed && request.OriginalAmount != settlement.OriginalAmount)
                return Result<SettlementResponse>.Failure("Cannot change the amount of a completed settlement.");

            var contact = await _contactRepository.GetByIdAsync(request.ContactId);

            if (contact is null)
                return Result<SettlementResponse>.Failure("Contact not found.");

            if (contact.UserId != userId)
                return Result<SettlementResponse>.Failure("Contact does not belong to this user.");

            var paidAmount = settlement.OriginalAmount - settlement.PendingAmount;

            if (request.OriginalAmount < paidAmount)
                return Result<SettlementResponse>.Failure(
                    $"Original amount cannot be less than the amount already settled ({paidAmount:F2}).");

            settlement.ContactId = request.ContactId;
            settlement.Reason = request.Reason.Trim();
            settlement.OriginalAmount = request.OriginalAmount;
            settlement.PendingAmount = request.OriginalAmount - paidAmount;
            settlement.DueDate = request.DueDate;
            settlement.Notes = request.Notes?.Trim();

            await _settlementRepository.UpdateAsync(settlement);
            await _unitOfWork.SaveChangesAsync();

            settlement.Contact = contact;

            return Result<SettlementResponse>.Success(Map(settlement), "Settlement updated successfully.");
        }

        public async Task<Result> DeleteAsync(Guid id, Guid userId)
        {
            var settlement = await _settlementRepository.GetByIdAsync(id);

            if (settlement is null)
                return Result.Failure("Settlement not found.");

            if (settlement.UserId != userId)
                return Result.Failure("Access denied.");

            var hasPaymentHistory = await _settlementRepository.HasPaymentHistoryAsync(id);

            if (hasPaymentHistory)
                return Result.Failure("Cannot delete a settlement that has payment history.");

            await _settlementRepository.DeleteAsync(settlement);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success("Settlement deleted successfully.");
        }

        public async Task<Result<SettlementResponse>> ReceivePaymentAsync(
            Guid id, Guid userId, SettlementPaymentRequest request)
        {
            var settlement = await _settlementRepository.GetByIdAsync(id);

            if (settlement is null)
                return Result<SettlementResponse>.Failure("Settlement not found.");

            if (settlement.UserId != userId)
                return Result<SettlementResponse>.Failure("Access denied.");

            if (settlement.SettlementType != SettlementType.Receivable)
                return Result<SettlementResponse>.Failure("This endpoint is only for receivable settlements.");

            if (settlement.Status == SettlementStatus.Completed)
                return Result<SettlementResponse>.Failure("Settlement is already completed.");

            if (settlement.Status == SettlementStatus.Cancelled)
                return Result<SettlementResponse>.Failure("Settlement has been cancelled.");

            if (request.Amount <= 0)
                return Result<SettlementResponse>.Failure("Amount must be greater than zero.");

            if (request.Amount > settlement.PendingAmount)
                return Result<SettlementResponse>.Failure(
                    $"Amount cannot exceed the pending amount ({settlement.PendingAmount:F2}).");

            if (request.TransactionDate == default)
                return Result<SettlementResponse>.Failure("Transaction date is required.");

            var account = await _accountRepository.GetByIdAsync(request.AccountId);

            if (account is null)
                return Result<SettlementResponse>.Failure("Account not found.");

            if (account.UserId != userId)
                return Result<SettlementResponse>.Failure("Account does not belong to this user.");

            if (!account.IsActive)
                return Result<SettlementResponse>.Failure("Account is inactive and cannot be used for transactions.");

            var transaction = new FinancialTransaction
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                AccountId = request.AccountId,
                SettlementId = settlement.Id,
                CategoryId = request.CategoryId,
                TransactionType = EntryType.Income,
                Amount = request.Amount,
                TransactionDate = request.TransactionDate,
                Party = settlement.Contact?.Name,
                Notes = request.Notes
            };

            settlement.PendingAmount -= request.Amount;
            settlement.Status = settlement.PendingAmount == 0
                ? SettlementStatus.Completed
                : SettlementStatus.Partial;

            if (settlement.Status == SettlementStatus.Completed)
            {
                var reminder = await _reminderRepository.GetActiveReminderByReferenceAsync(
                    settlement.UserId, ReferenceModule.Settlement,
                    settlement.Id, ReminderType.SettlementReceivable);

                if (reminder is not null)
                {
                    reminder.Status = ReminderStatus.Completed;
                    reminder.CompletedAt = DateTimeOffset.UtcNow;
                    await _reminderRepository.UpdateAsync(reminder);
                }
            }

            await _transactionRepository.AddAsync(transaction);
            await _settlementRepository.UpdateAsync(settlement);
            await _unitOfWork.SaveChangesAsync();

            return Result<SettlementResponse>.Success(Map(settlement), "Payment received successfully.");
        }

        public async Task<Result<SettlementResponse>> PayAsync(
            Guid id, Guid userId, SettlementPaymentRequest request)
        {
            var settlement = await _settlementRepository.GetByIdAsync(id);

            if (settlement is null)
                return Result<SettlementResponse>.Failure("Settlement not found.");

            if (settlement.UserId != userId)
                return Result<SettlementResponse>.Failure("Access denied.");

            if (settlement.SettlementType != SettlementType.Payable)
                return Result<SettlementResponse>.Failure("This endpoint is only for payable settlements.");

            if (settlement.Status == SettlementStatus.Completed)
                return Result<SettlementResponse>.Failure("Settlement is already completed.");

            if (settlement.Status == SettlementStatus.Cancelled)
                return Result<SettlementResponse>.Failure("Settlement has been cancelled.");

            if (request.Amount <= 0)
                return Result<SettlementResponse>.Failure("Amount must be greater than zero.");

            if (request.Amount > settlement.PendingAmount)
                return Result<SettlementResponse>.Failure(
                    $"Amount cannot exceed the pending amount ({settlement.PendingAmount:F2}).");

            if (request.TransactionDate == default)
                return Result<SettlementResponse>.Failure("Transaction date is required.");

            var account = await _accountRepository.GetByIdAsync(request.AccountId);

            if (account is null)
                return Result<SettlementResponse>.Failure("Account not found.");

            if (account.UserId != userId)
                return Result<SettlementResponse>.Failure("Account does not belong to this user.");

            if (!account.IsActive)
                return Result<SettlementResponse>.Failure("Account is inactive and cannot be used for transactions.");

            var transaction = new FinancialTransaction
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                AccountId = request.AccountId,
                SettlementId = settlement.Id,
                CategoryId = request.CategoryId,
                TransactionType = EntryType.Expense,
                Amount = request.Amount,
                TransactionDate = request.TransactionDate,
                Party = settlement.Contact?.Name,
                Notes = request.Notes
            };

            settlement.PendingAmount -= request.Amount;
            settlement.Status = settlement.PendingAmount == 0
                ? SettlementStatus.Completed
                : SettlementStatus.Partial;

            if (settlement.Status == SettlementStatus.Completed)
            {
                var reminder = await _reminderRepository.GetActiveReminderByReferenceAsync(
                    settlement.UserId, ReferenceModule.Settlement,
                    settlement.Id, ReminderType.SettlementPayable);

                if (reminder is not null)
                {
                    reminder.Status = ReminderStatus.Completed;
                    reminder.CompletedAt = DateTimeOffset.UtcNow;
                    await _reminderRepository.UpdateAsync(reminder);
                }
            }

            await _transactionRepository.AddAsync(transaction);
            await _settlementRepository.UpdateAsync(settlement);
            await _unitOfWork.SaveChangesAsync();

            return Result<SettlementResponse>.Success(Map(settlement), "Payment made successfully.");
        }

        public async Task<Result<SettlementResponse>> CancelAsync(Guid id, Guid userId)
        {
            var settlement = await _settlementRepository.GetByIdAsync(id);

            if (settlement is null)
                return Result<SettlementResponse>.Failure("Settlement not found.");

            if (settlement.UserId != userId)
                return Result<SettlementResponse>.Failure("Access denied.");

            if (settlement.Status == SettlementStatus.Completed)
                return Result<SettlementResponse>.Failure("Cannot cancel a completed settlement.");

            if (settlement.Status == SettlementStatus.Cancelled)
                return Result<SettlementResponse>.Failure("Settlement is already cancelled.");

            settlement.Status = SettlementStatus.Cancelled;

            await _settlementRepository.UpdateAsync(settlement);
            await _unitOfWork.SaveChangesAsync();

            return Result<SettlementResponse>.Success(Map(settlement), "Settlement cancelled successfully.");
        }

        private static SettlementResponse Map(Settlement settlement) => new()
        {
            Id = settlement.Id,
            UserId = settlement.UserId,
            ContactId = settlement.ContactId,
            ContactName = settlement.Contact?.Name ?? string.Empty,
            SettlementType = settlement.SettlementType,
            Reason = settlement.Reason,
            OriginalAmount = settlement.OriginalAmount,
            PendingAmount = settlement.PendingAmount,
            Status = settlement.Status,
            DueDate = settlement.DueDate,
            IsActive = settlement.IsActive,
            Notes = settlement.Notes,
            CreatedAt = settlement.CreatedAt
        };

        private static SettlementListResponse MapToList(Settlement settlement) => new()
        {
            Id = settlement.Id,
            ContactId = settlement.ContactId,
            ContactName = settlement.Contact?.Name ?? string.Empty,
            SettlementType = settlement.SettlementType,
            Reason = settlement.Reason,
            OriginalAmount = settlement.OriginalAmount,
            PendingAmount = settlement.PendingAmount,
            Status = settlement.Status,
            DueDate = settlement.DueDate,
            CreatedAt = settlement.CreatedAt
        };
    }
}
