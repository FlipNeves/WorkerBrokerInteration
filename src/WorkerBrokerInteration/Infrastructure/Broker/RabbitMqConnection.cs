using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using WorkerBrokerInteration.Configurations;

namespace WorkerBrokerInteration.Infrastructure.Broker;

public sealed class RabbitMqConnection : IAsyncDisposable
{
    private readonly MessageBrokerOptions _options;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private IConnection? _connection;

    public RabbitMqConnection(IOptions<MessageBrokerOptions> options)
    {
        _options = options.Value;
    }

    public async Task<IConnection> GetAsync(CancellationToken cancellationToken)
    {
        if (_connection is not null)
            return _connection;

        await _lock.WaitAsync(cancellationToken);
        try
        {
            return _connection ??= await CreateFactory().CreateConnectionAsync(cancellationToken);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
            await _connection.DisposeAsync();

        _lock.Dispose();
    }

    private ConnectionFactory CreateFactory() => new()
    {
        HostName = _options.Host,
        Port = _options.Port,
        UserName = _options.User,
        Password = _options.Password,
        VirtualHost = _options.VirtualHost,
        ConsumerDispatchConcurrency = _options.ConsumerConcurrency
    };
}
