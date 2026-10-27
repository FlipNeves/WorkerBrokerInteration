using Hangfire;
using Microsoft.Extensions.Options;
using WorkerBrokerInteration.Configurations;
using WorkerBrokerInteration.Domain.Interfaces;

namespace WorkerBrokerInteration.Infrastructure.Scheduling;

public static class HangfireSetup
{
    private const string DashboardPath = "/jobs";

    public static IServiceCollection AddHangfireScheduler(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<DashboardCredentials>(configuration.GetSection(DashboardCredentials.Section));

        services.AddHangfire(hangfire => hangfire
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseInMemoryStorage());

        services.AddHangfireServer((provider, server) =>
            server.WorkerCount = provider.GetRequiredService<IOptions<SchedulerOptions>>().Value.WorkerCount);

        services.AddSingleton<IJobScheduler, HangfireJobScheduler>();
        return services;
    }

    public static WebApplication UseHangfireDashboardWithBasicAuth(this WebApplication app)
    {
        var credentials = app.Services.GetRequiredService<IOptions<DashboardCredentials>>().Value;

        app.UseHangfireDashboard(DashboardPath, new DashboardOptions
        {
            Authorization = [new BasicAuthDashboardFilter(credentials)]
        });

        return app;
    }
}
