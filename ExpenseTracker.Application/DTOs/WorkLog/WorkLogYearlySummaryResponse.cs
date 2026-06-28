namespace ExpenseTracker.Application.DTOs.WorkLog
{
    public class WorkLogYearlySummaryResponse
    {
        public int Year { get; set; }

        public int TotalWorkedDays { get; set; }

        public decimal TotalWorkedHours { get; set; }

        public decimal TotalExpectedAmount { get; set; }

        public decimal TotalReceivedAmount { get; set; }

        public decimal PendingAmount { get; set; }

        public int WeekendCount { get; set; }

        public int HolidayCount { get; set; }

        public int OnCallCount { get; set; }

        public int LateNightCount { get; set; }

        public int ProductionSupportCount { get; set; }
    }
}
