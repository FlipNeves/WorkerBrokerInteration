using WorkerBrokerInteration.Tests.Support;

namespace WorkerBrokerInteration.Tests;

public sealed class JobConfigurationRepositoryTests : IDisposable
{
    private readonly TestDatabase _database = new();

    [Fact]
    public async Task UpdateScheduleAsync_SavesTheNewSchedule()
    {
        await _database.CreateRepository().UpdateScheduleAsync("AnotherSampleJob", "0 * * * *", isActive: true, CancellationToken.None);

        var configuration = Assert.Single(await _database.CreateRepository().GetAllAsync(CancellationToken.None), job => job.JobId == "AnotherSampleJob");
        Assert.Equal("0 * * * *", configuration.CronExpression);
        Assert.True(configuration.IsActive);
        Assert.Equal(FixedTimeProvider.Now.UtcDateTime, configuration.UpdatedAt);
    }

    [Fact]
    public async Task UpdateScheduleAsync_UnknownJob_ReturnsNull()
    {
        var configuration = await _database.CreateRepository().UpdateScheduleAsync("MissingJob", "0 * * * *", isActive: true, CancellationToken.None);

        Assert.Null(configuration);
    }

    [Fact]
    public async Task GetAllAsync_ReadsDatesBackAsUtc()
    {
        await _database.CreateRepository().RegisterExecutionAsync("SampleJob", FixedTimeProvider.Now.UtcDateTime, CancellationToken.None);

        var configuration = Assert.Single(await _database.CreateRepository().GetAllAsync(CancellationToken.None), job => job.JobId == "SampleJob");
        Assert.Equal(DateTimeKind.Utc, configuration.CreatedAt.Kind);
        Assert.Equal(DateTimeKind.Utc, configuration.LastExecutedAt?.Kind);
    }

    public void Dispose() => _database.Dispose();
}
