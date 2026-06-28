namespace ExpenseTracker.Application.DTOs.WorkLog
{
    public class WorkLogDashboardResponse
    {
        public int TotalEntries { get; set; }

        public int DraftCount { get; set; }

        public int AppliedCount { get; set; }

        public int ApprovedCount { get; set; }

        public int PaidCount { get; set; }

        public int RejectedCount { get; set; }

        public int CancelledCount { get; set; }

        public decimal TotalWorkedHours { get; set; }

        public decimal TotalExpectedAmount { get; set; }

        public decimal TotalReceivedAmount { get; set; }

        public decimal TotalPendingAmount { get; set; }

        public int TotalWeekendEntries { get; set; }

        public int TotalHolidayEntries { get; set; }

        public int TotalOnCallEntries { get; set; }

        public int TotalProductionSupportEntries { get; set; }
    }
}
