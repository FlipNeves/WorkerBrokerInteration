using Microsoft.EntityFrameworkCore;
using WorkerBrokerInteration.Domain.Interfaces;
using WorkerBrokerInteration.Infrastructure.Broker;
using WorkerBrokerInteration.Infrastructure.Persistence;
using WorkerBrokerInteration.Services;

namespace WorkerBrokerInteration.Configurations;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<JobsDbContext>(options => options.UseSqlite(configuration.GetConnectionString("Jobs")));
        services.AddScoped<IJobConfigurationRepository, JobConfigurationRepository>();
        return services;
    }

    public static IServiceCollection AddBroker(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MessageBrokerOptions>(configuration.GetSection(MessageBrokerOptions.Section));
        services.AddSingleton<RabbitMqConnection>();
        services.AddSingleton<IEventPublisher, EventPublisher>();
        return services;
    }

    public static IServiceCollection AddJobScheduling(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SchedulerOptions>(configuration.GetSection(SchedulerOptions.Section));
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<JobCatalog>();
        services.AddScoped<JobRunner>();
        services.AddScoped<JobSynchronizer>();
        services.AddHostedService<JobSynchronizationService>();
        return services;
    }

    public static IServiceCollection AddJob<TJob>(this IServiceCollection services) where TJob : class, IJob
    {
        services.AddScoped<TJob>();
        services.AddSingleton(new JobRegistration(typeof(TJob).Name, typeof(TJob)));
        return services;
    }

    public static async Task EnsureJobsDatabaseAsync(this IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<JobsDbContext>().Database.EnsureCreatedAsync();
    }
}
