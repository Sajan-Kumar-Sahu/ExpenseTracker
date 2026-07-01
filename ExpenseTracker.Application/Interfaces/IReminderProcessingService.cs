namespace ExpenseTracker.Application.Interfaces
{
    public interface IReminderProcessingService
    {
        Task ProcessDueRemindersAsync(CancellationToken cancellationToken = default);

        Task ProcessExpiredRemindersAsync(CancellationToken cancellationToken = default);
    }
}
