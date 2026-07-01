using ExpenseTracker.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ExpenseTracker.API.BackgroundServices
{
    public class ReminderBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ReminderBackgroundService> _logger;

        // Run every 5 minutes. Replace with Quartz.NET or Hangfire trigger when needed.
        private static readonly TimeSpan Interval = TimeSpan.FromMinutes(1);

        public ReminderBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<ReminderBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Reminder background service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var processingService =
                        scope.ServiceProvider.GetRequiredService<IReminderProcessingService>();

                    await processingService.ProcessDueRemindersAsync(stoppingToken);
                    await processingService.ProcessExpiredRemindersAsync(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while processing reminders.");
                }

                await Task.Delay(Interval, stoppingToken);
            }

            _logger.LogInformation("Reminder background service stopped.");
        }
    }
}
