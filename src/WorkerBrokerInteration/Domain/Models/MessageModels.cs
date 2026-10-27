namespace WorkerBrokerInteration.Domain.Models;

public enum QueueEventType
{
    SampleQueueEvent
}

public enum MessageType
{
    ResourceId,
    ResourceBody,
    FederalDocument
}

public sealed record MessageRequestModel(QueueEventType QueueEvent, MessageType MessageType, string Message, bool Reprocess = false);

public sealed record MessageEventModel(Guid Id, DateTime CreatedAt, MessageType MessageType, string Message, bool Reprocess);
