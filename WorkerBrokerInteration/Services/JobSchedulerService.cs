using Hangfire;
using Hangfire.Storage;
using WorkerBrokerInteration.Jobs.SampleJobExample;
using WorkerBrokerInteration.Services.Interfaces;

namespace WorkerBrokerInteration.Services
{
    public class JobSchedulerService : IJobSchedulerService
    {
        private readonly IConfigurationsService _configurationsService;
		private const int CheckIntervalSeconds = 5;


		public JobSchedulerService(IConfigurationsService configurationsService)
        {
            _configurationsService = configurationsService;
        }

		public async Task Execute()
		{
			var jobDefinitions = new List<(Type jobType, string jobId)>
	        {
                // Add new jobs here by specifying the job type and it`s corresponding ID.
                // This structure ensures that your jobs are properly configured and managed.
                (typeof(JobSampleJobExample), RecurringJobsIds.SampleJobExample)
	        };

			// Schedule all the defined jobs concurrently.
			var tasks = jobDefinitions.Select(jobDef => AddOrUpdate(jobDef.jobType, jobDef.jobId));
			await Task.WhenAll(tasks);  // Wait for all jobs to be processed.
		}

		public async Task AddOrUpdate(Type jobType, string recurringJobId)
        {
            await WaitForRunningJobToFinish(jobType);

            if (!IsJobActive(recurringJobId))
            {
                if (RecurringJobExists(recurringJobId))
                {
                    RecurringJob.RemoveIfExists(recurringJobId);
                }
                return;
            }

            await AddOrUpdateJob(() =>
            {
                RecurringJob.AddOrUpdate(
                    recurringJobId,
                    () => ExecuteJob(jobType),
                    GetCronExpression(jobType)
                );
            });
        }

        private bool IsJobActive(string recurringJobId) => _configurationsService.JobStatus(recurringJobId);

        private bool RecurringJobExists(string recurringJobId)
        {
            var job = JobStorage.Current.GetConnection().GetRecurringJobs().FirstOrDefault(j => j.Id == recurringJobId);
            return job != null;
        }

        public async Task AddOrUpdateJob(Action configureJob)
        {
            configureJob();
            await Task.CompletedTask;
        }

		private string GetCronExpression(Type jobType) => jobType switch
		{
			// For each new job, you'll need to add a new case here to provide the correct cron expression.
			// This ensures that each job runs on it's desired schedule.
			var t when t == typeof(JobSampleJobExample) => _configurationsService.GetCronJobSampleJobExample(),

			// If an unsupported job type is encountered, throw an exception to signal the issue.
			_ => throw new NotSupportedException($"Unsupported job type: {jobType.Name}")
		};

		public Task ExecuteJob(Type jobType)
		{
			var jobInstance = Activator.CreateInstance(jobType) as ISimpleJob;
			return jobInstance?.Execute() ?? Task.CompletedTask;
		}

		private async Task WaitForRunningJobToFinish(Type jobType)
		{
			while (await IsJobRunning(jobType))
			{
				await Task.Delay(TimeSpan.FromSeconds(CheckIntervalSeconds));
			}
		}

		private async Task<bool> IsJobRunning(Type jobType)
		{
			var runningJobs = JobStorage.Current.GetMonitoringApi().ProcessingJobs(0, int.MaxValue);

			return runningJobs.Any(job =>
			{
				var runningJobType = job.Value.Job.Type;

				if (runningJobType == typeof(JobSchedulerService))
					return true;

				return runningJobType == jobType || jobType.IsAssignableFrom(runningJobType);
			});
		}
	}
}
