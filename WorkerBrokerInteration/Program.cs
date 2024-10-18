using Financeiro.Services.Worker;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.Extensions.Options;
using WorkerBrokerInteration.Configurations;
using WorkerBrokerInteration.HangFireBasics;
using WorkerBrokerInteration.Jobs.SampleJobExample;
using WorkerBrokerInteration.Repository;
using WorkerBrokerInteration.Repository.Interfaces;
using WorkerBrokerInteration.Services;
using WorkerBrokerInteration.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

builder.Services.AddControllers();
builder.Services.AddScoped<IConfigurationsRepository, ConfigurationsRepository>();
builder.Services.AddScoped<IConfigurationsService, ConfigurationsService>();
builder.Services.AddScoped<IJobSchedulerService, JobSchedulerService>();
builder.Services.AddScoped<ISimpleJob, JobSampleJobExample>();

builder.Services.Configure<ConfigHangFire>(configuration.GetSection("ConfigHangFire"));

var memoryStorage = new MemoryStorage();
builder.Services.AddHangfire(configuration => configuration
    .UseStorage(memoryStorage));

builder.Services.AddHangfireServer((serviceProvider, options) =>
{
    var configHangfire = serviceProvider.GetRequiredService<IOptions<ConfigHangFire>>().Value;
    options.WorkerCount = configHangfire.WorkerCount;
});

var app = builder.Build();
app.UseHttpsRedirection();

app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.MapDefaultControllerRoute();
});

app.UseHangfireDashboard("/jobs", new DashboardOptions
{
    Authorization = new[]
    {
        new HangFireDashboardFilter(
            builder.Services.BuildServiceProvider().GetRequiredService<IOptions<ConfigHangFire>>()
        )
    }
});
app.UseHangfireServer();

app.UseStaticFiles();

var cronExpression = configuration.GetSection("ConfigHangFire:CronExpression").Value ?? "0 * * * *";
CreaterJobs.RegisterJob(cron: cronExpression);

app.Run();
