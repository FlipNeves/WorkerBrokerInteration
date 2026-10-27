using WorkerBrokerInteration.Domain.Interfaces;

namespace WorkerBrokerInteration.Jobs;

public sealed class AnotherSampleJob : IJob
{
    private const int Steps = 3;

    private readonly ILogger<AnotherSampleJob> _logger;

    public AnotherSampleJob(ILogger<AnotherSampleJob> logger)
    {
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        for (var step = 1; step <= Steps; step++)
        {
            await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            _logger.LogInformation("{Job} finished step {Step} of {Steps}", nameof(AnotherSampleJob), step, Steps);
        }
    }
}
