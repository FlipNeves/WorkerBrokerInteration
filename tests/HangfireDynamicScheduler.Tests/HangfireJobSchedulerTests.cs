using Hangfire;
using Hangfire.InMemory;
using HangfireDynamicScheduler.Domain.Entities;
using HangfireDynamicScheduler.Infrastructure.Scheduling;

namespace HangfireDynamicScheduler.Tests;

public sealed class HangfireJobSchedulerTests
{
    private readonly HangfireJobScheduler _scheduler;

    public HangfireJobSchedulerTests()
    {
        var storage = new InMemoryStorage();
        _scheduler = new HangfireJobScheduler(new RecurringJobManager(storage), storage);
    }

    [Fact]
    public async Task ScheduleAsync_RegistersTheJobWithItsNextRun()
    {
        await _scheduler.ScheduleAsync(new JobConfiguration { JobId = "SampleJob", CronExpression = "*/5 * * * *" }, CancellationToken.None);

        Assert.Equal("SampleJob", Assert.Single(await _scheduler.GetScheduledJobIdsAsync(CancellationToken.None)));
        var nextRun = await _scheduler.GetNextRunAsync("SampleJob", CancellationToken.None);
        Assert.NotNull(nextRun);
        Assert.Equal(DateTimeKind.Utc, nextRun.Value.Kind);
        Assert.Equal(0, nextRun.Value.Minute % 5);
    }

    [Fact]
    public async Task UnscheduleAsync_RemovesTheJob()
    {
        await _scheduler.ScheduleAsync(new JobConfiguration { JobId = "SampleJob", CronExpression = "* * * * *" }, CancellationToken.None);

        await _scheduler.UnscheduleAsync("SampleJob", CancellationToken.None);

        Assert.Empty(await _scheduler.GetScheduledJobIdsAsync(CancellationToken.None));
        Assert.Null(await _scheduler.GetNextRunAsync("SampleJob", CancellationToken.None));
    }

    [Theory]
    [InlineData("* * * * *", true)]
    [InlineData("*/5 9-18 * * 1-5", true)]
    [InlineData("0 * * ? * *", false)]
    [InlineData("61 * * * *", false)]
    [InlineData("every minute", false)]
    public void IsValidCron_AcceptsTheStandardFiveFieldSyntax(string cronExpression, bool expected) =>
        Assert.Equal(expected, _scheduler.IsValidCron(cronExpression));
}
