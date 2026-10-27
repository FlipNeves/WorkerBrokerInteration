using System.Text.Json;
using HangfireDynamicScheduler.Domain.Interfaces;
using HangfireDynamicScheduler.Domain.Models;

namespace HangfireDynamicScheduler.Jobs;

public sealed class SampleJob : IJob
{
    private readonly IEventPublisher _publisher;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<SampleJob> _logger;

    public SampleJob(IEventPublisher publisher, TimeProvider timeProvider, ILogger<SampleJob> logger)
    {
        _publisher = publisher;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);

        var report = JsonSerializer.Serialize(new { Job = nameof(SampleJob), FinishedAt = _timeProvider.GetUtcNow() });
        await _publisher.SendAsync(new MessageRequestModel(QueueEventType.SampleQueueEvent, MessageType.ResourceBody, report), cancellationToken);

        _logger.LogInformation("{Job} published its report to {Queue}", nameof(SampleJob), QueueEventType.SampleQueueEvent);
    }
}
