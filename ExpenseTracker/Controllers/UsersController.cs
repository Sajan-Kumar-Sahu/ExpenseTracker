using ExpenseTracker.Application.DTOs.User;
using ExpenseTracker.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
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
            var result = await _userService.GetByIdAsync(id);

            if (!result.IsSuccess)
                return NotFound(result);

            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _userService.GetAllAsync();

            return Ok(result);
        }

        [HttpPatch("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserRequest request)
        {
            var result = await _userService.UpdateAsync(id, request);

            if (!result.IsSuccess)
                return NotFound(result);

            return Ok(result);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _userService.DeleteAsync(id);

            if (!result.IsSuccess)
                return NotFound(result);

            return NoContent();
        }
    }
}
