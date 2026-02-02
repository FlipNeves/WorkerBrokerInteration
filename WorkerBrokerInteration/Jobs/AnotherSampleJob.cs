using WorkerBrokerInteration.Domain.Interfaces;

namespace WorkerBrokerInteration.Jobs
{
    /// <summary>
    /// Another sample job to demonstrate having multiple jobs in the system.
    /// </summary>
    public class AnotherSampleJob : IJob
    {
        private readonly ILogger<AnotherSampleJob> _logger;

        public string JobId => "AnotherSampleJob";

        public AnotherSampleJob(ILogger<AnotherSampleJob> logger)
        {
            _logger = logger;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[{JobId}] Starting processing...", JobId);

            // Simulating some work with delays
            for (int i = 1; i <= 3; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
            }

            _logger.LogInformation("[{JobId}] Processing completed.", JobId);
        }
    }
}
