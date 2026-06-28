using ExpenseTracker.Application.DTOs.WorkLog;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WorkLogsController : BaseController
    {
        private readonly IWorkLogService _workLogService;

        public WorkLogsController(IWorkLogService workLogService)
        {
            _workLogService = workLogService;
        }

        // ── CRUD ────────────────────────────────────────────────────────────────

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateWorkLogRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId is null) return UnauthorizedUser();

            var result = await _workLogService.CreateAsync(userId.Value, request);

            if (!result.IsSuccess)
                return BadRequest(result);

            return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var userId = GetCurrentUserId();
            if (userId is null) return UnauthorizedUser();

            var result = await _workLogService.GetByIdAsync(id, userId.Value);

            if (!result.IsSuccess)
                return NotFound(result);

            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var userId = GetCurrentUserId();
            if (userId is null) return UnauthorizedUser();

            var result = await _workLogService.GetAllByUserAsync(userId.Value);

            return Ok(result);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateWorkLogRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId is null) return UnauthorizedUser();

            var result = await _workLogService.UpdateAsync(id, userId.Value, request);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userId = GetCurrentUserId();
            if (userId is null) return UnauthorizedUser();

            var result = await _workLogService.DeleteAsync(id, userId.Value);

            if (!result.IsSuccess)
                return BadRequest(result);

            return NoContent();
        }

        // ── Filters ─────────────────────────────────────────────────────────────

        [HttpGet("status/{status:int}")]
        public async Task<IActionResult> GetByStatus(int status)
        {
            var userId = GetCurrentUserId();
            if (userId is null) return UnauthorizedUser();

            if (!Enum.IsDefined(typeof(WorkLogStatus), status))
                return BadRequest(new { IsSuccess = false, Message = "Invalid status value." });

            var result = await _workLogService.GetByStatusAsync(userId.Value, (WorkLogStatus)status);

            return Ok(result);
        }

        [HttpGet("worktype/{workType:int}")]
        public async Task<IActionResult> GetByWorkType(int workType)
        {
            var userId = GetCurrentUserId();
            if (userId is null) return UnauthorizedUser();

            if (!Enum.IsDefined(typeof(WorkType), workType))
                return BadRequest(new { IsSuccess = false, Message = "Invalid work type value." });

            var result = await _workLogService.GetByWorkTypeAsync(userId.Value, (WorkType)workType);

            return Ok(result);
        }

        [HttpGet("project/{projectId:guid}")]
        public async Task<IActionResult> GetByProject(Guid projectId)
        {
            var userId = GetCurrentUserId();
            if (userId is null) return UnauthorizedUser();

            var result = await _workLogService.GetByProjectAsync(projectId, userId.Value);

            if (!result.IsSuccess)
                return NotFound(result);

            return Ok(result);
        }

        [HttpGet("month/{year:int}/{month:int}")]
        public async Task<IActionResult> GetByMonth(int year, int month)
        {
            var userId = GetCurrentUserId();
            if (userId is null) return UnauthorizedUser();

            var result = await _workLogService.GetByMonthAsync(userId.Value, year, month);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("pending")]
        public async Task<IActionResult> GetPending()
        {
            var userId = GetCurrentUserId();
            if (userId is null) return UnauthorizedUser();

            var result = await _workLogService.GetPendingAsync(userId.Value);

            return Ok(result);
        }

        [HttpGet("applied-not-paid")]
        public async Task<IActionResult> GetAppliedNotPaid()
        {
            var userId = GetCurrentUserId();
            if (userId is null) return UnauthorizedUser();

            var result = await _workLogService.GetAppliedNotPaidAsync(userId.Value);

            return Ok(result);
        }

        // ── Analytics ───────────────────────────────────────────────────────────

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            var userId = GetCurrentUserId();
            if (userId is null) return UnauthorizedUser();

            var result = await _workLogService.GetDashboardAsync(userId.Value);

            return Ok(result);
        }

        [HttpGet("summary/{year:int}")]
        public async Task<IActionResult> GetYearlySummary(int year)
        {
            var userId = GetCurrentUserId();
            if (userId is null) return UnauthorizedUser();

            var result = await _workLogService.GetYearlySummaryAsync(userId.Value, year);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        // ── Status transitions ───────────────────────────────────────────────────

        [HttpPost("{id:guid}/apply")]
        public async Task<IActionResult> Apply(Guid id)
        {
            var userId = GetCurrentUserId();
            if (userId is null) return UnauthorizedUser();

            var result = await _workLogService.ApplyAsync(id, userId.Value);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("{id:guid}/approve")]
        public async Task<IActionResult> Approve(Guid id)
        {
            var userId = GetCurrentUserId();
            if (userId is null) return UnauthorizedUser();

            var result = await _workLogService.ApproveAsync(id, userId.Value);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("{id:guid}/reject")]
        public async Task<IActionResult> Reject(Guid id)
        {
            var userId = GetCurrentUserId();
            if (userId is null) return UnauthorizedUser();

            var result = await _workLogService.RejectAsync(id, userId.Value);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("{id:guid}/mark-paid")]
        public async Task<IActionResult> MarkPaid(Guid id, [FromBody] MarkPaidRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId is null) return UnauthorizedUser();

            var result = await _workLogService.MarkPaidAsync(id, userId.Value, request);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }
    }
}
