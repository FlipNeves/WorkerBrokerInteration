using WorkerBrokerInteration.Domain.Models;

namespace WorkerBrokerInteration.Domain.Interfaces
{
    /// <summary>
    /// Interface for publishing messages to RabbitMQ queues.
    /// Supports single and batch message sending, with optional virtual host override.
    /// </summary>
    public interface IEventPublisher
    {
        /// <summary>
        /// Publishes a single message to the default virtual host.
        /// </summary>
        Task SendAsync(MessageRequestModel queueRequest);

        /// <summary>
        /// Publishes a single message to a specific virtual host.
        /// </summary>
        Task SendAsync(string virtualHost, MessageRequestModel queueRequest);

        /// <summary>
        /// Publishes multiple messages to the default virtual host.
        /// </summary>
        Task SendAllAsync(List<MessageRequestModel> queueRequests);

        /// <summary>
        /// Publishes multiple messages to a specific virtual host.
        /// </summary>
        Task SendAllAsync(string virtualHost, List<MessageRequestModel> queueRequests);
    }
}
