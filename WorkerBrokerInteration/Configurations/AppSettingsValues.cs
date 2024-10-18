namespace WorkerBrokerInteration.Configurations
{
    public class ConfigHangFire
    {
        public int WorkerCount { get; set; }
        public int SemaphoreTimeOutInMinutes { get; set; }
        public HangFireDashboardAuth? DashboardAuth { get; set; }
    }

    public class HangFireDashboardAuth
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
    }
}
