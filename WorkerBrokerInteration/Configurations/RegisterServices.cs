using WorkerBrokerInteration.Consumers;
using WorkerBrokerInteration.Domain.Interfaces;
using WorkerBrokerInteration.Infrastructure.Broker;

namespace WorkerBrokerInteration.Configurations
{
    public static class RegisterServices
    {
        /// <summary>
        /// Registers message broker services (publisher + consumers).
        /// </summary>
        public static IServiceCollection AddBrokerServices(this IServiceCollection services)
        {
            // Publisher
            services.AddScoped<IEventPublisher, EventPublisher>();

            // Queue Consumers (BackgroundServices)
            services.AddHostedService<SampleQueueConsumer>();

            return services;
        }
    }
}
