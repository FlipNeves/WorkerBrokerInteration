using Microsoft.AspNetCore.Mvc;
using WorkerBrokerInteration.Domain.Interfaces;
using WorkerBrokerInteration.Services;

namespace WorkerBrokerInteration.Controllers;

[ApiController]
[Route("api/jobs")]
public sealed class JobsController : ControllerBase
{
    private readonly IJobConfigurationRepository _repository;
    private readonly IJobScheduler _scheduler;
    private readonly JobSynchronizer _synchronizer;

    public JobsController(IJobConfigurationRepository repository, IJobScheduler scheduler, JobSynchronizer synchronizer)
    {
        _repository = repository;
        _scheduler = scheduler;
        _synchronizer = synchronizer;
    }

    [HttpGet]
    public async Task<IReadOnlyList<JobView>> GetAll(CancellationToken cancellationToken)
    {
        var configurations = await _repository.GetAllAsync(cancellationToken);
        var views = new List<JobView>(configurations.Count);

        foreach (var configuration in configurations)
        {
            var nextRunAt = await _scheduler.GetNextRunAsync(configuration.JobId, cancellationToken);
            views.Add(JobView.From(configuration, nextRunAt));
        }

        return views;
    }

    [HttpPut("{jobId}")]
    public async Task<ActionResult<JobView>> UpdateSchedule(string jobId, UpdateScheduleRequest request, CancellationToken cancellationToken)
    {
        if (!_scheduler.IsValidCron(request.CronExpression))
        {
            ModelState.AddModelError(nameof(request.CronExpression), $"'{request.CronExpression}' is not a valid cron expression.");
            return ValidationProblem(ModelState);
        }

        var configuration = await _repository.UpdateScheduleAsync(jobId, request.CronExpression, request.IsActive, cancellationToken);
        if (configuration is null)
            return NotFound();

        await _synchronizer.ApplyAsync(configuration, cancellationToken);

        return JobView.From(configuration, await _scheduler.GetNextRunAsync(jobId, cancellationToken));
    }
}
