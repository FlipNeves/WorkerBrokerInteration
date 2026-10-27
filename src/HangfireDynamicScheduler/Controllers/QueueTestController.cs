using System.Text.Json;
using HangfireDynamicScheduler.Domain.Interfaces;
using HangfireDynamicScheduler.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace HangfireDynamicScheduler.Controllers;

[ApiController]
[Route("api/queue")]
public sealed class QueueTestController : ControllerBase
{
    private const QueueEventType Queue = QueueEventType.SampleQueueEvent;

    private readonly IEventPublisher _publisher;

    public QueueTestController(IEventPublisher publisher)
    {
        _publisher = publisher;
    }

    [HttpPost("send")]
    public async Task<IActionResult> Send([FromQuery] string message, [FromQuery] bool reprocess, CancellationToken cancellationToken)
    {
        await _publisher.SendAsync(new MessageRequestModel(Queue, MessageType.ResourceId, message, reprocess), cancellationToken);
        return Accepted();
    }

    [HttpPost("send-body")]
    public async Task<IActionResult> SendBody([FromBody] JsonElement body, CancellationToken cancellationToken)
    {
        await _publisher.SendAsync(new MessageRequestModel(Queue, MessageType.ResourceBody, body.GetRawText()), cancellationToken);
        return Accepted();
    }

    [HttpPost("send-batch")]
    public async Task<IActionResult> SendBatch([FromBody] IReadOnlyList<string> messages, CancellationToken cancellationToken)
    {
        if (messages.Count == 0)
        {
            ModelState.AddModelError(nameof(messages), "Send at least one message.");
            return ValidationProblem(ModelState);
        }

        var requests = messages.Select(message => new MessageRequestModel(Queue, MessageType.ResourceId, message));
        await _publisher.SendAllAsync(requests, cancellationToken);
        return Accepted();
    }
}
