namespace HangfireDynamicScheduler.Domain.Interfaces;

public interface IJob
{
    Task ExecuteAsync(CancellationToken cancellationToken);
}
