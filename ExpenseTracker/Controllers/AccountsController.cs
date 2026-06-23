using ExpenseTracker.Application.DTOs.Account;
using ExpenseTracker.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AccountsController : BaseController
    {
        private readonly IAccountService _accountService;

        public AccountsController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAccountRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId is null) return UnauthorizedUser();

            var result = await _accountService.CreateAsync(userId.Value, request);

            if (!result.IsSuccess)
                return BadRequest(result);

            return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var userId = GetCurrentUserId();
            if (userId is null) return UnauthorizedUser();

            var result = await _accountService.GetByIdAsync(id, userId.Value);

            if (!result.IsSuccess)
                return NotFound(result);

            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var userId = GetCurrentUserId();
            if (userId is null) return UnauthorizedUser();

            var result = await _accountService.GetAllByUserAsync(userId.Value);

            return Ok(result);
        }

        [HttpPatch]
        public async Task<IActionResult> Update([FromBody] UpdateAccountRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId is null) return UnauthorizedUser();

            var result = await _accountService.UpdateAsync(userId.Value, request);

            if (!result.IsSuccess)
                return NotFound(result);

            return Ok(result);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userId = GetCurrentUserId();
            if (userId is null) return UnauthorizedUser();

            var result = await _accountService.DeleteAsync(id, userId.Value);

            if (!result.IsSuccess)
                return NotFound(result);

            return NoContent();
        }
    }
}
