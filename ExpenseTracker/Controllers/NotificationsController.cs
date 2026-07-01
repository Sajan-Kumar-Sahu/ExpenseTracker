using ExpenseTracker.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationsController : BaseController
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var userId = GetCurrentUserId();
            if (userId is null) return UnauthorizedUser();

            var result = await _notificationService.GetAllByUserAsync(userId.Value);
            return Ok(result);
        }

        [HttpGet("unread")]
        public async Task<IActionResult> GetUnread()
        {
            var userId = GetCurrentUserId();
            if (userId is null) return UnauthorizedUser();

            var result = await _notificationService.GetUnreadAsync(userId.Value);
            return Ok(result);
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetHistory()
        {
            var userId = GetCurrentUserId();
            if (userId is null) return UnauthorizedUser();

            var result = await _notificationService.GetHistoryAsync(userId.Value);
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var userId = GetCurrentUserId();
            if (userId is null) return UnauthorizedUser();

            var result = await _notificationService.GetByIdAsync(id, userId.Value);

            if (!result.IsSuccess)
                return NotFound(result);

            return Ok(result);
        }

        [HttpPost("{id:guid}/mark-read")]
        public async Task<IActionResult> MarkRead(Guid id)
        {
            var userId = GetCurrentUserId();
            if (userId is null) return UnauthorizedUser();

            var result = await _notificationService.MarkReadAsync(id, userId.Value);

            if (!result.IsSuccess)
                return NotFound(result);

            return Ok(result);
        }

        [HttpPost("{id:guid}/mark-clicked")]
        public async Task<IActionResult> MarkClicked(Guid id)
        {
            var userId = GetCurrentUserId();
            if (userId is null) return UnauthorizedUser();

            var result = await _notificationService.MarkClickedAsync(id, userId.Value);

            if (!result.IsSuccess)
                return NotFound(result);

            return Ok(result);
        }

        [HttpPost("mark-all-read")]
        public async Task<IActionResult> MarkAllRead()
        {
            var userId = GetCurrentUserId();
            if (userId is null) return UnauthorizedUser();

            var result = await _notificationService.MarkAllReadAsync(userId.Value);
            return Ok(result);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userId = GetCurrentUserId();
            if (userId is null) return UnauthorizedUser();

            var result = await _notificationService.DeleteAsync(id, userId.Value);

            if (!result.IsSuccess)
                return NotFound(result);

            return NoContent();
        }
    }
}
