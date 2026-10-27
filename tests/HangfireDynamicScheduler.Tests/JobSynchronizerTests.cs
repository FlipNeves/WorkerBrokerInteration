using HangfireDynamicScheduler.Services;
using HangfireDynamicScheduler.Tests.Support;
using Microsoft.Extensions.Logging.Abstractions;

namespace HangfireDynamicScheduler.Tests;

public sealed class JobSynchronizerTests : IDisposable
{
    private readonly TestDatabase _database = new();
    private readonly FakeJobScheduler _scheduler = new();

    [Fact]
    public async Task SyncAsync_SchedulesOnlyActiveJobs()
    {
        await CreateSynchronizer("SampleJob", "AnotherSampleJob").SyncAsync(CancellationToken.None);

        Assert.Equal(new Dictionary<string, string> { ["SampleJob"] = "* * * * *" }, _scheduler.Scheduled);
    }

    [Fact]
    public async Task SyncAsync_PicksUpScheduleChangesFromTheDatabase()
    {
        var synchronizer = CreateSynchronizer("SampleJob", "AnotherSampleJob");
        await synchronizer.SyncAsync(CancellationToken.None);

        var repository = _database.CreateRepository();
        await repository.UpdateScheduleAsync("SampleJob", "0 * * * *", isActive: false, CancellationToken.None);
        await repository.UpdateScheduleAsync("AnotherSampleJob", "*/10 * * * *", isActive: true, CancellationToken.None);
        await synchronizer.SyncAsync(CancellationToken.None);

        Assert.Equal(new Dictionary<string, string> { ["AnotherSampleJob"] = "*/10 * * * *" }, _scheduler.Scheduled);
    }

    [Fact]
    public async Task SyncAsync_ActiveJobWithoutImplementation_IsNotScheduled()
    {
        await CreateSynchronizer("AnotherSampleJob").SyncAsync(CancellationToken.None);

        Assert.Empty(_scheduler.Scheduled);
    }

    [Fact]
    public async Task SyncAsync_JobRemovedFromTheDatabase_IsUnscheduled()
    {
        _scheduler.Scheduled["LegacyJob"] = "0 * * * *";

        await CreateSynchronizer("SampleJob").SyncAsync(CancellationToken.None);

        Assert.Equal("SampleJob", Assert.Single(_scheduler.Scheduled.Keys));
    }

    public void Dispose() => _database.Dispose();

    private JobSynchronizer CreateSynchronizer(params string[] registeredJobIds)
    {
        var catalog = new JobCatalog(registeredJobIds.Select(jobId => new JobRegistration(jobId, typeof(RecordingJob))));
        return new JobSynchronizer(_database.CreateRepository(), _scheduler, catalog, NullLogger<JobSynchronizer>.Instance);
    }
}
