using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ExpenseTracker.API.Controllers
{
    public abstract class BaseController : ControllerBase
    {
        protected Guid? GetCurrentUserId()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub");

            if (claim is null || !Guid.TryParse(claim, out var userId))
                return null;

            return userId;
        }

        protected IActionResult UnauthorizedUser() =>
            Unauthorized(new { IsSuccess = false, Message = "Invalid or missing user identity." });
    }
}
