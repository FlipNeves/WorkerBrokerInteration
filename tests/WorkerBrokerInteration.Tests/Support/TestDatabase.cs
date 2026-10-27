using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WorkerBrokerInteration.Infrastructure.Persistence;

namespace WorkerBrokerInteration.Tests.Support;

public sealed class TestDatabase : IDisposable
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    public TestDatabase()
    {
        _connection.Open();
        using var context = CreateContext();
        context.Database.EnsureCreated();
    }

    public JobsDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<JobsDbContext>().UseSqlite(_connection).Options);

    public JobConfigurationRepository CreateRepository() => new(CreateContext(), new FixedTimeProvider());

    public void Dispose() => _connection.Dispose();
}
