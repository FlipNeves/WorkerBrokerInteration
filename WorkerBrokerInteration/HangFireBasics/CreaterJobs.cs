using Hangfire;
using WorkerBrokerInteration.Services.Interfaces;

namespace WorkerBrokerInteration.HangFireBasics
{
    /// <summary>
    /// Handles the registration of the main job that keeps the database jobs
    /// in sync with Hangfire.
    /// This job runs on a schedule and:
    /// - Creates or updates active jobs from the database in Hangfire
    /// - Removes from Hangfire any jobs that were disabled in the database
    /// </summary>
    public static class CreaterJobs
    {
        public const string JobSchedulerJobId = "JobScheduler";

        /// <summary>
        /// Sets up the Job Scheduler using the given cron expression.
        /// </summary>
        /// <param name="cronExpression">Cron expression that controls how often the sync runs.</param>
        public static void RegisterJobScheduler(string cronExpression)
        {
            RecurringJob.AddOrUpdate<IJobSchedulerService>(
                recurringJobId: JobSchedulerJobId,
                methodCall: service => service.ExecuteAsync(default),
                cronExpression: cronExpression);
        }
    }
}
