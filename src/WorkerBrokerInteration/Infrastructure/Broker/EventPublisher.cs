using System.Text.Json;
using RabbitMQ.Client;
using WorkerBrokerInteration.Domain.Interfaces;
using WorkerBrokerInteration.Domain.Models;

namespace WorkerBrokerInteration.Infrastructure.Broker;

public sealed class EventPublisher : IEventPublisher
{
    private readonly RabbitMqConnection _connection;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<EventPublisher> _logger;

    public EventPublisher(RabbitMqConnection connection, TimeProvider timeProvider, ILogger<EventPublisher> logger)
    {
        _connection = connection;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public Task SendAsync(MessageRequestModel request, CancellationToken cancellationToken) =>
        SendAllAsync([request], cancellationToken);

    public async Task SendAllAsync(IEnumerable<MessageRequestModel> requests, CancellationToken cancellationToken)
    {
        var connection = await _connection.GetAsync(cancellationToken);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        foreach (var request in requests)
        {
            var queue = request.QueueEvent.ToString();
            await channel.QueueDeclareAsync(queue, durable: true, exclusive: false, autoDelete: false, cancellationToken: cancellationToken);

            var message = new MessageEventModel(
                Guid.NewGuid(),
                _timeProvider.GetUtcNow().UtcDateTime,
                request.MessageType,
                request.Message,
                request.Reprocess);

            var properties = new BasicProperties
            {
                Persistent = true,
                ContentType = "application/json",
                MessageId = message.Id.ToString()
            };

            await channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: queue,
                mandatory: false,
                basicProperties: properties,
                body: JsonSerializer.SerializeToUtf8Bytes(message),
                cancellationToken: cancellationToken);

            _logger.LogInformation("Message {MessageId} sent to queue {Queue}", message.Id, queue);
        }
    }
}
