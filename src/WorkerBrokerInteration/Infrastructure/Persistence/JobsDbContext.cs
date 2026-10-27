using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using WorkerBrokerInteration.Domain.Entities;

namespace WorkerBrokerInteration.Infrastructure.Persistence;

public sealed class JobsDbContext : DbContext
{
    public JobsDbContext(DbContextOptions<JobsDbContext> options) : base(options)
    {
    }

    public DbSet<JobConfiguration> JobConfigurations => Set<JobConfiguration>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<JobConfiguration>(entity =>
        {
            entity.HasIndex(job => job.JobId).IsUnique();
            entity.Property(job => job.JobId).HasMaxLength(100);
            entity.Property(job => job.CronExpression).HasMaxLength(100);
            entity.HasData(JobConfigurationSeed.Jobs);
        });
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
    }

    private sealed class UtcDateTimeConverter : ValueConverter<DateTime, DateTime>
    {
        public UtcDateTimeConverter()
            : base(value => value.ToUniversalTime(), value => DateTime.SpecifyKind(value, DateTimeKind.Utc))
        {
        }
    }
}
