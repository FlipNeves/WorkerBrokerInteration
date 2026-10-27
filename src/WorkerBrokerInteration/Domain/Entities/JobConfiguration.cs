namespace WorkerBrokerInteration.Domain.Entities;

public sealed class JobConfiguration
{
    public long Id { get; set; }
    public string JobId { get; set; } = string.Empty;
    public string CronExpression { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime? LastExecutedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
