using Hangfire;
using WorkerBrokerInteration.Services.Interfaces;

namespace WorkerBrokerInteration.HangFireBasics
{
    public class CreaterJobs
    {
        public static void RegisterJob(string cron)
        {
            RegisterJobCreater(cron);
        }

        private static void RegisterJobCreater(string cron)
        {
            RecurringJob.AddOrUpdate<IJobSchedulerService>(
                recurringJobId: RecurringJobsIds.JobCreation,
                methodCall: x => x.Execute(),
                cronExpression: cron);
        }
    }
}
