using WorkerBrokerInteration.Domain.Entities;

namespace WorkerBrokerInteration.Domain.Interfaces
{
    /// <summary>
    /// Repository interface for job configuration data.
    /// </summary>
    public interface IJobConfigurationRepository
    {
        /// <summary>
        /// Returns all active job configurations.
        /// </summary>
        Task<IEnumerable<JobConfiguration>> GetAllActiveAsync();

        /// <summary>
        /// Returns the JobIds of all inactive jobs.
        /// </summary>
        Task<IEnumerable<string>> GetAllInactiveJobIdsAsync();

        /// <summary>
        /// Updates the last execution timestamp of the job.
        /// </summary>
        Task UpdateLastExecutionAsync(long configId, DateTime executedAt);
    }
}
