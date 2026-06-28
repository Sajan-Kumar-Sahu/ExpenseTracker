using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Application.DTOs.Contact
{
    public class ContactResponse
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? MobileNumber { get; set; }

        public string? Email { get; set; }

        public ContactType ContactType { get; set; }

        public bool IsActive { get; set; }

        public string? Notes { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
    }
}
