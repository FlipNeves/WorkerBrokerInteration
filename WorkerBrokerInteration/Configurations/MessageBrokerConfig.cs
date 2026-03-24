namespace WorkerBrokerInteration.Configurations
{
    /// <summary>
    /// Configuration for connecting to the RabbitMQ message broker.
    /// Mapped from the "MessageBroker" section in appsettings.json.
    /// </summary>
    public class MessageBrokerConfig
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; }
        public string User { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string VirtualHost { get; set; } = string.Empty;
        public ExternalBrokerConfig? ExternalBroker { get; set; }
    }

    /// <summary>
    /// Configuration for external broker virtual hosts.
    /// Allows connecting to different RabbitMQ virtual hosts for external integrations.
    /// </summary>
    public class ExternalBrokerConfig
    {
        public string? VirtualHostExternal { get; set; }
    }

    /// <summary>
    /// Configuration for parallel message processing.
    /// Controls the number of concurrent consumer threads per queue.
    /// </summary>
    public class ParallelProcessingConfig
    {
        public int ThreadsCount { get; set; } = 1;
    }
}
