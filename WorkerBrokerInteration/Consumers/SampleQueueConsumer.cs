using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WorkerBrokerInteration.Configurations;
using WorkerBrokerInteration.Domain.Models;
using WorkerBrokerInteration.Infrastructure.Broker;

namespace WorkerBrokerInteration.Consumers
{
    /// <summary>
    /// Sample queue consumer demonstrating how to implement the EventConsumerBase pattern.
    /// 
    /// This consumer listens to the SAMPLE_QUEUE_EVENT queue and processes incoming messages.
    /// Replace or extend this with your own business logic.
    /// 
    /// To add a new consumer:
    /// 1. Copy this class and rename it
    /// 2. Change the QueueEventType in the constructor
    /// 3. Override ExecuteProcess with your logic
    /// 4. Add the new QueueEventType value to the enum in MessageModels.cs
    /// 5. Register it in Program.cs: builder.Services.AddHostedService&lt;YourConsumer&gt;()
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

        protected override bool ExecuteProcess(MessageEventModel messageEvent)
        {
            using var scope = _scopeFactory.CreateScope();
            try
            {
                _logger.LogInformation(
                    "[{QueueName}] Processing message: {Message}",
                    _queueName, messageEvent.Message);

                // TODO: Replace with your business logic.
                // Example: resolve a scoped service and process the message
                //
                // var myService = scope.ServiceProvider.GetRequiredService<IMyService>();
                // myService.Process(messageEvent.Message);

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
