namespace ExpenseTracker.Application.DTOs.User
{
    public class UserResponse
    {
        public Guid Id { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string MobileNumber { get; set; } = string.Empty;

        public DateTimeOffset CreatedAt { get; set; }
    }
}