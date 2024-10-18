namespace WorkerBrokerInteration.Services.Interfaces
{
    public interface IJobSchedulerService
    {
        Task Execute();
        Task AddOrUpdateJob(Action configureJob);
    }
}
