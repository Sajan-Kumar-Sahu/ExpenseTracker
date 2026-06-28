using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Application.DTOs.Contact
{
    public class UpdateContactRequest
    {
        public string Name { get; set; } = string.Empty;

        public string? MobileNumber { get; set; }

        public string? Email { get; set; }

        public ContactType ContactType { get; set; }

        public bool IsActive { get; set; }

        public string? Notes { get; set; }
    }
}
