using ExpenseTracker.Application.DTOs.Common;
using ExpenseTracker.Application.DTOs.FinancialTransaction;
using ExpenseTracker.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TransactionsController : BaseController
    {
        private readonly ITransactionService _transactionService;

        public TransactionsController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTransactionRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId is null) return UnauthorizedUser();

            var result = await _transactionService.CreateAsync(userId.Value, request);

            if (!result.IsSuccess)
                return BadRequest(result);

            return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var userId = GetCurrentUserId();
            if (userId is null) return UnauthorizedUser();

            var result = await _transactionService.GetByIdAsync(id, userId.Value);

            if (!result.IsSuccess)
                return NotFound(result);

            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PaginationRequest pagination)
        {
            var userId = GetCurrentUserId();
            if (userId is null) return UnauthorizedUser();

            var result = await _transactionService.GetAllByUserAsync(userId.Value, pagination);

            return Ok(result);
        }

        [HttpPatch("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTransactionRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId is null) return UnauthorizedUser();

            var result = await _transactionService.UpdateAsync(id, userId.Value, request);

            if (!result.IsSuccess)
                return NotFound(result);

            return Ok(result);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userId = GetCurrentUserId();
            if (userId is null) return UnauthorizedUser();

            var result = await _transactionService.DeleteAsync(id, userId.Value);

            if (!result.IsSuccess)
                return NotFound(result);

            return NoContent();
        }
    }
}
