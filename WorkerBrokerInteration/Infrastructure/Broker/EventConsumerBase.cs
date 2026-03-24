using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using WorkerBrokerInteration.Configurations;
using WorkerBrokerInteration.Domain.Models;

namespace WorkerBrokerInteration.Infrastructure.Broker
{
    /// <summary>
    /// Abstract base class for RabbitMQ queue consumers.
    /// Implements BackgroundService to run as a hosted service.
    /// 
    /// To create a new consumer:
    /// 1. Create a class inheriting from EventConsumerBase
    /// 2. Pass the queue name (QueueEventType) in the constructor
    /// 3. Override ExecuteProcess(MessageEventModel) with your business logic
    /// 4. Register it in Program.cs with builder.Services.AddHostedService&lt;YourConsumer&gt;()
    /// </summary>
    public abstract class EventConsumerBase : BackgroundService
    {
        protected readonly int _countThreads;
        protected readonly ILogger<EventConsumerBase> _logger;
        protected readonly string _queueName;
        protected readonly MessageBrokerConfig _messageBrokerConfig;

        protected ConnectionFactory? _factory;
        protected IConnection? _connection;
        protected IModel? _channel;
        protected string _virtualHost;

        protected EventConsumerBase(
            string queueName,
            ILogger<EventConsumerBase> logger,
            IOptions<MessageBrokerConfig> messageBrokerConfig,
            int threadsCount = 1)
        {
            _queueName = queueName;
            _countThreads = threadsCount;
            _messageBrokerConfig = messageBrokerConfig.Value;
            _logger = logger;
            _virtualHost = _messageBrokerConfig.VirtualHost;
        }

        /// <summary>
        /// Allows overriding the virtual host for consumers that connect to external brokers.
        /// </summary>
        protected void SetVirtualHost(string virtualHost)
        {
            _virtualHost = virtualHost;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting consumer for queue [{QueueName}]", _queueName);

            _factory = new ConnectionFactory()
            {
                HostName = _messageBrokerConfig.Host,
                Port = _messageBrokerConfig.Port,
                UserName = _messageBrokerConfig.User,
                Password = _messageBrokerConfig.Password,
                ConsumerDispatchConcurrency = (ushort)_countThreads,
                AutomaticRecoveryEnabled = true
            };

            if (!string.IsNullOrEmpty(_virtualHost))
            {
                _factory.VirtualHost = _virtualHost;
            }

            _connection = _factory.CreateConnection();
            _connection.ConnectionShutdown += OnConnectionShutdown;

            _channel = _connection.CreateModel();
            _channel.QueueDeclare(
                queue: _queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            _logger.LogInformation("Consumer started for queue [{QueueName}]", _queueName);

            return base.StartAsync(cancellationToken);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                var consumer = new EventingBasicConsumer(_channel!);
                consumer.Received += OnMessageReceived;

                _channel!.BasicConsume(
                    queue: _queueName,
                    autoAck: false,
                    consumer: consumer);

                _logger.LogInformation("Consuming messages from queue [{QueueName}]", _queueName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating consumer for queue [{QueueName}]", _queueName);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Handles incoming messages from RabbitMQ.
        /// Deserializes the message and delegates to ExecuteProcess.
        /// Always acknowledges the message (even on failure) to prevent infinite redelivery.
        /// </summary>
        protected virtual void OnMessageReceived(object? sender, BasicDeliverEventArgs eventArgs)
        {
            var rawMessage = "Message payload not processed";
            try
            {
                rawMessage = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
                var messageEvent = JsonSerializer.Deserialize<MessageEventModel>(rawMessage);

                if (messageEvent == null)
                {
                    _logger.LogWarning("Received null message from queue [{QueueName}]", _queueName);
                    return;
                }

                _logger.LogInformation(
                    "Message received on [{QueueName}]: {Message} | Reprocess: {Reprocess}",
                    _queueName, messageEvent.Message, messageEvent.Reprocess);

                ExecuteProcess(messageEvent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message from queue [{QueueName}]: {RawMessage}", _queueName, rawMessage);
            }
            finally
            {
                _channel?.BasicAck(eventArgs.DeliveryTag, false);
            }
        }

        /// <summary>
        /// Override this method to implement your business logic for processing queue messages.
        /// </summary>
        /// <param name="messageEvent">The deserialized message from the queue.</param>
        /// <returns>True if processing succeeded, false otherwise.</returns>
        protected virtual bool ExecuteProcess(MessageEventModel messageEvent)
        {
            _logger.LogWarning("No action registered for queue [{QueueName}]", _queueName);
            return false;
        }

        private void OnConnectionShutdown(object? sender, ShutdownEventArgs e)
        {
            _logger.LogError("Connection shutdown for queue [{QueueName}]: {ReplyText}", _queueName, e.ReplyText);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogWarning("Stopping consumer for queue [{QueueName}]", _queueName);

            _channel?.Close();
            _connection?.Close();

            await base.StopAsync(cancellationToken);

            _logger.LogWarning("Consumer stopped for queue [{QueueName}]", _queueName);
        }

        public override void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
            base.Dispose();
        }
    }
}
