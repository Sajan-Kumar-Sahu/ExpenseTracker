namespace ExpenseTracker.Application.DTOs.Project
{
    public class UpdateProjectRequest
    {
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public bool IsActive { get; set; }
    }
}
