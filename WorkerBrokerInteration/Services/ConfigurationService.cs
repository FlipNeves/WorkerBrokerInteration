using WorkerBrokerInteration.Repository;
using WorkerBrokerInteration.Repository.Interfaces;
using WorkerBrokerInteration.Services.Interfaces;

namespace WorkerBrokerInteration.Services
{
    public class ConfigurationsService : IConfigurationsService
    {
        private readonly IConfigurationsRepository _configurationsRepository;

        public ConfigurationsService(IConfigurationsRepository configurationsRepository)
        {
            _configurationsRepository = configurationsRepository;
        }

        public string GetConfigurationValue(string configurationKey)
        {
            return _configurationsRepository.GetConfiguration(configurationKey);
        }

        public string SetConfigurationValue(string configurationKey, string newValue)
        {
            return _configurationsRepository.SetConfiguration(configurationKey, newValue);
        }

        public string GetCronJobSampleJobExample()
        {
            var cronString = GetConfigurationValue(ConfigurationsKeys.CronJobSampleJobExample);
            return cronString;
        }

        public bool JobStatus(string recurringJobId)
        {
            return true;
            // recurringJobId.ToUpper() will match the values listed in the "enum" Configurations
            var jobStatusString = GetConfigurationValue(recurringJobId.ToUpper());
            if (bool.TryParse(jobStatusString, out bool jobStatus))
                return jobStatus;

            return false;
        }
    }
}
