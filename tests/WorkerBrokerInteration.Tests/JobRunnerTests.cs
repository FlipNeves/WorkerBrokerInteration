using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using WorkerBrokerInteration.Domain.Interfaces;
using WorkerBrokerInteration.Services;
using WorkerBrokerInteration.Tests.Support;

namespace WorkerBrokerInteration.Tests;

public sealed class JobRunnerTests : IDisposable
{
    private const string JobId = "SampleJob";

    private readonly TestDatabase _database = new();

    [Fact]
    public async Task RunAsync_ExecutesTheJobAndRecordsTheExecution()
    {
        var job = new RecordingJob();

        await CreateRunner(job).RunAsync(JobId, CancellationToken.None);

        Assert.Equal(1, job.Executions);
        Assert.Equal(FixedTimeProvider.Now.UtcDateTime, await LastExecutionAsync());
    }

    [Fact]
    public async Task RunAsync_FailingJob_DoesNotRecordAnExecution()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() => CreateRunner(new FailingJob()).RunAsync(JobId, CancellationToken.None));

        Assert.Null(await LastExecutionAsync());
    }

    [Fact]
    public async Task RunAsync_UnknownJob_Throws()
    {
        var job = new RecordingJob();

        await Assert.ThrowsAsync<InvalidOperationException>(() => CreateRunner(job).RunAsync("MissingJob", CancellationToken.None));

        Assert.Equal(0, job.Executions);
    }

    public void Dispose() => _database.Dispose();

    private JobRunner CreateRunner(IJob job)
    {
        var services = new ServiceCollection().AddSingleton(job.GetType(), job).BuildServiceProvider();
        var catalog = new JobCatalog([new JobRegistration(JobId, job.GetType())]);

        return new JobRunner(catalog, services, _database.CreateRepository(), new FixedTimeProvider(), NullLogger<JobRunner>.Instance);
    }

    private async Task<DateTime?> LastExecutionAsync()
    {
        await using var context = _database.CreateContext();
        var configuration = await context.JobConfigurations.SingleAsync(job => job.JobId == JobId);
        return configuration.LastExecutedAt;
    }
}
