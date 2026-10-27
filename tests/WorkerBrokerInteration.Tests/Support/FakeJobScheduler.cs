using WorkerBrokerInteration.Domain.Entities;
using WorkerBrokerInteration.Domain.Interfaces;

namespace WorkerBrokerInteration.Tests.Support;

public sealed class FakeJobScheduler : IJobScheduler
{
    public Dictionary<string, string> Scheduled { get; } = [];

    public bool IsValidCron(string cronExpression) => true;

    public Task ScheduleAsync(JobConfiguration configuration, CancellationToken cancellationToken)
    {
        Scheduled[configuration.JobId] = configuration.CronExpression;
        return Task.CompletedTask;
    }

    public Task UnscheduleAsync(string jobId, CancellationToken cancellationToken)
    {
        Scheduled.Remove(jobId);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyCollection<string>> GetScheduledJobIdsAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyCollection<string>>(Scheduled.Keys.ToList());

    public Task<DateTime?> GetNextRunAsync(string jobId, CancellationToken cancellationToken) =>
        Task.FromResult<DateTime?>(null);
}
