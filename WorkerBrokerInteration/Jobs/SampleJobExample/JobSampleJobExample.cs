using WorkerBrokerInteration.Services.Interfaces;

namespace WorkerBrokerInteration.Jobs.SampleJobExample
{
    public class JobSampleJobExample : ISimpleJob
    {
        public async Task Execute()
        {
			await Task.Delay(millisecondsDelay: 1500000);
			return;
            //Codes of job here
        }
    }
}
