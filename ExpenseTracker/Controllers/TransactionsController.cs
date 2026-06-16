using ExpenseTracker.Application.DTOs.FinancialTransaction;
using ExpenseTracker.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionService _transactionService;

        public TransactionsController(
            ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateTransactionRequest request)
        {
            var result = await _transactionService.CreateAsync(request);

            if (!result.IsSuccess)
                return BadRequest(result);

            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Data!.Id },
                result);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _transactionService.GetByIdAsync(id);

            if (!result.IsSuccess)
                return NotFound(result);

            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _transactionService.GetAllAsync();

            return Ok(result);
        }

        [HttpGet("user/{userId:guid}")]
        public async Task<IActionResult> GetByUserId(Guid userId)
        {
            var result = await _transactionService.GetByUserIdAsync(userId);

            return Ok(result);
        }

        [HttpPatch("{id:guid}")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateTransactionRequest request)
        {
            var result = await _transactionService.UpdateAsync(
                id,
                request);

            if (!result.IsSuccess)
                return NotFound(result);

            return Ok(result);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _transactionService.DeleteAsync(id);

            if (!result.IsSuccess)
                return NotFound(result);

            return NoContent();
        }
    }
}
