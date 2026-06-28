using ExpenseTracker.Domain.Common;

namespace ExpenseTracker.Domain.Entities
{
    public class Project : AuditableEntity
    {
        public Guid UserId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public User User { get; set; } = null!;
        public ICollection<WorkLog> WorkLogs { get; set; } = new List<WorkLog>();
    }
}
