using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Application.DTOs.Contact
{
    public class CreateContactRequest
    {
        public string Name { get; set; } = string.Empty;

        public string? MobileNumber { get; set; }

        public string? Email { get; set; }

        public ContactType ContactType { get; set; }

        public string? Notes { get; set; }
    }
}
