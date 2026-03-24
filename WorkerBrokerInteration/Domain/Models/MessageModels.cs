namespace WorkerBrokerInteration.Domain.Models
{
    /// <summary>
    /// Model representing a message received from a RabbitMQ queue.
    /// Contains metadata about the message along with its payload.
    /// </summary>
    public class MessageEventModel
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public MessageType MessageType { get; set; } = MessageType.RESOURCE_UNIQUE_ID;
        public string Message { get; set; } = string.Empty;
        public bool Reprocess { get; set; } = false;
    }

    /// <summary>
    /// Model used to request publishing a message to a RabbitMQ queue.
    /// The caller specifies the target queue and message payload.
    /// </summary>
    public class MessageRequestModel
    {
        public QueueEventType QueueEvent { get; set; }
        public MessageType MessageType { get; set; } = MessageType.RESOURCE_UNIQUE_ID;
        public string Message { get; set; } = string.Empty;
        public bool Reprocess { get; set; }
    }

    /// <summary>
    /// Available queue types in the system.
    /// Each value maps to a RabbitMQ queue name (using ToString()).
    /// Add new entries here when creating new queue consumers.
    /// </summary>
    public enum QueueEventType
    {
        SAMPLE_QUEUE_EVENT
    }

    /// <summary>
    /// Describes the format/type of the message payload.
    /// </summary>
    public enum MessageType
    {
        /// <summary>A unique resource identifier (e.g., database ID).</summary>
        RESOURCE_UNIQUE_ID,

        /// <summary>A serialized resource body (e.g., JSON object).</summary>
        RESOURCE_BODY,

        /// <summary>A federal document number (e.g., CPF/CNPJ).</summary>
        FEDERAL_DOCUMENT
    }
}
