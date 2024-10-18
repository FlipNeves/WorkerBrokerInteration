# WorkerBrokerInteraction

WorkerBrokerInteraction is a study project that demonstrates how to manage and schedule background jobs in .NET using Hangfire. The project explores concepts such as job scheduling, recurring jobs, job execution management, and worker services integration, all in a structured and modular way.

## Table of Contents

- Overview
- Features
- Technologies
- Getting Started
  - Prerequisites
  - Installation
  - Usage
    - Adding New Jobs
    - Managing Job Execution
  - Configuration

## Overview

The WorkerBrokerInteraction project is designed to demonstrate the following key concepts:

- Background job processing using Hangfire.
- Recurring job scheduling and management.
- Best practices for managing job lifecycle, including checking if a job is running before scheduling it.
- Modular architecture with clear separation between job definitions, scheduling, and execution.

This project serves as a learning tool for developers who want to explore job scheduling and worker management in .NET.

## Features

- Background Job Scheduling: Uses Hangfire for easy and efficient job scheduling.
- Recurring Jobs: Allows the configuration of jobs to run at specific intervals using CRON expressions.
- Job Management: Ensures jobs do not overlap by checking if they are already running before starting a new instance.
- Flexible Job Definitions: Jobs are added and updated dynamically through configuration.

## Technologies

- .NET Core: Backend framework.
- Hangfire: Library for background job processing.
- RabbitMQ (optional): Can be used for message brokering and communication between services (if extended).
- Dependency Injection: Follows DI principles to manage services and job configurations.

## Getting Started

### Prerequisites

To run the project, ensure you have the following installed:

- .NET Core SDK (version 6.0 or later)
- Hangfire (included as a dependency)

### Installation

1. Clone the repository:

   git clone https://github.com/yourusername/WorkerBrokerInteraction.git
   cd WorkerBrokerInteraction

2. Install dependencies: Restore the required NuGet packages:

   dotnet restore

3. Run the application:

   dotnet run

## Usage

### Adding New Jobs

To add new jobs to the scheduling system, follow these steps:

1. Define the Job: In the Execute method of the JobSchedulerService, add the new job to the jobDefinitions list:

```cs
   var jobDefinitions = new List<(Type jobType, string jobId)>
   {
       (typeof(YourNewJobType), RecurringJobsIds.YourNewJobId)
   };
```

2. Configure the CRON expression: Extend the GetCronExpression method to return a CRON expression for the new job:

```cs
   private string GetCronExpression(Type jobType) => jobType switch
   {
       var t when t == typeof(YourNewJobType) => _configurationsService.GetCronYourNewJob(),
       _ => throw new NotSupportedException($"Unsupported job type: {jobType.Name}")
   };
```

3. Update the Configuration: Ensure the new job's configuration is included in your configuration service or appsettings.json.

### Managing Job Execution

Before scheduling a job, the system checks whether an instance of the job is already running to prevent overlap. This is done using the IsJobRunning method:

```cs
private async Task<bool> IsJobRunning(Type jobType)
{
    var runningJobs = JobStorage.Current.GetMonitoringApi().ProcessingJobs(0, int.MaxValue);
    return runningJobs.Any(job => job.Value.Job.Type == jobType);
}
```

This ensures that jobs are not scheduled concurrently and that system resources are efficiently managed.

## Configuration

The project uses a ConfigurationsService to manage job settings and CRON expressions. You can extend this service to provide custom job configurations and define schedules for your jobs.

Example Configuration:

```json
"ConfigHangFire": {
  "WorkerCount": 5,
  "SemaphoreTimeOutInMinutes": 30,
  "DashboardAuth": {
    "Username": "admin",
    "Password": "123"
  },
  "CronExpression": "0 * * * *"
}
```
