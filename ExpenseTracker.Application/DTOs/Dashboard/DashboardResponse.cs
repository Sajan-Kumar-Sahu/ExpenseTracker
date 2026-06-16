namespace ExpenseTracker.Application.DTOs.Dashboard
{
    public class DashboardResponse
    {
        public Guid UserId { get; set; }
        public decimal TotalBalance { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal TotalSpent { get; set; }
        public decimal TotalSavings { get; set; }
        public List<CategoryBreakdownResponse> ExpenseBreakdown { get; set; } = [];
    }

    public class CategoryBreakdownResponse
    {
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal Percentage { get; set; }
    }
}
