using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using WorkerBrokerInteration.Configurations;
using WorkerBrokerInteration.Domain.Interfaces;
using WorkerBrokerInteration.Domain.Models;

namespace WorkerBrokerInteration.Infrastructure.Broker
{
    /// <summary>
    /// Publishes messages to RabbitMQ queues.
    /// Creates a new connection per publish operation — suitable for low-throughput scenarios.
    /// For high-throughput, consider pooling connections.
    /// </summary>
    public class EventPublisher : IEventPublisher
    {
        private readonly ILogger<EventPublisher> _logger;
        private readonly MessageBrokerConfig _messageBrokerConfig;

        public EventPublisher(
            ILogger<EventPublisher> logger,
            IOptions<MessageBrokerConfig> messageBrokerConfig)
        {
            _logger = logger;
            _messageBrokerConfig = messageBrokerConfig.Value;
        }

        private ConnectionFactory GetConnectionFactory(string? virtualHost = null)
        {
            var factory = new ConnectionFactory()
            {
                HostName = _messageBrokerConfig.Host,
                Port = _messageBrokerConfig.Port,
                UserName = _messageBrokerConfig.User,
                Password = _messageBrokerConfig.Password
            };

            var vhost = virtualHost ?? _messageBrokerConfig.VirtualHost;
            if (!string.IsNullOrEmpty(vhost))
            {
                factory.VirtualHost = vhost;
            }

            return factory;
        }

        public async Task SendAsync(MessageRequestModel queueRequest)
        {
            await SendAsync(_messageBrokerConfig.VirtualHost, queueRequest);
        }

        public async Task SendAsync(string virtualHost, MessageRequestModel queueRequest)
        {
            if (queueRequest == null)
                return;

            using var connection = GetConnectionFactory(virtualHost).CreateConnection();
            using var channel = connection.CreateModel();

            await SendInternalAsync(channel, queueRequest);
        }

        public async Task SendAllAsync(List<MessageRequestModel> queueRequests)
        {
            await SendAllAsync(_messageBrokerConfig.VirtualHost, queueRequests);
        }

        public async Task SendAllAsync(string virtualHost, List<MessageRequestModel> queueRequests)
        {
            if (queueRequests == null || !queueRequests.Any())
                return;

            using var connection = GetConnectionFactory(virtualHost).CreateConnection();
            using var channel = connection.CreateModel();

            foreach (var request in queueRequests)
            {
                await SendInternalAsync(channel, request);
            }
        }

        private Task SendInternalAsync(IModel channel, MessageRequestModel queueRequest)
        {
            var queueName = queueRequest.QueueEvent.ToString();

            channel.QueueDeclare(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var messageEvent = new MessageEventModel()
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                Message = queueRequest.Message,
                MessageType = queueRequest.MessageType,
                Reprocess = queueRequest.Reprocess
            };

            var messageData = JsonSerializer.Serialize(messageEvent);
            var body = Encoding.UTF8.GetBytes(messageData);

            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;

            channel.BasicPublish(
                exchange: "",
                routingKey: queueName,
                basicProperties: properties,
                body: body);

            _logger.LogInformation("Message sent to queue {QueueName}: {Message}", queueName, queueRequest.Message);

            return Task.CompletedTask;
        }
    }
}
