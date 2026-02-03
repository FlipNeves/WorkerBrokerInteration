# Dynamic Job Scheduler with Hangfire

A template project demonstrating how to build a **dynamic job scheduling system** using Hangfire, where jobs are fully controlled through database configuration.

## ✨ Key Features

- **Database-Driven Jobs** — Jobs are configured entirely in the database, no code changes needed
- **Auto-Sync with Hangfire** — Active jobs are registered, inactive jobs are automatically removed
- **DI Container Integration** — Jobs are resolved through dependency injection
- **Type Discovery** — Jobs are discovered by type name using reflection
- **Execution Tracking** — Last execution timestamp is recorded for each job

## 🏗️ Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                        Hangfire                                 │
│  ┌─────────────────┐    ┌─────────────────┐                     │
│  │  JobScheduler   │───▶│   SampleJob     │                    │
│  │  (runs on cron) │    │   AnotherJob    │                     │
│  └────────┬────────┘    └─────────────────┘                     │
└───────────┼─────────────────────────────────────────────────────┘
            │
            ▼
┌─────────────────────────────────────────────────────────────────┐
│                    JobSchedulerService                          │
│  • GetAllActiveAsync() → Register jobs in Hangfire              │
│  • GetAllInactiveJobIdsAsync() → Remove jobs from Hangfire      │
│  • ExecuteJobAsync() → Run job & update last execution          │
└───────────┬─────────────────────────────────────────────────────┘
            │
            ▼
┌─────────────────────────────────────────────────────────────────┐
│                    Database (JobConfiguration)                  │
│  ┌──────┬────────────┬─────────────┬────────────┬──────────┐    │
│  │  Id  │   JobId    │ JobTypeName │    Cron    │ IsActive │    │
│  ├──────┼────────────┼─────────────┼────────────┼──────────┤    │
│  │  1   │ SampleJob  │ ...SampleJob│ */5 * * * *│   true   │    │
│  │  2   │ OtherJob   │ ...OtherJob │ 0 * * * *  │   true   │    │
│  │  3   │ DisabledJob│ ...Disabled │ 0 0 * * *  │  false   │    │
│  └──────┴────────────┴─────────────┴────────────┴──────────┘    │
└─────────────────────────────────────────────────────────────────┘
```

## 📁 Project Structure

```
WorkerBrokerInteration/
├── Domain/
│   ├── Entities/
│   │   └── JobConfiguration.cs       # Database entity
│   └── Interfaces/
│       ├── IJob.cs                   # Base interface for all jobs
│       └── IJobConfigurationRepository.cs
├── Infrastructure/
│   └── Repositories/
│       └── JobConfigurationRepository.cs  # Mock repository (replace with EF Core)
├── Services/
│   ├── Interfaces/
│   │   └── IJobSchedulerService.cs
│   └── JobSchedulerService.cs        # Core sync logic
├── Jobs/
│   ├── SampleJob.cs                  # Example job implementation
│   └── AnotherSampleJob.cs
├── HangFireBasics/
│   ├── CreaterJobs.cs                # Registers the scheduler
│   └── HangFireDashboardFilter.cs    # Dashboard authentication
└── Program.cs
```

## 🚀 Quick Start

### 1. Clone and Run

```bash
git clone https://github.com/FlipNeves/WorkerBrokerInteration
cd WorkerBrokerInteration/WorkerBrokerInteration
dotnet run
```

### 2. Access Hangfire Dashboard

Navigate to `https://localhost:{PORT}/jobs`

**Credentials:** `admin` / `123`

## 📝 How to Add a New Job

### Step 1: Create the Job Class

```csharp
public class MyNewJob : IJob
{
    private readonly ILogger<MyNewJob> _logger;

    public string JobId => "MyNewJob";  // Must match database JobId

    public MyNewJob(ILogger<MyNewJob> logger)
    {
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[{JobId}] Starting...", JobId);
        
        // Your job logic here
        await Task.Delay(1000, cancellationToken);
        
        _logger.LogInformation("[{JobId}] Completed.", JobId);
    }
}
```

### Step 2: Register in DI Container (Program.cs)

```csharp
builder.Services.AddScoped<MyNewJob>();
```

### Step 3: Add Database Configuration

Add to `JobConfigurationRepository.cs` (or your actual database):

```csharp
new JobConfiguration
{
    Id = 4,
    JobId = "MyNewJob",
    JobTypeName = "WorkerBrokerInteration.Jobs.MyNewJob",
    CronExpression = "0 */2 * * *",  // Every 2 hours
    IsActive = true,
    CreatedAt = DateTime.UtcNow,
    UpdatedAt = DateTime.UtcNow
}
```

That's it! The job will be automatically registered in Hangfire on the next sync cycle.

## 🔄 How Job Sync Works

The `JobSchedulerService` runs periodically (controlled by `ConfigHangFire:CronExpression`) and:

1. **Fetches active jobs** from the database
2. **Registers/updates** each job in Hangfire using `AddOrUpdate`
3. **Fetches inactive job IDs** from the database
4. **Removes** those jobs from Hangfire using `RemoveIfExists`

```
[JobScheduler] Starting job synchronization...
[JobScheduler] Job registered/updated: SampleJob | Cron: */5 * * * *
[JobScheduler] Job registered/updated: AnotherSampleJob | Cron: 0 * * * *
[JobScheduler] Job removed from Hangfire: InactiveJob
[JobScheduler] Job synchronization completed.
```

## ⚙️ Configuration

### appsettings.json

```json
{
  "ConfigHangFire": {
    "WorkerCount": 5,
    "SemaphoreTimeOutInMinutes": 30,
    "DashboardAuth": {
      "Username": "admin",
      "Password": "your-secure-password"
    },
    "CronExpression": "* * * * *"  // How often to sync jobs
  }
}
```

### Cron Expression Examples

| Expression | Description |
|------------|-------------|
| `* * * * *` | Every minute |
| `*/5 * * * *` | Every 5 minutes |
| `0 * * * *` | Every hour |
| `0 0 * * *` | Daily at midnight |
| `0 0 * * 0` | Weekly on Sunday |

## 🔧 Production Considerations

### Replace Mock Repository

The current `JobConfigurationRepository` uses an in-memory list. For production:

1. Add Entity Framework Core packages
2. Create `AppDbContext` with `DbSet<JobConfiguration>`
3. Implement repository using EF Core
4. Add connection string to `appsettings.json`

### Use Persistent Storage

Replace Hangfire's MemoryStorage with a persistent option:

```csharp
// SQL Server
builder.Services.AddHangfire(config => 
    config.UseSqlServerStorage(connectionString));

// Redis
builder.Services.AddHangfire(config => 
    config.UseRedisStorage(connectionString));
```

## 📄 License

MIT License - feel free to use this template in your projects.

## 🤝 Contributing

Contributions are welcome! Feel free to open issues or submit pull requests.
