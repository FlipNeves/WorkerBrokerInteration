using Microsoft.Extensions.Options;
using WorkerBrokerInteration.Configurations;

namespace WorkerBrokerInteration.Services;

public sealed class JobSynchronizationService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly SchedulerOptions _options;
    private readonly ILogger<JobSynchronizationService> _logger;

    public JobSynchronizationService(
        IServiceScopeFactory scopeFactory,
        IOptions<SchedulerOptions> options,
        ILogger<JobSynchronizationService> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(_options.SyncInterval);

        do
        {
            await SynchronizeAsync(stoppingToken);
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task SynchronizeAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            await scope.ServiceProvider.GetRequiredService<JobSynchronizer>().SyncAsync(cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _logger.LogError(exception, "Job synchronization failed");
        }
    }
}
