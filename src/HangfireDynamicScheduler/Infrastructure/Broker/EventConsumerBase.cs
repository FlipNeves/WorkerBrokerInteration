using System.Text.Json;
using HangfireDynamicScheduler.Configurations;
using HangfireDynamicScheduler.Domain.Models;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace HangfireDynamicScheduler.Infrastructure.Broker;

public abstract class EventConsumerBase : BackgroundService
{
    private readonly string _queue;
    private readonly RabbitMqConnection _connection;
    private readonly MessageBrokerOptions _options;
    private IChannel? _channel;

    protected EventConsumerBase(
        QueueEventType queue,
        RabbitMqConnection connection,
        IOptions<MessageBrokerOptions> options,
        ILogger logger)
    {
        _queue = queue.ToString();
        _connection = connection;
        _options = options.Value;
        Logger = logger;
    }

    protected ILogger Logger { get; }

    protected abstract Task HandleAsync(MessageEventModel message, CancellationToken cancellationToken);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await StartConsumingAsync(stoppingToken);
                return;
            }
            catch (BrokerUnreachableException)
            {
                Logger.LogWarning("RabbitMQ is unreachable, queue {Queue} will retry in {Delay}", _queue, _options.ReconnectDelay);
            }

            await Task.Delay(_options.ReconnectDelay, stoppingToken);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_channel is { IsOpen: true })
            await _channel.CloseAsync(cancellationToken);

        await base.StopAsync(cancellationToken);
    }

    private async Task StartConsumingAsync(CancellationToken stoppingToken)
    {
        var connection = await _connection.GetAsync(stoppingToken);
        _channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await _channel.QueueDeclareAsync(_queue, durable: true, exclusive: false, autoDelete: false, cancellationToken: stoppingToken);
        await _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: _options.ConsumerConcurrency, global: false, stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += OnReceivedAsync;
        await _channel.BasicConsumeAsync(_queue, autoAck: false, consumer, stoppingToken);

        Logger.LogInformation("Consuming queue {Queue}", _queue);
    }

    private async Task OnReceivedAsync(object sender, BasicDeliverEventArgs delivery)
    {
        var channel = ((AsyncEventingBasicConsumer)sender).Channel;

        try
        {
            var message = JsonSerializer.Deserialize<MessageEventModel>(delivery.Body.Span)
                ?? throw new JsonException("The message body is empty.");

            await HandleAsync(message, delivery.CancellationToken);
            await channel.BasicAckAsync(delivery.DeliveryTag, multiple: false);
        }
        catch (Exception exception)
        {
            Logger.LogError(exception, "Message {DeliveryTag} from queue {Queue} failed and was rejected", delivery.DeliveryTag, _queue);
            await channel.BasicNackAsync(delivery.DeliveryTag, multiple: false, requeue: false);
        }
    }
}
