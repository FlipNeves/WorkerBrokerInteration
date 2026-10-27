using WorkerBrokerInteration.Domain.Entities;

namespace WorkerBrokerInteration.Controllers;

public sealed record JobView(string JobId, string CronExpression, bool IsActive, DateTime? LastExecutedAt, DateTime? NextRunAt)
{
    public static JobView From(JobConfiguration configuration, DateTime? nextRunAt) =>
        new(configuration.JobId, configuration.CronExpression, configuration.IsActive, configuration.LastExecutedAt, nextRunAt);
}

public sealed record UpdateScheduleRequest(string CronExpression, bool IsActive);
