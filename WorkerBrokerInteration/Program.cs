using Financeiro.Services.Worker;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.Extensions.Options;
using WorkerBrokerInteration.Configurations;
using WorkerBrokerInteration.Domain.Interfaces;
using WorkerBrokerInteration.HangFireBasics;
using WorkerBrokerInteration.Infrastructure.Repositories;
using WorkerBrokerInteration.Jobs;
using WorkerBrokerInteration.Services;
using WorkerBrokerInteration.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

// Controllers
builder.Services.AddControllers();

// Hangfire Configurations
builder.Services.Configure<ConfigHangFire>(configuration.GetSection("ConfigHangFire"));

builder.Services.AddScoped<IJobConfigurationRepository, JobConfigurationRepository>();

builder.Services.AddScoped<IJobSchedulerService, JobSchedulerService>();

// Add all jobs to DI 
builder.Services.AddScoped<SampleJob>();
builder.Services.AddScoped<AnotherSampleJob>();

builder.Services.AddScoped<IJob, SampleJob>();
builder.Services.AddScoped<IJob, AnotherSampleJob>();

// Hangfire storage in memory (in production, use SQL Server, Redis, another one.)
var memoryStorage = new MemoryStorage();
builder.Services.AddHangfire(config => config.UseStorage(memoryStorage));

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

// HangFire's Dashboard
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

// Initialize Job Scheduler
var cronExpression = configuration.GetSection("ConfigHangFire:CronExpression").Value ?? "10 * * * *";
CreaterJobs.RegisterJobScheduler(cronExpression);

app.Run();
