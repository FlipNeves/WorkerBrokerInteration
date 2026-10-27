namespace HangfireDynamicScheduler.Tests.Support;

public sealed class FixedTimeProvider : TimeProvider
{
    public static readonly DateTimeOffset Now = new(2026, 11, 2, 9, 30, 0, TimeSpan.Zero);

    public override DateTimeOffset GetUtcNow() => Now;
}
