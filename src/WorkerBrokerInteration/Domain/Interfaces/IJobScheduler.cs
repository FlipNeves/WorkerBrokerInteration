using WorkerBrokerInteration.Domain.Entities;

namespace WorkerBrokerInteration.Domain.Interfaces;

public interface IJobScheduler
{
    bool IsValidCron(string cronExpression);
    Task ScheduleAsync(JobConfiguration configuration, CancellationToken cancellationToken);
    Task UnscheduleAsync(string jobId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<string>> GetScheduledJobIdsAsync(CancellationToken cancellationToken);
    Task<DateTime?> GetNextRunAsync(string jobId, CancellationToken cancellationToken);
}
