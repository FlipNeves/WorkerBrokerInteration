using Hangfire;
using WorkerBrokerInteration.Domain.Entities;
using WorkerBrokerInteration.Domain.Interfaces;
using WorkerBrokerInteration.Services.Interfaces;

namespace WorkerBrokerInteration.Services
{
    /// <summary>
    /// Service responsible for keeping database job configurations in sync with Hangfire.
    /// - Registers/updates active jobs
    /// - Removes inactive jobs
    /// - Executes jobs by resolving them through the DI container
    /// </summary>
    public class JobSchedulerService : IJobSchedulerService
    {
        private readonly IJobConfigurationRepository _repository;
        private readonly IRecurringJobManager _recurringJobManager;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<JobSchedulerService> _logger;

        public JobSchedulerService(
            IJobConfigurationRepository repository,
            IRecurringJobManager recurringJobManager,
            IServiceProvider serviceProvider,
            ILogger<JobSchedulerService> logger)
        {
            _repository = repository;
            _recurringJobManager = recurringJobManager;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        /// <summary>
        /// Runs the full sync: registers active jobs and removes inactive ones.
        /// </summary>
        public async Task ExecuteAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Starting job synchronization...");

            await RegisterOrUpdateAllActiveJobs();
            await RemoveIfExistsAllInactiveJobs();

            _logger.LogInformation("Job synchronization completed.");
        }

        /// <summary>
        /// Removes from Hangfire all jobs marked as inactive in the database.
        /// </summary>
        private async Task RemoveIfExistsAllInactiveJobs()
        {
            var inactiveJobIds = await _repository.GetAllInactiveJobIdsAsync();

            foreach (var jobId in inactiveJobIds)
            {
                _recurringJobManager.RemoveIfExists(jobId);
                _logger.LogInformation("Job removed from Hangfire: {JobId}", jobId);
            }
        }

        /// <summary>
        /// Registers or updates all active jobs from the database into Hangfire.
        /// </summary>
        private async Task RegisterOrUpdateAllActiveJobs()
        {
            var configurations = await _repository.GetAllActiveAsync();

            foreach (var config in configurations)
            {
                try
                {
                    RegisterOrUpdateJob(config);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while registering job {JobId}", config.JobId);
                }
            }
        }

        /// <summary>
        /// Registers or updates a specific job in Hangfire.
        /// </summary>
        private void RegisterOrUpdateJob(JobConfiguration config)
        {
            var jobType = ResolveJobType(config.JobTypeName);

            if (jobType == null)
            {
                _logger.LogWarning("Job type not found: {JobTypeName}", config.JobTypeName);
                return;
            }

            // Ensures the type implements IJob
            if (!typeof(IJob).IsAssignableFrom(jobType))
            {
                _logger.LogWarning("Type {JobTypeName} does not implement IJob", config.JobTypeName);
                return;
            }

            _recurringJobManager.AddOrUpdate(
                config.JobId,
                () => ExecuteJobAsync(config.JobTypeName, config.Id),
                config.CronExpression);

            _logger.LogInformation(
                "Job registered/updated: {JobId} | Cron: {CronExpression}",
                config.JobId,
                config.CronExpression);
        }

        /// <summary>
        /// Executes a specific job. Called by Hangfire when the job is triggered.
        /// Resolves the job through the DI container and updates its last execution date.
        /// </summary>
        public async Task ExecuteJobAsync(string jobTypeName, long configId)
        {
            using var scope = _serviceProvider.CreateScope();

            var jobType = ResolveJobType(jobTypeName);
            if (jobType == null)
            {
                _logger.LogError("Job type not found for execution: {JobTypeName}", jobTypeName);
                return;
            }

            // Resolve the job via DI
            var job = scope.ServiceProvider.GetService(jobType) as IJob;
            if (job == null)
            {
                _logger.LogError("Job not registered in the DI container: {JobTypeName}", jobTypeName);
                return;
            }

            try
            {
                _logger.LogInformation("Executing job: {JobId}", job.JobId);

                await job.ExecuteAsync();

                // Update the last execution timestamp in the database
                var repository = scope.ServiceProvider.GetRequiredService<IJobConfigurationRepository>();
                await repository.UpdateLastExecutionAsync(configId, DateTime.UtcNow);

                _logger.LogInformation("Job executed successfully: {JobId}", job.JobId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while executing job {JobId}", job.JobId);
                throw; // rethrow so Hangfire can record the failure
            }
        }

        /// <summary>
        /// Resolves the job type by its full name (namespace + class).
        /// Searches all loaded assemblies.
        /// </summary>
        private Type? ResolveJobType(string typeName)
        {
            // Try direct resolution first
            var type = Type.GetType(typeName);
            if (type != null) return type;

            // Search all loaded assemblies
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = assembly.GetType(typeName);
                if (type != null) return type;
            }

            return null;
        }
    }
}
