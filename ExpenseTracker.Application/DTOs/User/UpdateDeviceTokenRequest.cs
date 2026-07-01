using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Application.DTOs.User
{
    public class UpdateDeviceTokenRequest
    {
        [Required]
        [MaxLength(512)]
        public string DeviceToken { get; set; } = string.Empty;
    }
}
