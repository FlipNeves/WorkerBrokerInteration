namespace WorkerBrokerInteration.Services.Interfaces
{
    public interface IConfigurationsService
    {
        string GetConfigurationValue(string configurationKey);
        string SetConfigurationValue(string configurationKey, string newValue);
        string GetCronJobSampleJobExample();
        bool JobStatus(string recurringJobId);
    }
}
