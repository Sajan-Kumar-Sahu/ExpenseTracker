using ExpenseTracker.Application.DTOs.Category;
using ExpenseTracker.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CategoriesController : BaseController
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId is null) return UnauthorizedUser();

            var result = await _categoryService.CreateAsync(userId.Value, request);

            if (!result.IsSuccess)
                return BadRequest(result);

            return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var userId = GetCurrentUserId();
            if (userId is null) return UnauthorizedUser();

            var result = await _categoryService.GetByIdAsync(id, userId.Value);

            if (!result.IsSuccess)
                return NotFound(result);

            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var userId = GetCurrentUserId();
            if (userId is null) return UnauthorizedUser();

            return Ok(await _categoryService.GetAllByUserAsync(userId.Value));
        }

        [HttpPatch("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCategoryRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId is null) return UnauthorizedUser();

            var result = await _categoryService.UpdateAsync(id, userId.Value, request);

            if (!result.IsSuccess)
                return NotFound(result);

            return Ok(result);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userId = GetCurrentUserId();
            if (userId is null) return UnauthorizedUser();

            var result = await _categoryService.DeleteAsync(id, userId.Value);

            if (!result.IsSuccess)
                return NotFound(result);

            return NoContent();
        }
    }
}
