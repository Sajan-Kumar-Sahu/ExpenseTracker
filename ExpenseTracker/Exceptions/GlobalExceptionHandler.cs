using ExpenseTracker.API.Common;
using Microsoft.AspNetCore.Diagnostics;

namespace ExpenseTracker.API.Exceptions
{
    public class GlobalExceptionHandler
    : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            _logger.LogError(
                exception,
                exception.Message);

            var response = new ApiResponse<object>
            {
                Success = false,
                Message = exception.Message
            };

            httpContext.Response.StatusCode = 500;

            await httpContext.Response.WriteAsJsonAsync(
                response,
                cancellationToken);

            return true;
        }
    }
}
