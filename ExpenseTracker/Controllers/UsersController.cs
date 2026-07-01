using ExpenseTracker.Application.DTOs.User;
using ExpenseTracker.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : BaseController
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
        {
            var result = await _userService.CreateAsync(request);

            if (!result.IsSuccess)
                return Conflict(result);

            return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var userId = GetCurrentUserId();
            if (userId is null) return UnauthorizedUser();

            if (id != userId.Value)
                return Forbid();

            var result = await _userService.GetByIdAsync(id);

            if (!result.IsSuccess)
                return NotFound(result);

            return Ok(result);
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            var userId = GetCurrentUserId();
            if (userId is null) return UnauthorizedUser();

            var result = await _userService.GetByIdAsync(userId.Value);

            if (!result.IsSuccess)
                return NotFound(result);

            return Ok(result);
        }

        [HttpPatch("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId is null) return UnauthorizedUser();

            if (id != userId.Value)
                return Forbid();

            var result = await _userService.UpdateAsync(id, request);

            if (!result.IsSuccess)
                return NotFound(result);

            return Ok(result);
        }

        [HttpPut("me/device-token")]
        public async Task<IActionResult> UpdateDeviceToken([FromBody] UpdateDeviceTokenRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId is null) return UnauthorizedUser();

            var result = await _userService.UpdateDeviceTokenAsync(userId.Value, request.DeviceToken);

            if (!result.IsSuccess)
                return NotFound(result);

            return NoContent();
        }

        [HttpPost("me/test-push")]
        public async Task<IActionResult> TestPush(
            [FromServices] IPushNotificationService pushService)
        {
            var userId = GetCurrentUserId();
            if (userId is null) return UnauthorizedUser();

            var sent = await pushService.SendToUserAsync(
                userId.Value,
                title: "Test Notification",
                body: "Backend → FCM → Device is working!");

            return Ok(new { sent, userId });
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userId = GetCurrentUserId();
            if (userId is null) return UnauthorizedUser();

            if (id != userId.Value)
                return Forbid();

            var result = await _userService.DeleteAsync(id);

            if (!result.IsSuccess)
                return NotFound(result);

            return NoContent();
        }
    }
}
