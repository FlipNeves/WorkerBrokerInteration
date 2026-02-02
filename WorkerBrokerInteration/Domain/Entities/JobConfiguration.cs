namespace WorkerBrokerInteration.Domain.Entities
{
    public class JobConfiguration
    {
        public long Id { get; set; }
        public string JobId { get; set; } = string.Empty;
        /// <summary>
        /// Full type name (namespace + class) of the implementation of IJob.
        /// </summary>
        public string JobTypeName { get; set; } = string.Empty;
        public string CronExpression { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime? LastExecutedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
