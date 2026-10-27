using WorkerBrokerInteration.Domain.Interfaces;

namespace WorkerBrokerInteration.Tests.Support;

public sealed class RecordingJob : IJob
{
    public int Executions { get; private set; }

    public Task ExecuteAsync(CancellationToken cancellationToken)
    {
        Executions++;
        return Task.CompletedTask;
    }
}

public sealed class FailingJob : IJob
{
    public Task ExecuteAsync(CancellationToken cancellationToken) =>
        throw new InvalidOperationException("The job failed.");
}
