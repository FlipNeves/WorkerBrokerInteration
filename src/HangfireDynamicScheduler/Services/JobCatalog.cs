namespace HangfireDynamicScheduler.Services;

public sealed record JobRegistration(string JobId, Type JobType);

public sealed class JobCatalog
{
    private readonly Dictionary<string, Type> _jobTypes;

    public JobCatalog(IEnumerable<JobRegistration> registrations)
    {
        _jobTypes = registrations.ToDictionary(registration => registration.JobId, registration => registration.JobType);
    }

    public bool Contains(string jobId) => _jobTypes.ContainsKey(jobId);

    public Type? Find(string jobId) => _jobTypes.GetValueOrDefault(jobId);
}
