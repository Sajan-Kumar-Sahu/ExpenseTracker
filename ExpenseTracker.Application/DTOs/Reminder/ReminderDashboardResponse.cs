namespace ExpenseTracker.Application.DTOs.Reminder
{
    public class ReminderDashboardResponse
    {
        public int TotalReminders { get; set; }

        public int PendingCount { get; set; }

        public int CompletedCount { get; set; }

        public int OverdueCount { get; set; }

        public int TodayCount { get; set; }

        public int UpcomingThisWeekCount { get; set; }

        public int CriticalCount { get; set; }
    }
}
