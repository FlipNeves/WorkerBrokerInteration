namespace WorkerBrokerInteration.Services.Interfaces
{
    /// <summary>
    /// Interface for the service responsible for synchronizing database jobs with Hangfire.
    /// </summary>
    public interface IJobSchedulerService
    {
        /// <summary>
        /// Runs the synchronization: registers active jobs and removes inactive jobs from Hangfire.
        /// </summary>
        Task ExecuteAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes a specific job by its type name.
        /// Called by Hangfire when the job is triggered.
        /// </summary>
        Task ExecuteJobAsync(string jobTypeName, long configId);
    }
}
