using ALS.Core.Constants;

namespace ALS.Consumer
{
    public class Worker(
        ILogger<Worker> logger,
        IConfiguration configuration) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var delayMs = configuration.GetValue<int?>($"{Constants.ConfigurationKeys.Worker}:{Constants.ConfigurationKeys.DelayMilliseconds}")
                ?? throw new InvalidOperationException(Constants.ErrorMessages.WorkerDelayMilliseconsNotConfigured);

            while (!stoppingToken.IsCancellationRequested)
            {
                logger.LogInformation(Constants.LogMessages.WorkerRunningAt, DateTimeOffset.Now);
                await Task.Delay(delayMs, stoppingToken);
            }
        }
    }
}