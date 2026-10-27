using HangfireDynamicScheduler.Domain.Entities;
using HangfireDynamicScheduler.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HangfireDynamicScheduler.Infrastructure.Persistence;

public sealed class JobConfigurationRepository : IJobConfigurationRepository
{
    private readonly JobsDbContext _context;
    private readonly TimeProvider _timeProvider;

    public JobConfigurationRepository(JobsDbContext context, TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task<IReadOnlyList<JobConfiguration>> GetAllAsync(CancellationToken cancellationToken) =>
        await _context.JobConfigurations.AsNoTracking().OrderBy(job => job.JobId).ToListAsync(cancellationToken);

    public async Task<JobConfiguration?> UpdateScheduleAsync(string jobId, string cronExpression, bool isActive, CancellationToken cancellationToken)
    {
        var configuration = await _context.JobConfigurations.SingleOrDefaultAsync(job => job.JobId == jobId, cancellationToken);
        if (configuration is null)
            return null;

        configuration.CronExpression = cronExpression;
        configuration.IsActive = isActive;
        configuration.UpdatedAt = _timeProvider.GetUtcNow().UtcDateTime;
        await _context.SaveChangesAsync(cancellationToken);

        return configuration;
    }

    public Task RegisterExecutionAsync(string jobId, DateTime executedAt, CancellationToken cancellationToken) =>
        _context.JobConfigurations
            .Where(job => job.JobId == jobId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(job => job.LastExecutedAt, executedAt), cancellationToken);
}
