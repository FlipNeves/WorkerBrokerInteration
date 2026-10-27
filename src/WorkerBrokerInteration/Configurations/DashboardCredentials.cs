namespace WorkerBrokerInteration.Configurations;

public sealed class DashboardCredentials
{
    public const string Section = "HangfireDashboard";

    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
