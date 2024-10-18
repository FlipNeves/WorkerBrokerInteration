using WorkerBrokerInteration.Repository.Interfaces;

namespace WorkerBrokerInteration.Repository
{
    public class ConfigurationsRepository : IConfigurationsRepository
    {
        private readonly Dictionary<string, string> _configurations;

        public ConfigurationsRepository()
        {
            //With Db communication, this is wont be necessary. But just to see how the things goes, the values are here.
            //Remember: Here I`m talking about a table of configurations with 2 collumns, key and value.
            _configurations = new Dictionary<string, string>
            {
                { ConfigurationsKeys.CronJobSampleJobExample, "0 0 * * *" }, 
                { ConfigurationsKeys.JobStatusSampleJobExample, "true" }
            };
        }

        public string GetConfiguration(string configurationKey)
        {
            return _configurations.ContainsKey(configurationKey) ? _configurations[configurationKey] : string.Empty;
        }

        public string SetConfiguration(string configurationKey, string newValue)
        {
            _configurations[configurationKey] = newValue;
            return _configurations[configurationKey];
        }
    }

    //Find a place to put this, where it`s make more sense on the strutcture of your project.
    public static class ConfigurationsKeys
    {
        public const string CronJobSampleJobExample = "CronJobSampleJobExample";
        public const string JobStatusSampleJobExample = "JobStatusSampleJobExample";
    }
}
