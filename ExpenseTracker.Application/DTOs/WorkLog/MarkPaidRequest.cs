namespace ExpenseTracker.Application.DTOs.WorkLog
{
    public class MarkPaidRequest
    {
        public decimal ActualAmount { get; set; }

        public string PaymentMonth { get; set; } = string.Empty;
    }
}
