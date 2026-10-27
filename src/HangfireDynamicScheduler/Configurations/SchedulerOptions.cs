namespace HangfireDynamicScheduler.Configurations;

public sealed class SchedulerOptions
{
    public const string Section = "Scheduler";

    public TimeSpan SyncInterval { get; set; } = TimeSpan.FromMinutes(1);
    public int WorkerCount { get; set; } = 5;
}
