using WorkerBrokerInteration.Domain.Entities;

namespace WorkerBrokerInteration.Domain.Interfaces;

public interface IJobConfigurationRepository
{
    Task<IReadOnlyList<JobConfiguration>> GetAllAsync(CancellationToken cancellationToken);
    Task<JobConfiguration?> UpdateScheduleAsync(string jobId, string cronExpression, bool isActive, CancellationToken cancellationToken);
    Task RegisterExecutionAsync(string jobId, DateTime executedAt, CancellationToken cancellationToken);
}
