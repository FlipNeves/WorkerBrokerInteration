namespace WorkerBrokerInteration.Repository.Interfaces
{
    public interface IConfigurationsRepository
    {
        string GetConfiguration(string configurationKey);
        string SetConfiguration(string configurationKey, string newValue);
    }
}