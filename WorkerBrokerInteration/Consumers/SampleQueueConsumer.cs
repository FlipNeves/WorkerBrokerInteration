using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WorkerBrokerInteration.Configurations;
using WorkerBrokerInteration.Domain.Interfaces;
using WorkerBrokerInteration.Domain.Models;
using WorkerBrokerInteration.Infrastructure.Broker;

namespace WorkerBrokerInteration.Consumers
{
    /// <summary>
    /// Sample queue consumer that listens to the SAMPLE_QUEUE_EVENT queue.
    /// 
    /// Demonstrates:
    /// - How to inherit from EventConsumerBase
    /// - How to use IServiceScopeFactory to resolve scoped services
    /// - How to handle different MessageTypes
    /// - How to republish messages (for retry/reprocessing)
    /// </summary>
    public class SampleQueueConsumer : EventConsumerBase
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public SampleQueueConsumer(
            IServiceScopeFactory scopeFactory,
            ILogger<SampleQueueConsumer> logger,
            IOptions<MessageBrokerConfig> messageBrokerConfig,
            IOptions<ParallelProcessingConfig> parallelProcessingConfig)
            : base(
                QueueEventType.SAMPLE_QUEUE_EVENT.ToString(),
                logger,
                messageBrokerConfig,
                parallelProcessingConfig?.Value?.ThreadsCount ?? 1)
        {
            _scopeFactory = scopeFactory;
        }

        /// <summary>
        /// Processes messages received from the SAMPLE_QUEUE_EVENT queue.
        /// 
        /// This method is called automatically by EventConsumerBase
        /// every time a message arrives in the queue.
        /// </summary>
        protected override bool ExecuteProcess(MessageEventModel messageEvent)
        {
            using var scope = _scopeFactory.CreateScope();
            try
            {
                _logger.LogInformation(
                    "[{QueueName}] ==============================", _queueName);
                _logger.LogInformation(
                    "[{QueueName}] Message received!", _queueName);
                _logger.LogInformation(
                    "[{QueueName}]   ID:          {Id}", _queueName, messageEvent.Id);
                _logger.LogInformation(
                    "[{QueueName}]   Created at:  {CreatedAt}", _queueName, messageEvent.CreatedAt);
                _logger.LogInformation(
                    "[{QueueName}]   Type:        {MessageType}", _queueName, messageEvent.MessageType);
                _logger.LogInformation(
                    "[{QueueName}]   Reprocess:   {Reprocess}", _queueName, messageEvent.Reprocess);
                _logger.LogInformation(
                    "[{QueueName}]   Payload:     {Message}", _queueName, messageEvent.Message);
                _logger.LogInformation(
                    "[{QueueName}] ==============================", _queueName);

                // -------------------------------------------------------
                // Place your business logic here.
                // Examples:
                //
                // 1. If the message is a resource ID:
                //    var service = scope.ServiceProvider.GetRequiredService<IMyService>();
                //    service.ProcessById(messageEvent.Message);
                //
                // 2. If the message is a JSON body:
                //    var dto = JsonSerializer.Deserialize<MyDto>(messageEvent.Message);
                //    service.Process(dto);
                //
                // 3. If it's a reprocess / retry:
                //    if (messageEvent.Reprocess)
                //    {
                //        _logger.LogWarning("Reprocessing message {Id}", messageEvent.Id);
                //        // retry logic
                //    }
                // -------------------------------------------------------

                // Example: handle by MessageType
                switch (messageEvent.MessageType)
                {
                    case MessageType.RESOURCE_UNIQUE_ID:
                        _logger.LogInformation(
                            "[{QueueName}] Processing resource by ID: {ResourceId}",
                            _queueName, messageEvent.Message);
                        break;

                    case MessageType.RESOURCE_BODY:
                        _logger.LogInformation(
                            "[{QueueName}] Processing JSON body: {Body}",
                            _queueName, messageEvent.Message);
                        break;

                    case MessageType.FEDERAL_DOCUMENT:
                        _logger.LogInformation(
                            "[{QueueName}] Processing federal document: {Document}",
                            _queueName, messageEvent.Message);
                        break;
                }

                // Example: re-enqueue if reprocessing is needed
                if (messageEvent.Reprocess)
                {
                    _logger.LogWarning(
                        "[{QueueName}] Reprocess flag is active! " +
                        "In a real scenario, you could re-enqueue the message here.",
                        _queueName);

                    // Example of how to re-enqueue:
                    // var publisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();
                    // await publisher.SendAsync(new MessageRequestModel
                    // {
                    //     QueueEvent = QueueEventType.SAMPLE_QUEUE_EVENT,
                    //     Message = messageEvent.Message,
                    //     Reprocess = true
                    // });
                }

                _logger.LogInformation(
                    "[{QueueName}] Message processed successfully: {MessageId}",
                    _queueName, messageEvent.Id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "[{QueueName}] Error processing message: {Message}",
                    _queueName, messageEvent.Message);
                return false;
            }
        }
    }
}
