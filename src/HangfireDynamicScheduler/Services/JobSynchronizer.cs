using HangfireDynamicScheduler.Domain.Entities;
using HangfireDynamicScheduler.Domain.Interfaces;

namespace HangfireDynamicScheduler.Services;

public sealed class JobSynchronizer
{
    private readonly IJobConfigurationRepository _repository;
    private readonly IJobScheduler _scheduler;
    private readonly JobCatalog _catalog;
    private readonly ILogger<JobSynchronizer> _logger;

    public JobSynchronizer(
        IJobConfigurationRepository repository,
        IJobScheduler scheduler,
        JobCatalog catalog,
        ILogger<JobSynchronizer> logger)
    {
        _repository = repository;
        _scheduler = scheduler;
        _catalog = catalog;
        _logger = logger;
    }

    public async Task SyncAsync(CancellationToken cancellationToken)
    {
        var configurations = await _repository.GetAllAsync(cancellationToken);

        foreach (var configuration in configurations)
        {
            try
            {
                await ApplyAsync(configuration, cancellationToken);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                _logger.LogError(exception, "Could not apply the configuration of job {JobId}", configuration.JobId);
            }
        }

        var configuredJobIds = configurations.Select(configuration => configuration.JobId).ToHashSet();
        var scheduledJobIds = await _scheduler.GetScheduledJobIdsAsync(cancellationToken);

        foreach (var jobId in scheduledJobIds.Where(jobId => !configuredJobIds.Contains(jobId)))
        {
            await _scheduler.UnscheduleAsync(jobId, cancellationToken);
            _logger.LogInformation("Job {JobId} is no longer in the database and was unscheduled", jobId);
        }
    }

    public async Task ApplyAsync(JobConfiguration configuration, CancellationToken cancellationToken)
    {
        if (configuration.IsActive && _catalog.Contains(configuration.JobId))
        {
            await _scheduler.ScheduleAsync(configuration, cancellationToken);
            return;
        }

        if (configuration.IsActive)
            _logger.LogWarning("Job {JobId} is active in the database but has no registered implementation", configuration.JobId);

        await _scheduler.UnscheduleAsync(configuration.JobId, cancellationToken);
    }
}
