namespace ExpenseTracker.Application.DTOs.User
{
    public class UpdateUserRequest
    {
        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string MobileNumber { get; set; } = string.Empty;
    }
}
