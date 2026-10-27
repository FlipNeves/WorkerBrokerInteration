using WorkerBrokerInteration.Domain.Interfaces;

namespace WorkerBrokerInteration.Services;

public sealed class JobRunner
{
    private readonly JobCatalog _catalog;
    private readonly IServiceProvider _services;
    private readonly IJobConfigurationRepository _repository;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<JobRunner> _logger;

    public JobRunner(
        JobCatalog catalog,
        IServiceProvider services,
        IJobConfigurationRepository repository,
        TimeProvider timeProvider,
        ILogger<JobRunner> logger)
    {
        _catalog = catalog;
        _services = services;
        _repository = repository;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task RunAsync(string jobId, CancellationToken cancellationToken)
    {
        var jobType = _catalog.Find(jobId)
            ?? throw new InvalidOperationException($"No job is registered with the id '{jobId}'.");

        var job = (IJob)_services.GetRequiredService(jobType);

        _logger.LogInformation("Running job {JobId}", jobId);
        await job.ExecuteAsync(cancellationToken);
        await _repository.RegisterExecutionAsync(jobId, _timeProvider.GetUtcNow().UtcDateTime, cancellationToken);
        _logger.LogInformation("Job {JobId} finished", jobId);
    }
}
