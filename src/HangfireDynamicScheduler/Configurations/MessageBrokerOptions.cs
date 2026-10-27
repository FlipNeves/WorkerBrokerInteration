namespace HangfireDynamicScheduler.Configurations;

public sealed class MessageBrokerOptions
{
    public const string Section = "MessageBroker";

    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string User { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";
    public ushort ConsumerConcurrency { get; set; } = 1;
    public TimeSpan ReconnectDelay { get; set; } = TimeSpan.FromSeconds(10);
}
