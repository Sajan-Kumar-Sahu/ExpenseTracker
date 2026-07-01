using ExpenseTracker.API.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Hosting;

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

            var env = httpContext.RequestServices
                .GetRequiredService<IHostEnvironment>();

            var message = env.IsDevelopment() && exception.InnerException is not null
                ? $"{exception.Message} | Inner: {exception.InnerException.Message}"
                : exception.Message;

            var response = new ApiResponse<object>
            {
                Success = false,
                Message = message
            };

            httpContext.Response.StatusCode = 500;

            await httpContext.Response.WriteAsJsonAsync(
                response,
                cancellationToken);

            return true;
        }
    }
}
