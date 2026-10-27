using WorkerBrokerInteration.Domain.Entities;

namespace WorkerBrokerInteration.Infrastructure.Persistence;

public static class JobConfigurationSeed
{
    private static readonly DateTime SeededAt = new(2026, 10, 27, 0, 0, 0, DateTimeKind.Utc);

    public static readonly JobConfiguration[] Jobs =
    [
        new() { Id = 1, JobId = "SampleJob", CronExpression = "* * * * *", IsActive = true, CreatedAt = SeededAt, UpdatedAt = SeededAt },
        new() { Id = 2, JobId = "AnotherSampleJob", CronExpression = "*/5 * * * *", IsActive = false, CreatedAt = SeededAt, UpdatedAt = SeededAt }
    ];
}
