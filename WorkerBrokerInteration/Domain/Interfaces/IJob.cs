namespace WorkerBrokerInteration.Domain.Interfaces
{
    /// <summary>
    /// Base interface for all system jobs.
    /// JobId is used to match the job with its database configuration.
    /// </summary>
    public interface IJob
    {
        /// <summary>
        /// Unique identifier for the job.
        /// Must match the JobId in the database configuration table.
        /// </summary>
        string JobId { get; }

        /// <summary>
        /// Executes the job logic.
        /// </summary>
        /// <param name="cancellationToken">Token for graceful cancellation.</param>
        Task ExecuteAsync(CancellationToken cancellationToken = default);
    }
}
