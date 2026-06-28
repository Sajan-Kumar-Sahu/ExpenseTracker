using ExpenseTracker.Domain.Common;
using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Domain.Entities
{
    public class Contact : AuditableEntity
    {
        public Guid UserId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? MobileNumber { get; set; }

        public string? Email { get; set; }

        public ContactType ContactType { get; set; }

        public bool IsActive { get; set; } = true;

        public string? Notes { get; set; }

        public User User { get; set; } = null!;
        public ICollection<Settlement> Settlements { get; set; } = new List<Settlement>();
    }
}
