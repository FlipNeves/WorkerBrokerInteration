using WorkerBrokerInteration.Domain.Entities;
using WorkerBrokerInteration.Domain.Interfaces;

namespace WorkerBrokerInteration.Infrastructure.Repositories
{
    /// <summary>
    /// Mock repository for job configurations.
    /// In production, replace with an implementation using Entity Framework Core or another ORM.
    /// </summary>
    public class JobConfigurationRepository : IJobConfigurationRepository
    {
        // Simulates a job configuration table in the database
        private readonly List<JobConfiguration> _configurations;

        public JobConfigurationRepository()
        {
            // Mock data — in production this would come from the database
            _configurations = new List<JobConfiguration>
            {
                new JobConfiguration
                {
                    Id = 1,
                    JobId = "SampleJob",
                    JobTypeName = "WorkerBrokerInteration.Jobs.SampleJob",
                    CronExpression = "*/5 * * * *", // Every 5 minutes
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new JobConfiguration
                {
                    Id = 2,
                    JobId = "AnotherSampleJob",
                    JobTypeName = "WorkerBrokerInteration.Jobs.AnotherSampleJob",
                    CronExpression = "0 * * * *", // Every hour
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                // Example of an inactive job — will be removed from Hangfire
                new JobConfiguration
                {
                    Id = 3,
                    JobId = "InactiveJob",
                    JobTypeName = "WorkerBrokerInteration.Jobs.InactiveJob",
                    CronExpression = "0 0 * * *",
                    IsActive = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };
        }

        public Task<IEnumerable<JobConfiguration>> GetAllActiveAsync()
        {
            var activeJobs = _configurations.Where(c => c.IsActive).ToList();
            return Task.FromResult<IEnumerable<JobConfiguration>>(activeJobs);
        }

        public Task<IEnumerable<string>> GetAllInactiveJobIdsAsync()
        {
            var inactiveJobIds = _configurations
                .Where(c => !c.IsActive)
                .Select(c => c.JobId)
                .ToList();

            return Task.FromResult<IEnumerable<string>>(inactiveJobIds);
        }

        public Task UpdateLastExecutionAsync(long configId, DateTime executedAt)
        {
            var config = _configurations.FirstOrDefault(c => c.Id == configId);
            if (config != null)
            {
                config.LastExecutedAt = executedAt;
                config.UpdatedAt = DateTime.UtcNow;
            }

            return Task.CompletedTask;
        }
    }
}
