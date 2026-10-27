using Cronos;
using Hangfire;
using Hangfire.Storage;
using WorkerBrokerInteration.Domain.Entities;
using WorkerBrokerInteration.Domain.Interfaces;
using WorkerBrokerInteration.Services;

namespace WorkerBrokerInteration.Infrastructure.Scheduling;

public sealed class HangfireJobScheduler : IJobScheduler
{
    private readonly IRecurringJobManager _recurringJobs;
    private readonly JobStorage _storage;

    public HangfireJobScheduler(IRecurringJobManager recurringJobs, JobStorage storage)
    {
        _recurringJobs = recurringJobs;
        _storage = storage;
    }

    public bool IsValidCron(string cronExpression) =>
        CronExpression.TryParse(cronExpression, CronFormat.Standard, out _);

    public Task ScheduleAsync(JobConfiguration configuration, CancellationToken cancellationToken)
    {
        var jobId = configuration.JobId;

        _recurringJobs.AddOrUpdate<JobRunner>(
            jobId,
            runner => runner.RunAsync(jobId, CancellationToken.None),
            configuration.CronExpression,
            new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });

        return Task.CompletedTask;
    }

    public Task UnscheduleAsync(string jobId, CancellationToken cancellationToken)
    {
        _recurringJobs.RemoveIfExists(jobId);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyCollection<string>> GetScheduledJobIdsAsync(CancellationToken cancellationToken)
    {
        using var connection = _storage.GetConnection();
        IReadOnlyCollection<string> jobIds = connection.GetRecurringJobs().Select(job => job.Id).ToList();
        return Task.FromResult(jobIds);
    }

    public Task<DateTime?> GetNextRunAsync(string jobId, CancellationToken cancellationToken)
    {
        using var connection = _storage.GetConnection();
        var nextExecution = connection.GetRecurringJobs([jobId]).SingleOrDefault(job => !job.Removed)?.NextExecution;
        return Task.FromResult(nextExecution is { } next ? DateTime.SpecifyKind(next, DateTimeKind.Utc) : (DateTime?)null);
    }
}
