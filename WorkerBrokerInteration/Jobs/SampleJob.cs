using WorkerBrokerInteration.Domain.Interfaces;

namespace WorkerBrokerInteration.Jobs
{
    /// <summary>
    /// Sample job that demonstrates the implementation of the IJob interface.
    /// The JobId "SampleJob" must match the JobId stored in the database configuration.
    /// </summary>
    public class SampleJob : IJob
    {
        private readonly ILogger<SampleJob> _logger;

        public string JobId => "SampleJob";

        public SampleJob(ILogger<SampleJob> logger)
        {
            _logger = logger;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[{JobId}] Starting job execution...", JobId);

            try
            {
                // Simulate some processing work
                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);

                _logger.LogInformation("[{JobId}] Job completed successfully.", JobId);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("[{JobId}] Job was canceled.", JobId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{JobId}] Error during job execution.", JobId);
                throw;
            }
        }
    }
}
